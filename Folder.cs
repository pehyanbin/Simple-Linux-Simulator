using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileStorageSystem
{
    // Represents a folder in the file system, inheriting from FileSystemEntity.
    public class Folder : FileSystemEntity
    {
        // List to store the contents of the folder (files and subfolders).
        private List<FileSystemEntity> _contents = new List<FileSystemEntity>();

        // Public property for accessing the folder's contents.
        public List<FileSystemEntity> Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }

        // Constructor creates a folder and ensures its physical directory exists.
        public Folder(string name, Folder parent) : base(name, parent)
        {
            _contents = new List<FileSystemEntity>(); // Initialize the contents list.
            string fullPath = GetFullPath(); // Get the full path of the folder.
            if (!Directory.Exists(fullPath)) // Check if the directory exists on disk.
            {
                Directory.CreateDirectory(fullPath); // Create the directory if it doesn't exist.
                CreationDate = Directory.GetCreationTime(fullPath); // Set creation time from disk.
                LastModifiedDate = Directory.GetLastWriteTime(fullPath); // Set last modified time.
                LastAccessedDate = Directory.GetLastAccessTime(fullPath); // Set last accessed time.
            }
        }

        // Adds a file or folder to the folder's contents.
        public void AddEntity(FileSystemEntity entity)
        {
            // Check for name conflicts in the folder.
            if (Contents.Any(e => e.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"An entity with the name '{entity.Name}' already exists in this folder.");
            }
            Contents.Add(entity); // Add the entity to the contents list.
            entity.ParentFolder = this; // Set the entity's parent to this folder.
            LastModifiedDate = DateTime.Now; // Update the folder's last modified timestamp.
        }

        // Removes a file or folder from the folder's contents.
        public void RemoveEntity(FileSystemEntity entity)
        {
            if (Contents.Remove(entity)) // Remove the entity from the contents list.
            {
                string entityPath = Path.Combine(GetFullPath(), entity.Name); // Get the entity's full path.
                if (entity is Folder) // If the entity is a folder.
                {
                    if (Directory.Exists(entityPath))
                    {
                        Directory.Delete(entityPath, true); // Delete the folder and its contents from disk.
                    }
                }
                else if (entity is File) // If the entity is a file.
                {
                    if (System.IO.File.Exists(entityPath))
                    {
                        System.IO.File.Delete(entityPath); // Delete the file from disk.
                    }
                }
                LastModifiedDate = DateTime.Now; // Update the folder's last modified timestamp.
            }
            else
            {
                throw new InvalidOperationException($"Entity '{entity.Name}' not found in folder '{Name}'.");
            }
        }

        // Retrieves an entity from the folder by name.
        public FileSystemEntity GetEntity(string name)
        {
            return Contents.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        // Calculates the total size of all contents in the folder.
        public override long GetSize()
        {
            long totalSize = 0;
            foreach (var entity in Contents)
            {
                totalSize += entity.GetSize(); // Sum the size of each entity.
            }
            return totalSize;
        }

        // Lists the contents of the folder, with an option for detailed output.
        public void ListContents(bool detailed = false)
        {
            LastAccessedDate = DateTime.Now; // Update the last accessed timestamp.
            Console.WriteLine($"Contents of {GetFullPath()}:"); // Display the folder's path.
            if (!Contents.Any()) // Check if the folder is empty.
            {
                Console.WriteLine("  (Empty)");
                return;
            }

            foreach (var entity in Contents.OrderBy(e => e.Name)) // Sort contents by name.
            {
                if (detailed) // Detailed view includes type, size, and timestamps.
                {
                    string type = entity is Folder ? "DIR" : "FIL"; // Indicate entity type.
                    Console.WriteLine($"  {type} {entity.Name,-20} Size: {entity.GetSize(),-8} Created: {entity.CreationDate.ToShortDateString()} {entity.CreationDate.ToShortTimeString()} Modified: {entity.LastModifiedDate.ToShortDateString()} {entity.LastModifiedDate.ToShortTimeString()} Accessed: {entity.LastAccessedDate.ToShortDateString()} {entity.LastAccessedDate.ToShortTimeString()}");
                }
                else // Simple view shows only names.
                {
                    Console.WriteLine($"  {entity.Name}{(entity is Folder ? "/" : "")}"); // Append "/" for folders.
                }
            }
        }
    }
}