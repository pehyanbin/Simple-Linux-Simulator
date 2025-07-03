using System;
using System.IO;

// Namespace to organize the file storage system-related classes
namespace FileStorageSystem
{
    // Abstract base class for file system entities (files and folders), implementing IFileFeatures
    public abstract class FileSystemEntity : IFileFeatures
    {
        // Private fields to store entity properties
        private string _name;               // Name of the entity
        private DateTime _creationDate;     // Creation date of the entity
        private DateTime _lastModifiedDate; // Last modified date of the entity
        private DateTime _lastAccessedDate; // Last accessed date of the entity
        private Folder _parentFolder;       // Parent folder of the entity

        // Public property for the entity's name
        public string Name
        {
            // Getter returns the private name field
            get { return _name; }
            // Setter updates the private name field
            set { _name = value; }
        }

        // Public property for the entity's creation date
        public DateTime CreationDate
        {
            // Getter returns the private creation date field
            get { return _creationDate; }
            // Setter updates the private creation date field
            set { _creationDate = value; }
        }

        // Public property for the entity's last modified date
        public DateTime LastModifiedDate
        {
            // Getter returns the private last modified date field
            get { return _lastModifiedDate; }
            // Setter updates the private last modified date field
            set { _lastModifiedDate = value; }
        }

        // Public property for the entity's last accessed date
        public DateTime LastAccessedDate
        {
            // Getter returns the private last accessed date field
            get { return _lastAccessedDate; }
            // Setter updates the private last accessed date field
            set { _lastAccessedDate = value; }
        }

        // Public property for the entity's parent folder
        public Folder ParentFolder
        {
            // Getter returns the private parent folder field
            get { return _parentFolder; }
            // Setter updates the private parent folder field
            set { _parentFolder = value; }
        }

        // Constructor for FileSystemEntity, takes a name and parent folder
        public FileSystemEntity(string name, Folder parent)
        {
            // Set the entity's name
            Name = name;
            // Set the creation date to the current time
            CreationDate = DateTime.Now;
            // Set the last modified date to the current time
            LastModifiedDate = DateTime.Now;
            // Set the last accessed date to the current time
            LastAccessedDate = DateTime.Now;
            // Set the parent folder
            ParentFolder = parent;
        }

        // Method to rename the entity
        public void Rename(string newName)
        {
            // Check if the new name is null or whitespace
            if (string.IsNullOrWhiteSpace(newName))
            {
                // Throw an exception if the new name is invalid
                throw new ArgumentException("New name cannot be empty.");
            }

            // Check if an entity with the new name already exists in the parent folder
            if (ParentFolder != null && ParentFolder.Contents.Exists(e => e.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                // Throw an exception if a duplicate name is found
                throw new InvalidOperationException($"An entity with the name '{newName}' already exists in this folder.");
            }

            // Update the entity's name
            Name = newName;
            // Update the last modified date to the current time
            LastModifiedDate = DateTime.Now;
            // Print a confirmation message
            Console.WriteLine($"Renamed to: {newName}");
        }

        // Abstract method to get the size of the entity, to be implemented by derived classes
        public abstract long GetSize();

        // Method to get the full path of the entity
        public string GetFullPath()
        {
            // Define the base directory for the file system
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage");

            // If the entity has no parent folder (e.g., root), return its path
            if (ParentFolder == null)
            {
                return Path.Combine(basePath, Name);
            }

            // Otherwise, combine the parent folder's path with the entity's name
            return Path.Combine(ParentFolder.GetFullPath(), Name);
        }
    }
}