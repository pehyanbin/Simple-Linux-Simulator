using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageSystem
{
    // Defines an interface for file system entities (files and folders) to enforce common functionality.
    public interface IFileFeatures
    {
        // Method to rename the entity (file or folder).
        void Rename(string newName);

        // Method to get the size of the entity (in bytes for files, total size of contents for folders).
        long GetSize();

        // Method to get the full path of the entity in the file system.
        string GetFullPath();
    }
}