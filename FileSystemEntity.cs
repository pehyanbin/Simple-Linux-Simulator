using System;
using System.IO;

namespace FileStorageSystem
{
    // Abstract base class for file system entities (files and folders), implementing IFileFeatures.
    public abstract class FileSystemEntity : IFileFeatures
    {
        // Private fields to store entity properties.
        private string _name;
        private DateTime _creationDate;
        private DateTime _lastModifiedDate;
        private DateTime _lastAccessedDate;
        private Folder _parentFolder;

        // Public property for the entity's name with getter and setter.
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        // Public property for the creation date of the entity.
        public DateTime CreationDate
        {
            get { return _creationDate; }
            set { _creationDate = value; }
        }

        // Public property for the last modified date of the entity.
        public DateTime LastModifiedDate
        {
            get { return _lastModifiedDate; }
            set { _lastModifiedDate = value; }
        }

        // Public property for the last accessed date of the entity.
        public DateTime LastAccessedDate
        {
            get { return _lastAccessedDate; }
            set { _lastAccessedDate = value; }
        }

        // Public property for the parent folder of the entity.
        public Folder ParentFolder
        {
            get { return _parentFolder; }
            set { _parentFolder = value; }
        }

        // Constructor initializes the entity with a name and parent folder, setting timestamps.
        public FileSystemEntity(string name, Folder parent)
        {
            Name = name;
            CreationDate = DateTime.Now; // Set creation time to current time.
            LastModifiedDate = DateTime.Now; // Set last modified time to current time.
            LastAccessedDate = DateTime.Now; // Set last accessed time to current time.
            ParentFolder = parent; // Assign the parent folder (null for root).
        }

        // Implements Rename method from IFileFeatures to change the entity's name.
        public void Rename(string newName)
        {
            // Validate that the new name is not empty or whitespace.
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("New name cannot be empty.");
            }
            // Check for name conflicts in the parent folder.
            if (ParentFolder != null && ParentFolder.Contents.Exists(e => e.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"An entity with the name '{newName}' already exists in this folder.");
            }
            Name = newName; // Update the name.
            LastModifiedDate = DateTime.Now; // Update the last modified timestamp.
            Console.WriteLine($"Renamed to: {newName}"); // Log the rename action.
        }

        // Abstract method for getting the size, to be implemented by derived classes.
        public abstract long GetSize();

        // Implements GetFullPath from IFileFeatures to return the full file system path.
        public string GetFullPath()
        {
            // Define the base directory for the file system.
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage");
            // If no parent folder (e.g., root), return path relative to base directory.
            if (ParentFolder == null)
            {
                return Path.Combine(basePath, Name);
            }
            // Otherwise, build path by combining parent folder's path with the entity's name.
            return Path.Combine(ParentFolder.GetFullPath(), Name);
        }
    }
}