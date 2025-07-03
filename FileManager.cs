using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

// Namespace to organize the file storage system-related classes
namespace FileStorageSystem
{
    // Class to manage the file system operations
    public class FileManager
    {
        // Private fields for the root folder, current folder, history logger, and base directory
        private Folder _rootFolder;       // Root folder of the file system
        private Folder _currentFolder;    // Current working folder
        private HistoryLogger _historyLogger; // Logger for file access history
        private string _baseDirectory;    // Base directory for the file system

        // Public property for the root folder
        public Folder RootFolder
        {
            // Getter returns the private root folder
            get { return _rootFolder; }
            // Setter updates the private root folder
            set { _rootFolder = value; }
        }

        // Public property for the current folder
        public Folder CurrentFolder
        {
            // Getter returns the private current folder
            get { return _currentFolder; }
            // Setter updates the private current folder
            set { _currentFolder = value; }
        }

        // Public property for the history logger
        public HistoryLogger Historylogger
        {
            // Getter returns the private history logger
            get { return _historyLogger; }
            // Setter updates the private history logger
            set { _historyLogger = value; }
        }

        // Constructor for FileManager
        public FileManager()
        {
            // Set the base directory for the file system
            _baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage");

            // If the base directory does not exist, create it
            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
            }

            // Initialize the root folder with the name "root" and no parent
            RootFolder = new Folder("root", null);

            // Set the current folder to the root folder
            CurrentFolder = RootFolder;

            // Initialize the history logger
            _historyLogger = new HistoryLogger();

            // Load the physical file system into the root folder
            LoadPhysicalFileSystem(RootFolder, Path.Combine(_baseDirectory, "root"));

            // Print a message indicating the file system is initialized
            Console.WriteLine("File System Initialized. Type 'help' for commands.");
        }

        // Method to load the physical file system into the in-memory structure
        public void LoadPhysicalFileSystem(Folder folder, string physicalPath)
        {
            try
            {
                // If the physical directory does not exist, create it
                if (!Directory.Exists(physicalPath))
                {
                    Directory.CreateDirectory(physicalPath);
                }

                // Iterate through all subdirectories in the physical path
                foreach (var dir in Directory.GetDirectories(physicalPath))
                {
                    // Get the directory name from the path
                    string dirName = Path.GetFileName(dir);
                    // Check if the folder does not already exist in the in-memory structure
                    if (folder.GetEntity(dirName) == null)
                    {
                        // Create a new folder in the in-memory structure
                        Folder newFolder = new Folder(dirName, folder);
                        // Add the new folder to the parent folder's contents
                        folder.AddEntity(newFolder);
                        // Recursively load the subdirectory
                        LoadPhysicalFileSystem(newFolder, Path.Combine(physicalPath, dirName));
                    }
                }

                // Iterate through all files in the physical path
                foreach (var file in Directory.GetFiles(physicalPath))
                {
                    // Get the file name from the path
                    string fileName = Path.GetFileName(file);

                    // Check if the file does not already exist in the in-memory structure
                    if (folder.GetEntity(fileName) == null)
                    {
                        // Read the file's content
                        string content = System.IO.File.ReadAllText(file);

                        // Create a new file in the in-memory structure
                        File newFile = new File(fileName, folder, content);
                        // Add the new file to the parent folder's contents
                        folder.AddEntity(newFile);

                        // Set the file's creation date from the physical file
                        newFile.CreationDate = System.IO.File.GetCreationTime(file);
                        // Set the file's last modified date from the physical file
                        newFile.LastModifiedDate = System.IO.File.GetLastWriteTime(file);
                        // Set the file's last accessed date from the physical file
                        newFile.LastAccessedDate = System.IO.File.GetLastAccessTime(file);
                    }
                }
            }
            catch (Exception ex)
            {
                // Print an error message if loading the file system fails
                Console.WriteLine($"Error loading physical file system: {ex.Message}");
            }
        }

        // Method to retrieve an entity (file or folder) from a given path
        public FileSystemEntity GetEntityFromPath(string path)
        {
            // Start from the current folder
            Folder startingFolder = CurrentFolder;

            // If the path starts with '/', start from the root folder
            if (path.StartsWith("/"))
            {
                startingFolder = RootFolder;
                // Remove the leading '/' from the path
                path = path.Substring(1);
            }

            // Split the path into parts, removing empty entries
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Start with the starting folder as the current entity
            FileSystemEntity currentEntity = startingFolder;

            // If the path is just '/', return the root folder
            if (pathParts.Length == 0 && path == "/")
            {
                return RootFolder;
            }

            // Iterate through each part of the path
            foreach (string part in pathParts)
            {
                // Check if the current entity is a folder
                if (currentEntity is Folder folder)
                {
                    // If the part is '.', skip to the next part (current directory)
                    if (part == ".")
                    {
                        continue;
                    }

                    // If the part is '..', move to the parent folder if it exists
                    if (part == "..")
                    {
                        if (folder.ParentFolder != null)
                        {
                            currentEntity = folder.ParentFolder;
                        }
                        else
                        {
                            // Return null if there is no parent folder
                            return null;
                        }
                    }
                    else
                    {
                        // Look for an entity with the given name in the current folder
                        currentEntity = folder.GetEntity(part);

                        // If no entity is found, return null
                        if (currentEntity == null)
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    // Return null if the current entity is not a folder
                    return null;
                }
            }
            // Return the found entity
            return currentEntity;
        }

        // Method to get the parent folder from a given path
        public Folder GetParentFolderFromPath(string path)
        {
            // Get the directory part of the path
            string directoryPath = Path.GetDirectoryName(path);

            // If the directory path is empty, return the current folder
            if (string.IsNullOrEmpty(directoryPath))
            {
                return CurrentFolder;
            }

            // Return the folder corresponding to the directory path
            return GetEntityFromPath(directoryPath) as Folder;
        }

        // Method to get the entity name from a given path
        public string GetEntityNameFromPath(string path)
        {
            // Return the file or folder name from the path
            return Path.GetFileName(path);
        }

        // Method to create a new file
        public void CreateFile(string path, string content = "")
        {
            try
            {
                // Get the parent folder from the path
                Folder parent = GetParentFolderFromPath(path);

                // If the parent folder is not found, print an error
                if (parent == null)
                {
                    Console.WriteLine("Error: Parent directory not found.");
                    return;
                }

                // Get the file name from the path
                string fileName = GetEntityNameFromPath(path);

                // If the file name is empty, print an error
                if (string.IsNullOrEmpty(fileName))
                {
                    Console.WriteLine("Error: File name cannot be empty.");
                    return;
                }

                // Check if an entity with the same name already exists
                if (parent.GetEntity(fileName) != null)
                {
                    Console.WriteLine($"Error: File '{fileName}' already exists in '{parent.GetFullPath()}'.");
                    return;
                }

                // Create a new file with the specified name and content
                File newFile = new File(fileName, parent, content);
                // Add the file to the parent folder
                parent.AddEntity(newFile);

                // Print a confirmation message
                Console.WriteLine($"File '{newFile.Name}' created in '{parent.GetFullPath()}'.");

                // Log the file creation event
                _historyLogger.LogAccess(newFile.GetFullPath(), DateTime.Now);
            }
            catch (Exception ex)
            {
                // Print an error message if file creation fails
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Method to create a new folder
        public void CreateFolder(string path)
        {
            try
            {
                // Get the parent folder from the path
                Folder parent = GetParentFolderFromPath(path);

                // If the parent folder is not found, print an error
                if (parent == null)
                {
                    Console.WriteLine("Error: Parent directory not found.");
                    return;
                }

                // Get the folder name from the path
                string folderName = GetEntityNameFromPath(path);

                // If the folder name is empty, print an error
                if (string.IsNullOrEmpty(folderName))
                {
                    Console.WriteLine("Error: Folder name cannot be empty.");
                    return;
                }

                // Check if an entity with the same name already exists
                if (parent.GetEntity(folderName) != null)
                {
                    Console.WriteLine($"Error: Folder '{folderName}' already exists in '{parent.GetFullPath()}'.");
                    return;
                }

                // Create a new folder with the specified name
                Folder newFolder = new Folder(folderName, parent);
                // Add the folder to the parent folder
                parent.AddEntity(newFolder);

                // Print a confirmation message
                Console.WriteLine($"Folder '{newFolder.Name}' created in '{parent.GetFullPath()}'.");
            }
            catch (Exception ex)
            {
                // Print an error message if folder creation fails
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Method to delete a file or folder
        public void DeleteEntity(string path)
        {
            try
            {
                // Get the entity from the path
                FileSystemEntity entityToDelete = GetEntityFromPath(path);

                // If the entity is not found, print an error
                if (entityToDelete == null)
                {
                    Console.WriteLine("Error: File or folder not found.");
                    return;
                }

                // If the entity is the root folder, print an error
                if (entityToDelete == RootFolder)
                {
                    Console.WriteLine("Error: Cannot delete the root folder.");
                    return;
                }

                // If the entity has a parent folder, remove it
                if (entityToDelete.ParentFolder != null)
                {
                    entityToDelete.ParentFolder.RemoveEntity(entityToDelete);
                    // Print a confirmation message
                    Console.WriteLine($"'{entityToDelete.Name}' deleted.");
                }
                else
                {
                    // Print an error if the entity has no parent (should not happen for non-root)
                    Console.WriteLine("Error: Cannot delete entity without a parent folder (should not happen for non-root).");
                }
            }
            catch (Exception ex)
            {
                // Print an error message if deletion fails
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Method to rename a file or folder
        public void RenameEntity(string path, string newName)
        {
            try
            {
                // Get the entity from the path
                FileSystemEntity entity = GetEntityFromPath(path);
                // If the entity is not found, print an error
                if (entity == null)
                {
                    Console.WriteLine("Error: File or folder not found.");
                    return;
                }

                // If the entity is the root folder, print an error
                if (entity == RootFolder)
                {
                    Console.WriteLine("Error: Cannot rename the root folder.");
                    return;
                }

                // Store the old path and name for physical file system operations
                string oldPath = entity.GetFullPath();
                string oldName = entity.Name;
                // Rename the entity
                entity.Rename(newName);

                // Get the new path after renaming
                string newPath = entity.GetFullPath();

                // If the entity is a folder and exists in the physical file system, move it
                if (entity is Folder && Directory.Exists(oldPath))
                {
                    Directory.Move(oldPath, newPath);
                }
                // If the entity is a file and exists in the physical file system, move it
                else if (entity is File && System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Move(oldPath, newPath);
                }
                // Print a confirmation message
                Console.WriteLine($"'{oldName}' renamed to '{newName}'.");
            }
            catch (Exception ex)
            {
                // Print an error message if renaming fails
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Method to move a file or folder
        public void MoveEntity(string sourcePath, string destinationPath)
        {
            try
            {
                // Get the source entity from the path
                FileSystemEntity sourceEntity = GetEntityFromPath(sourcePath);

                // If the source entity is not found, print an error
                if (sourceEntity == null)
                {
                    Console.WriteLine("Error: Source file or folder not found.");
                    return;
                }

                // If the source is the root folder, print an error
                if (sourceEntity == RootFolder)
                {
                    Console.WriteLine("Error: Cannot move the root folder.");
                    return;
                }

                // Get the destination folder from the path
                Folder destinationFolder = GetEntityFromPath(destinationPath) as Folder;

                // If the destination folder is not found or is not a folder, print an error
                if (destinationFolder == null)
                {
                    Console.WriteLine("Error: Destination folder not found or is a file.");
                    return;
                }

                // Check if an entity with the same name already exists in the destination
                if (destinationFolder.GetEntity(sourceEntity.Name) != null)
                {
                    Console.WriteLine($"Error: An entity named '{sourceEntity.Name}' already exists in the destination folder.");
                    return;
                }

                // Get the full paths for the source and destination
                string sourceFullPath = sourceEntity.GetFullPath();
                string destinationFullPath = Path.Combine(destinationFolder.GetFullPath(), sourceEntity.Name);

                // If the source is a folder and exists, move it in the physical file system
                if (sourceEntity is Folder && Directory.Exists(sourceFullPath))
                {
                    Directory.Move(sourceFullPath, destinationFullPath);
                }
                // If the source is a file and exists, move it in the physical file system
                else if (sourceEntity is File && System.IO.File.Exists(sourceFullPath))
                {
                    System.IO.File.Move(sourceFullPath, destinationFullPath);
                }

                // Remove the entity from its current parent folder
                sourceEntity.ParentFolder.RemoveEntity(sourceEntity);
                // Add the entity to the destination folder
                destinationFolder.AddEntity(sourceEntity);

                // Print a confirmation message
                Console.WriteLine($"Moved '{sourceEntity.Name}' from '{sourceEntity.ParentFolder.GetFullPath()}' to '{destinationFolder.GetFullPath()}'.");
            }
            catch (Exception ex)
            {
                // Print an error message if moving fails
                Console.WriteLine($"Error moving entity: {ex.Message}");
            }
        }

        // Method to copy a file to a destination folder
        public void CopyFile(File source, Folder destination)
        {
            try
            {
                // Get the source and destination file paths
                string sourcePath = source.GetFullPath();
                string destPath = Path.Combine(destination.GetFullPath(), source.Name);

                // Check if the source file exists in the physical file system
                if (!System.IO.File.Exists(sourcePath))
                {
                    Console.WriteLine($"Error: Source file '{sourcePath}' does not exist in the physical file system.");
                    return;
                }

                // Check if a file with the same name already exists in the destination
                if (System.IO.File.Exists(destPath))
                {
                    Console.WriteLine($"Error: A file named '{source.Name}' already exists at '{destPath}'.");
                    return;
                }

                // Copy the file in the physical file system
                System.IO.File.Copy(sourcePath, destPath);
                // Create a new file in the in-memory structure
                File newFile = new File(source.Name, destination, source.Content);
                // Add the new file to the destination folder
                destination.AddEntity(newFile);

                // Print a confirmation message
                Console.WriteLine($"Copied file '{source.Name}' to '{destination.GetFullPath()}'.");
            }
            catch (IOException ex)
            {
                // Print an error message if copying fails
                Console.WriteLine($"Error copying file '{source.Name}': {ex.Message} (Source: {source.GetFullPath()}, Destination: {Path.Combine(destination.GetFullPath(), source.Name)})");
            }
        }

        // Method to copy a file or folder
        public void CopyEntity(string sourcePath, string destinationPath)
        {
            try
            {
                // Get the source entity from the path
                FileSystemEntity sourceEntity = GetEntityFromPath(sourcePath);
                // If the source entity is not found, print an error
                if (sourceEntity == null)
                {
                    Console.WriteLine("Error: Source file or folder not found.");
                    return;
                }

                // Get the destination folder from the path
                Folder destinationFolder = GetEntityFromPath(destinationPath) as Folder;

                // If the destination folder is not found or is not a folder, print an error
                if (destinationFolder == null)
                {
                    Console.WriteLine("Error: Destination folder not found or is a file.");
                    return;
                }

                // Check if an entity with the same name already exists in the destination
                if (destinationFolder.GetEntity(sourceEntity.Name) != null)
                {
                    Console.WriteLine($"Error: An entity named '{sourceEntity.Name}' already exists in the destination folder.");
                    return;
                }

                // If the source is a file, copy it
                if (sourceEntity is File sourceFile)
                {
                    CopyFile(sourceFile, destinationFolder);
                }
                // If the source is a folder, copy it
                else if (sourceEntity is Folder sourceFolder)
                {
                    // Create a new folder in the destination
                    Folder newFolder = new Folder(sourceFolder.Name, destinationFolder);
                    // Add the new folder to the destination
                    destinationFolder.AddEntity(newFolder);
                    // Copy the contents of the source folder to the new folder
                    CopyFolderContents(sourceFolder, newFolder);

                    // Print a confirmation message
                    Console.WriteLine($"Copied folder '{sourceFolder.Name}' to '{destinationFolder.GetFullPath()}'.");
                }
            }
            catch (Exception ex)
            {
                // Print an error message if copying fails
                Console.WriteLine($"Error copying entity: {ex.Message}");
            }
        }

        // Method to copy the contents of a folder recursively
        public void CopyFolderContents(Folder source, Folder destination)
        {
            // Iterate through all entities in the source folder
            foreach (var entity in source.Contents)
            {
                // Check if an entity with the same name already exists in the destination
                if (destination.GetEntity(entity.Name) != null)
                {
                    // Print a message and skip the entity
                    Console.WriteLine($"'{entity.Name}' already exists in '{destination.GetFullPath()}'. Skipping.");
                    continue;
                }

                // If the entity is a file, copy it
                if (entity is File file)
                {
                    CopyFile(file, destination);
                }
                // If the entity is a folder, copy it recursively
                else if (entity is Folder folder)
                {
                    try
                    {
                        // Create a new subfolder in the destination
                        Folder newSubFolder = new Folder(folder.Name, destination);
                        // Add the new subfolder to the destination
                        destination.AddEntity(newSubFolder);
                        // Recursively copy the contents of the folder
                        CopyFolderContents(folder, newSubFolder);

                        // Print a confirmation message
                        Console.WriteLine($"Copied folder '{folder.Name}' to '{destination.GetFullPath()}'.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Print an error message for folder creation issues
                        Console.WriteLine($"Error creating folder '{folder.Name}' in '{destination.GetFullPath()}': {ex.Message}");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        // Print an error message for other copying issues
                        Console.WriteLine($"Error copying folder '{folder.Name}': {ex.Message}");
                        continue;
                    }
                }
            }
        }

        // Method to navigate to a specified folder
        public void NavigateTo(string path)
        {
            try
            {
                // Get the entity from the path
                FileSystemEntity targetEntity = GetEntityFromPath(path);

                // If the entity is not found, print an error
                if (targetEntity == null)
                {
                    Console.WriteLine("Error: Path not found.");
                    return;
                }

                // If the entity is a folder, set it as the current folder
                if (targetEntity is Folder targetFolder)
                {
                    CurrentFolder = targetFolder;
                    // Print a confirmation message
                    Console.WriteLine($"Changed directory to '{CurrentFolder.GetFullPath()}'.");
                }
                else
                {
                    // Print an error if the path is a file
                    Console.WriteLine($"Error: '{path}' is a file, not a folder.");
                }
            }
            catch (Exception ex)
            {
                // Print an error message if navigation fails
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Method to list the contents of a folder
        public void ListContents(string path = ".", bool detailed = false)
        {
            try
            {
                // Get the entity from the path
                FileSystemEntity targetEntity = GetEntityFromPath(path);

                // If the entity is not found, print an error
                if (targetEntity == null)
                {
                    Console.WriteLine("Error: Path not found.");
                    return;
                }

                // If the entity is a folder, list its contents
                if (targetEntity is Folder targetFolder)
                {
                    targetFolder.ListContents(detailed);
                }
                else
                {
                    // Print an error if the path is a file
                    Console.WriteLine($"Error: '{path}' is a file, not a folder. Use 'cat' to view file content.");
                }
            }
            catch (Exception ex)
            {
                // Print an error message if listing fails
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Method to read and display a file's content
        public void ReadFileContent(string path)
        {
            try
            {
                // Get the entity from the path
                FileSystemEntity entity = GetEntityFromPath(path);

                // If the entity is not found, print an error
                if (entity == null)
                {
                    Console.WriteLine("Error: File not found.");
                    return;
                }

                // If the entity is a file, display its content
                if (entity is File file)
                {
                    // Log the file access
                    _historyLogger.LogAccess(file.GetFullPath(), DateTime.Now);
                    // Display the file's content
                    file.View();
                }
                else
                {
                    // Print an error if the path is a folder
                    Console.WriteLine($"Error: '{path}' is a folder, not a file.");
                }
            }
            catch (Exception ex)
            {
                // Print an error message if reading fails
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Method to edit a file's content
        public void EditFileContent(string path)
        {
            try
            {
                // Get the entity from the path
                FileSystemEntity entity = GetEntityFromPath(path);
                // If the entity is not found, print an error
                if (entity == null)
                {
                    Console.WriteLine("Error: File not found.");
                    return;
                }

                // If the entity is a file, edit its content
                if (entity is File file)
                {
                    // Open the text editor to edit the file's content
                    string updatedContent = TextEditor.EditText(file.Content);
                    // Update the file's content
                    file.Edit(updatedContent);
                    // Log the file access
                    _historyLogger.LogAccess(file.GetFullPath(), DateTime.Now);
                }
                else
                {
                    // Print an error if the path is a folder
                    Console.WriteLine($"Error: '{path}' is a folder, not a file.");
                }
            }
            catch (Exception ex)
            {
                // Print an error message if editing fails
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Method to search for files and folders by name or content
        public void SearchFiles(string searchTerm)
        {
            // Print a header for search results
            Console.WriteLine($"\n--- Search Results for '{searchTerm}' ---");

            // Perform a recursive search starting from the root folder
            List<FileSystemEntity> foundEntities = SearchRecursive(RootFolder, searchTerm);

            // If no entities are found, print a message
            if (!foundEntities.Any())
            {
                Console.WriteLine("No files or folders found matching the search term.");
                return;
            }

            // Print each found entity, sorted by path
            foreach (var entity in foundEntities.OrderBy(e => e.GetFullPath()))
            {
                // Print whether the entity is a folder (DIR) or file (FIL) and its full path
                Console.WriteLine($"  {(entity is Folder ? "DIR" : "FIL")}: {entity.GetFullPath()}");
            }
            // Print a footer
            Console.WriteLine("------------------------------------------");
        }

        // Recursive method to search for entities by name or content
        public List<FileSystemEntity> SearchRecursive(Folder folder, string searchTerm)
        {
            // Initialize a list to store search results
            List<FileSystemEntity> results = new List<FileSystemEntity>();

            // Iterate through all entities in the folder
            foreach (var entity in folder.Contents)
            {
                // If the entity's name contains the search term (case-insensitive), add it to results
                if (entity.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(entity);
                }

                // If the entity is a file and its content contains the search term, add it to results
                if (entity is File file && file.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    if (!results.Contains(file))
                    {
                        results.Add(file);
                    }
                }
                // If the entity is a folder, recursively search its contents
                else if (entity is Folder subFolder)
                {
                    results.AddRange(SearchRecursive(subFolder, searchTerm));
                }
            }
            // Return the list of found entities
            return results;
        }

        // Method to display the file access history
        public void DisplayHistory()
        {
            // Print a header for the history
            Console.WriteLine("\n--- File Access History ---");

            // Get the history from the logger
            List<string> history = _historyLogger.GetHistory();

            // If the history is empty, print a message
            if (!history.Any())
            {
                Console.WriteLine("  (No access history found)");
            }
            else
            {
                // Print each history entry
                foreach (var entry in history)
                {
                    Console.WriteLine($"  {entry}");
                }
            }
            // Print a footer
            Console.WriteLine("--------------------------");
        }

        // Method to display the help message with available commands
        public void DisplayHelp()
        {
            // Print a header for the help message
            Console.WriteLine("\n--- Available Commands ---");
            // Print the list of commands and their descriptions
            Console.WriteLine("  mkdir <folder_name>                 - Create a new folder.");
            Console.WriteLine("  touch <file_name> [content]         - Create a new file with optional content.");
            Console.WriteLine("  rm <path>                           - Delete a file or folder.");
            Console.WriteLine("  mv <source_path> <destination_path> - Move a file or folder.");
            Console.WriteLine("  cp <source_path> <destination_path> - Copy a file or folder.");
            Console.WriteLine("  rename <path> <new_name>            - Rename a file or folder.");
            Console.WriteLine("  cd <path>                           - Change directory. Use '..' for parent, '.' for current.");
            Console.WriteLine("  ls [-l] <path>                      - List contents of current or specified folder. Use '-l' for detailed view.");
            Console.WriteLine("  cat <file_path>                     - View content of a file.");
            Console.WriteLine("  nano <file_path>                    - Edit content of a file (interactive editor).");
            Console.WriteLine("  search <term>                       - Search files.");
            Console.WriteLine("  history                             - View file access history.");
            Console.WriteLine("  pwd                                 - Print working directory.");
            Console.WriteLine("  help                                - Display this help message.");
            Console.WriteLine("  exit                                - Exit the file system.");
            // Print a footer
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");
        }

        // Method to print the current working directory
        public void PrintWorkingDirectory()
        {
            // Print the full path of the current folder
            Console.WriteLine($"Current directory: {CurrentFolder.GetFullPath()}");
        }
    }
}