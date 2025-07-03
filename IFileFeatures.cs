using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Namespace to organize the file storage system-related classes
namespace FileStorageSystem
{
    // Interface defining common features for file system entities (files and folders)
    public interface IFileFeatures
    {
        // Method to rename the entity
        public void Rename(string newName);
        // Method to get the size of the entity
        public long GetSize();
        // Method to get the full path of the entity
        public string GetFullPath();
    }
}