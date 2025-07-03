using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;

// Namespace to organize the file storage system-related classes
namespace FileStorageSystem
{
    // Folder class inherits from FileSystemEntity to represent a directory in the file system
    public class Folder : FileSystemEntity
    {
        // Private list to store the contents (files and folders) of this folder
        private List<FileSystemEntity> _contents = new List<FileSystemEntity>();

        // Public property to access and modify the folder's contents
        public List<FileSystemEntity> Contents
        {
            // Getter returns the private _contents list
            get { return _contents; }
            // Setter assigns a new value to the _contents list
            set { _contents = value; }
        }

        // Constructor for Folder, takes a name and parent folder as parameters
        public Folder(string name, Folder parent) : base(name, parent)
        {
            // Initialize the _contents list to store child entities
            _contents = new List<FileSystemEntity>();

            // Get the full path of this folder using the inherited GetFullPath method
            string fullPath = GetFullPath();

            // Check if the directory does not exist in the physical file system
            if (!Directory.Exists(fullPath))
            {
                // Create the directory at the specified path
                Directory.CreateDirectory(fullPath);
                // Set the creation date of this folder based on the physical directory
                CreationDate = Directory.GetCreationTime(fullPath);
                // Set the last modified date of this folder
                LastModifiedDate = Directory.GetLastWriteTime(fullPath);
                // Set the last accessed date of this folder
                LastAccessedDate = Directory.GetLastAccessTime(fullPath);
            }
        }

        // Method to add a file or folder to this folder's contents
        public void AddEntity(FileSystemEntity entity)
        {
            // Check if an entity with the same name already exists (case-insensitive)
            if (Contents.Any(e => e.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase)))
            {
                // Throw an exception if a duplicate name is found
                throw new InvalidOperationException($"An entity with the name '{entity.Name}' already exists in this folder.");
            }

            // Add the entity to the contents list
            Contents.Add(entity);
            // Set the entity's parent folder to this folder
            entity.ParentFolder = this;
            // Update the last modified date of this folder to the current time
            LastModifiedDate = DateTime.Now;
        }

        // Method to remove a file or folder from this folder's contents
        public void RemoveEntity(FileSystemEntity entity)
        {
            // Attempt to remove the entity from the contents list
            if (Contents.Remove(entity))
            {
                // Construct the full path of the entity to be deleted
                string entityPath = Path.Combine(GetFullPath(), entity.Name);

                // Check if the entity is a folder
                if (entity is Folder)
                {
                    // If the directory exists in the physical file system, delete it recursively
                    if (Directory.Exists(entityPath))
                    {
                        Directory.Delete(entityPath, true);
                    }
                }
                // Check if the entity is a file
                else if (entity is File)
                {
                    // If the file exists in the physical file system, delete it
                    if (System.IO.File.Exists(entityPath))
                    {
                        System.IO.File.Delete(entityPath);
                    }
                }
                // Update the last modified date of this folder to the current time
                LastModifiedDate = DateTime.Now;
            }
            else
            {
                // Throw an exception if the entity was not found in the contents list
                throw new InvalidOperationException($"Entity '{entity.Name}' not found in folder '{Name}'.");
            }
        }

        // Method to retrieve an entity (file or folder) by name
        public FileSystemEntity GetEntity(string name)
        {
            // Return the first entity with a matching name (case-insensitive), or null if not found
            return Contents.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        // Override the abstract GetSize method to calculate the total size of this folder
        public override long GetSize()
        {
            // Initialize a variable to store the total size
            long totalSize = 0;

            // Iterate through all entities in this folder
            foreach (var entity in Contents)
            {
                // Add the size of each entity to the total
                totalSize += entity.GetSize();
            }
            // Return the total size of all contents
            return totalSize;
        }

        // Method to list the contents of this folder, with an optional detailed view
        public void ListContents(bool detailed = false)
        {
            // Update the last accessed date of this folder to the current time
            LastAccessedDate = DateTime.Now;

            // Print the full path of this folder
            Console.WriteLine($"Contents of {GetFullPath()}:");

            // Check if the folder is empty
            if (!Contents.Any())
            {
                // Print a message indicating the folder is empty
                Console.WriteLine("  (Empty)");
                return;
            }

            // Iterate through the contents, sorted by name
            foreach (var entity in Contents.OrderBy(e => e.Name))
            {
                // If detailed view is requested
                if (detailed)
                {
                    // Determine if the entity is a folder (DIR) or file (FIL)
                    string type = entity is Folder ? "DIR" : "FIL";
                    // Print detailed information including type, name, size, and dates
                    Console.WriteLine($"  {type} {entity.Name,-20} Size: {entity.GetSize(),-8} Created: {entity.CreationDate.ToShortDateString()} {entity.CreationDate.ToShortTimeString()} Modified: {entity.LastModifiedDate.ToShortDateString()} {entity.LastModifiedDate.ToShortTimeString()} Accessed: {entity.LastAccessedDate.ToShortDateString()} {entity.LastAccessedDate.ToShortTimeString()}");
                }
                else
                {
                    // Print just the entity name, with a trailing slash for folders
                    Console.WriteLine($"  {entity.Name}{(entity is Folder ? "/" : "")}");
                }
            }
        }
    }
}