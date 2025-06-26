using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace FileStorageSystem
{
    // Manages the file system operations and user interactions.
    public class FileManager
    {
        // Private fields for the root folder, current folder, history logger, and base directory.
        private Folder _rootFolder;
        private Folder _currentFolder;
        private HistoryLogger _historyLogger;
        private string _baseDirectory;

        // Public properties for accessing the root folder, current folder, and history logger.
        public Folder RootFolder
        {
            get { return _rootFolder; }
            set { _rootFolder = value; }
        }

        public Folder CurrentFolder
        {
            get { return _currentFolder; }
            set { _currentFolder = value; }
        }

        public HistoryLogger Historylogger
        {
            get { return _historyLogger; }
            set { _historyLogger = value; }
        }

        // Constructor initializes the file system and loads the physical file structure.
        public FileManager()
        {
            _baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage"); // Set the base directory.
            if (!Directory.Exists(_baseDirectory)) // Create the base directory if it doesn't exist.
            {
                Directory.CreateDirectory(_baseDirectory);
            }
            RootFolder = new Folder("root", null); // Create the root folder with no parent.
            CurrentFolder = RootFolder; // Set the current folder to root.
            _historyLogger = new HistoryLogger(); // Initialize the history logger.
            LoadPhysicalFileSystem(RootFolder, Path.Combine(_baseDirectory, "root")); // Load existing files and folders.
            Console.WriteLine("File System Initialized. Type 'help' for commands."); // Display startup message.
        }

        // Loads the physical file system into the in-memory structure.
        public void LoadPhysicalFileSystem(Folder folder, string physicalPath)
        {
            try
            {
                if (!Directory.Exists(physicalPath)) // Create the directory if it doesn't exist.
                {
                    Directory.CreateDirectory(physicalPath);
                }

                foreach (var dir in Directory.GetDirectories(physicalPath)) // Iterate through subdirectories.
                {
                    string dirName = Path.GetFileName(dir); // Get the directory name.
                    if (folder.GetEntity(dirName) == null) // Check if the folder doesn't already exist.
                    {
                        Folder newFolder = new Folder(dirName, folder); // Create a new folder object.
                        folder.AddEntity(newFolder); // Add to the parent folder.
                        LoadPhysicalFileSystem(newFolder, Path.Combine(physicalPath, dirName)); // Recursively load subfolders.
                    }
                }

                foreach (var file in Directory.GetFiles(physicalPath)) // Iterate through files.
                {
                    string fileName = Path.GetFileName(file); // Get the file name.
                    if (folder.GetEntity(fileName) == null) // Check if the file doesn't already exist.
                    {
                        string content = System.IO.File.ReadAllText(file); // Read file content.
                        File newFile = new File(fileName, folder, content); // Create a new file object.
                        folder.AddEntity(newFile); // Add to the parent folder.
                        newFile.CreationDate = System.IO.File.GetCreationTime(file); // Set file creation time.
                        newFile.LastModifiedDate = System.IO.File.GetLastWriteTime(file); // Set last modified time.
                        newFile.LastAccessedDate = System.IO.File.GetLastAccessTime(file); // Set last accessed time.
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading physical file system: {ex.Message}"); // Log any errors.
            }
        }

        // Resolves a path to a file system entity (file or folder).
        public FileSystemEntity GetEntityFromPath(string path)
        {
            Folder startingFolder = CurrentFolder; // Start from the current folder.
            if (path.StartsWith("/")) // If the path is absolute, start from the root.
            {
                startingFolder = RootFolder;
                path = path.Substring(1); // Remove the leading slash.
            }

            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries); // Split the path into components.
            FileSystemEntity currentEntity = startingFolder; // Start with the starting folder.

            if (pathParts.Length == 0 && path == "/") return RootFolder; // Return root for "/" path.

            foreach (string part in pathParts) // Traverse the path components.
            {
                if (currentEntity is Folder folder) // Ensure the current entity is a folder.
                {
                    if (part == ".") continue; // Stay in the current folder.
                    if (part == "..") // Move to the parent folder.
                    {
                        if (folder.ParentFolder != null)
                        {
                            currentEntity = folder.ParentFolder;
                        }
                        else
                        {
                            return null; // No parent folder (e.g., at root).
                        }
                    }
                    else
                    {
                        currentEntity = folder.GetEntity(part); // Get the entity by name.
                        if (currentEntity == null)
                        {
                            return null; // Entity not found.
                        }
                    }
                }
                else
                {
                    return null; // Path points to a file, not a folder.
                }
            }
            return currentEntity; // Return the resolved entity.
        }

        // Gets the parent folder from a path.
        public Folder GetParentFolderFromPath(string path)
        {
            string directoryPath = Path.GetDirectoryName(path); // Get the parent directory path.
            if (string.IsNullOrEmpty(directoryPath))
            {
                return CurrentFolder; // If no parent path, use current folder.
            }
            return GetEntityFromPath(directoryPath) as Folder; // Resolve the parent folder.
        }

        // Extracts the entity name from a path.
        public string GetEntityNameFromPath(string path)
        {
            return Path.GetFileName(path); // Return the last component of the path.
        }

        // Creates a new file with optional content.
        public void CreateFile(string path, string content = "")
        {
            try
            {
                Folder parent = GetParentFolderFromPath(path); // Get the parent folder.
                if (parent == null) { Console.WriteLine("Error: Parent directory not found."); return; }

                string fileName = GetEntityNameFromPath(path); // Get the file name.
                if (string.IsNullOrEmpty(fileName)) { Console.WriteLine("Error: File name cannot be empty."); return; }

                if (parent.GetEntity(fileName) != null) // Check for name conflicts.
                {
                    Console.WriteLine($"Error: File '{fileName}' already exists in '{parent.GetFullPath()}'.");
                    return;
                }

                File newFile = new File(fileName, parent, content); // Create a new file.
                parent.AddEntity(newFile); // Add to the parent folder.
                Console.WriteLine($"File '{newFile.Name}' created in '{parent.GetFullPath()}'."); // Log creation.
                _historyLogger.LogAccess(newFile.GetFullPath(), DateTime.Now); // Log file access.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Log any errors.
            }
        }

        // Creates a new folder.
        public void CreateFolder(string path)
        {
            try
            {
                Folder parent = GetParentFolderFromPath(path); // Get the parent folder.
                if (parent == null) { Console.WriteLine("Error: Parent directory not found."); return; }

                string folderName = GetEntityNameFromPath(path); // Get the folder name.
                if (string.IsNullOrEmpty(folderName)) { Console.WriteLine("Error: Folder name cannot be empty."); return; }

                if (parent.GetEntity(folderName) != null) // Check for name conflicts.
                {
                    Console.WriteLine($"Error: Folder '{folderName}' already exists in '{parent.GetFullPath()}'.");
                    return;
                }

                Folder newFolder = new Folder(folderName, parent); // Create a new folder.
                parent.AddEntity(newFolder); // Add to the parent folder.
                Console.WriteLine($"Folder '{newFolder.Name}' created in '{parent.GetFullPath()}'."); // Log creation.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Log any errors.
            }
        }

        // Deletes a file or folder.
        public void DeleteEntity(string path)
        {
            try
            {
                FileSystemEntity entityToDelete = GetEntityFromPath(path); // Resolve the entity.
                if (entityToDelete == null) { Console.WriteLine("Error: File or folder not found."); return; }

                if (entityToDelete == RootFolder) // Prevent deletion of the root folder.
                {
                    Console.WriteLine("Error: Cannot delete the root folder.");
                    return;
                }

                if (entityToDelete.ParentFolder != null) // Ensure the entity has a parent.
                {
                    entityToDelete.ParentFolder.RemoveEntity(entityToDelete); // Remove from parent folder.
                    Console.WriteLine($"'{entityToDelete.Name}' deleted."); // Log deletion.
                }
                else
                {
                    Console.WriteLine("Error: Cannot delete entity without a parent folder (should not happen for non-root).");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Log any errors.
            }
        }

        // Renames a file or folder.
        public void RenameEntity(string path, string newName)
        {
            try
            {
                FileSystemEntity entity = GetEntityFromPath(path); // Resolve the entity.
                if (entity == null) { Console.WriteLine("Error: File or folder not found."); return; }

                if (entity == RootFolder) // Prevent renaming the root folder.
                {
                    Console.WriteLine("Error: Cannot rename the root folder.");
                    return;
                }

                string oldPath = entity.GetFullPath(); // Get the current path.
                string oldName = entity.Name; // Store the old name.
                entity.Rename(newName); // Rename the entity.
                string newPath = entity.GetFullPath(); // Get the new path.

                if (entity is Folder && Directory.Exists(oldPath)) // Update the physical folder if it exists.
                {
                    Directory.Move(oldPath, newPath);
                }
                else if (entity is File && System.IO.File.Exists(oldPath)) // Update the physical file if it exists.
                {
                    System.IO.File.Move(oldPath, newPath);
                }
                Console.WriteLine($"'{oldName}' renamed to '{newName}'."); // Log the rename action.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Log any errors.
            }
        }

        // Moves a file or folder to a new location.
        public void MoveEntity(string sourcePath, string destinationPath)
        {
            try
            {
                FileSystemEntity sourceEntity = GetEntityFromPath(sourcePath); // Resolve the source entity.
                if (sourceEntity == null) { Console.WriteLine("Error: Source file or folder not found."); return; }

                if (sourceEntity == RootFolder) // Prevent moving the root folder.
                {
                    Console.WriteLine("Error: Cannot move the root folder.");
                    return;
                }

                Folder destinationFolder = GetEntityFromPath(destinationPath) as Folder; // Resolve the destination folder.
                if (destinationFolder == null) { Console.WriteLine("Error: Destination folder not found or is a file."); return; }

                if (destinationFolder.GetEntity(sourceEntity.Name) != null) // Check for name conflicts.
                {
                    Console.WriteLine($"Error: An entity named '{sourceEntity.Name}' already exists in the destination folder.");
                    return;
                }

                string sourceFullPath = sourceEntity.GetFullPath(); // Get the source path.
                string destinationFullPath = Path.Combine(destinationFolder.GetFullPath(), sourceEntity.Name); // Build the destination path.

                if (sourceEntity is Folder && Directory.Exists(sourceFullPath)) // Move the physical folder.
                {
                    Directory.Move(sourceFullPath, destinationFullPath);
                }
                else if (sourceEntity is File && System.IO.File.Exists(sourceFullPath)) // Move the physical file.
                {
                    System.IO.File.Move(sourceFullPath, destinationFullPath);
                }

                sourceEntity.ParentFolder.RemoveEntity(sourceEntity); // Remove from the source folder.
                destinationFolder.AddEntity(sourceEntity); // Add to the destination folder.

                Console.WriteLine($"Moved '{sourceEntity.Name}' from '{sourceEntity.ParentFolder.GetFullPath()}' to '{destinationFolder.GetFullPath()}'."); // Log the move action.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving entity: {ex.Message}"); // Log any errors.
            }
        }

        // Copies a file or folder to a new location.
        public void CopyEntity(string sourcePath, string destinationPath)
        {
            try
            {
                FileSystemEntity sourceEntity = GetEntityFromPath(sourcePath); // Resolve the source entity.
                if (sourceEntity == null) { Console.WriteLine("Error: Source file or folder not found."); return; }

                Folder destinationFolder = GetEntityFromPath(destinationPath) as Folder; // Resolve the destination folder.
                if (destinationFolder == null) { Console.WriteLine("Error: Destination folder not found or is a file."); return; }

                if (destinationFolder.GetEntity(sourceEntity.Name) != null) // Check for name conflicts.
                {
                    Console.WriteLine($"Error: An entity named '{sourceEntity.Name}' already exists in the destination folder.");
                    return;
                }

                if (sourceEntity is File sourceFile) // Copy a file.
                {
                    File newFile = new File(sourceFile.Name, destinationFolder, sourceFile.Content); // Create a new file copy.
                    destinationFolder.AddEntity(newFile); // Add to the destination folder.
                    string sourceFullPath = sourceFile.GetFullPath(); // Get the source path.
                    string destinationFullPath = Path.Combine(destinationFolder.GetFullPath(), sourceFile.Name); // Build the destination path.
                    if (System.IO.File.Exists(sourceFullPath))
                    {
                        System.IO.File.Copy(sourceFullPath, destinationFullPath); // Copy the physical file.
                    }
                    Console.WriteLine($"Copied file '{sourceFile.Name}' to '{destinationFolder.GetFullPath()}'."); // Log the copy action.
                }
                else if (sourceEntity is Folder sourceFolder) // Copy a folder.
                {
                    Folder newFolder = new Folder(sourceFolder.Name, destinationFolder); // Create a new folder.
                    destinationFolder.AddEntity(newFolder); // Add to the destination folder.
                    CopyFolderContents(sourceFolder, newFolder); // Recursively copy contents.
                    Console.WriteLine($"Copied folder '{sourceFolder.Name}' to '{destinationFolder.GetFullPath()}'."); // Log the copy action.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying entity: {ex.Message}"); // Log any errors.
            }
        }

        // Recursively copies the contents of a folder.
        public void CopyFolderContents(Folder source, Folder destination)
        {
            foreach (var entity in source.Contents) // Iterate through source folder contents.
            {
                if (entity is File file) // Copy a file.
                {
                    File newFile = new File(file.Name, destination, file.Content); // Create a new file copy.
                    destination.AddEntity(newFile); // Add to the destination folder.
                    string sourcePath = file.GetFullPath(); // Get the source path.
                    string destPath = newFile.GetFullPath(); // Get the destination path.
                    if (System.IO.File.Exists(sourcePath))
                    {
                        System.IO.File.Copy(sourcePath, destPath); // Copy the physical file.
                    }
                }
                else if (entity is Folder folder) // Copy a subfolder.
                {
                    Folder newSubFolder = new Folder(folder.Name, destination); // Create a new subfolder.
                    destination.AddEntity(newSubFolder); // Add to the destination folder.
                    CopyFolderContents(folder, newSubFolder); // Recursively copy subfolder contents.
                }
            }
        }

        // Navigates to a specified folder.
        public void NavigateTo(string path)
        {
            try
            {
                FileSystemEntity targetEntity = GetEntityFromPath(path); // Resolve the target entity.
                if (targetEntity == null) { Console.WriteLine("Error: Path not found."); return; }

                if (targetEntity is Folder targetFolder) // Ensure the target is a folder.
                {
                    CurrentFolder = targetFolder; // Update the current folder.
                    Console.WriteLine($"Changed directory to '{CurrentFolder.GetFullPath()}'."); // Log the navigation.
                }
                else
                {
                    Console.WriteLine($"Error: '{path}' is a file, not a folder."); // Handle invalid navigation.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Log any errors.
            }
        }

        // Lists the contents of a folder.
        public void ListContents(string path = ".", bool detailed = false)
        {
            try
            {
                FileSystemEntity targetEntity = GetEntityFromPath(path); // Resolve the target entity.
                if (targetEntity == null) { Console.WriteLine("Error: Path not found."); return; }

                if (targetEntity is Folder targetFolder) // Ensure the target is a folder.
                {
                    targetFolder.ListContents(detailed); // List the folder's contents.
                }
                else
                {
                    Console.WriteLine($"Error: '{path}' is a file, not a folder. Use 'cat' to view file content."); // Handle invalid target.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Log any errors.
            }
        }

        // Reads and displays the content of a file.
        public void ReadFileContent(string path)
        {
            try
            {
                FileSystemEntity entity = GetEntityFromPath(path); // Resolve the entity.
                if (entity == null) { Console.WriteLine("Error: File not found."); return; }

                if (entity is File file) // Ensure the entity is a file.
                {
                    _historyLogger.LogAccess(file.GetFullPath(), DateTime.Now); // Log file access.
                    file.View(); // Display the file's content.
                }
                else
                {
                    Console.WriteLine($"Error: '{path}' is a folder, not a file."); // Handle invalid target.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Log any errors.
            }
        }

        // Edits the content of a file using the text editor.
        public void EditFileContent(string path)
        {
            try
            {
                FileSystemEntity entity = GetEntityFromPath(path); // Resolve the entity.
                if (entity == null) { Console.WriteLine("Error: File not found."); return; }

                if (entity is File file) // Ensure the entity is a file.
                {
                    string updatedContent = TextEditor.EditText(file.Content); // Edit the content interactively.
                    file.Edit(updatedContent); // Update the file's content.
                    _historyLogger.LogAccess(file.GetFullPath(), DateTime.Now); // Log file access.
                }
                else
                {
                    Console.WriteLine($"Error: '{path}' is a folder, not a file."); // Handle invalid target.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Log any errors.
            }
        }

        // Searches for files and folders matching a term.
        public void SearchFiles(string searchTerm)
        {
            Console.WriteLine($"\n--- Search Results for '{searchTerm}' ---"); // Display search header.
            List<FileSystemEntity> foundEntities = SearchRecursive(RootFolder, searchTerm); // Perform recursive search.

            if (!foundEntities.Any()) // Check if any results were found.
            {
                Console.WriteLine("No files or folders found matching the search term.");
                return;
            }

            foreach (var entity in foundEntities.OrderBy(e => e.GetFullPath())) // Display sorted results.
            {
                Console.WriteLine($"  {(entity is Folder ? "DIR" : "FIL")}: {entity.GetFullPath()}"); // Show entity type and path.
            }
            Console.WriteLine("------------------------------------------"); // Display footer.
        }

        // Recursively searches for entities matching the search term.
        public List<FileSystemEntity> SearchRecursive(Folder folder, string searchTerm)
        {
            List<FileSystemEntity> results = new List<FileSystemEntity>(); // Initialize results list.

            foreach (var entity in folder.Contents) // Iterate through folder contents.
            {
                if (entity.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) // Check if name matches.
                {
                    results.Add(entity); // Add matching entity to results.
                }

                if (entity is File file && file.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) // Check file content.
                {
                    if (!results.Contains(file)) // Avoid duplicates.
                    {
                        results.Add(file);
                    }
                }
                else if (entity is Folder subFolder) // Recursively search subfolders.
                {
                    results.AddRange(SearchRecursive(subFolder, searchTerm));
                }
            }
            return results; // Return all matching entities.
        }

        // Displays the file access history.
        public void DisplayHistory()
        {
            Console.WriteLine("\n--- File Access History ---"); // Display header.
            List<string> history = _historyLogger.GetHistory(); // Get the history log.
            if (!history.Any()) // Check if the history is empty.
            {
                Console.WriteLine("  (No access history found)");
            }
            else
            {
                foreach (var entry in history) // Display each history entry.
                {
                    Console.WriteLine($"  {entry}");
                }
            }
            Console.WriteLine("--------------------------"); // Display footer.
        }

        // Displays the list of available commands.
        public void DisplayHelp()
        {
            Console.WriteLine("\n--- Available Commands ---");
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
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");
        }

        // Prints the current working directory.
        public void PrintWorkingDirectory()
        {
            Console.WriteLine($"Current directory: {CurrentFolder.GetFullPath()}"); // Display the current folder's path.
        }
    }
}