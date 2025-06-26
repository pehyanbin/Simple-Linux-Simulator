using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace FileStorageSystem
{
    // Represents a file in the file system, inheriting from FileSystemEntity.
    public class File : FileSystemEntity
    {
        // Private field to store the file's content.
        private string _content;

        // Public property for accessing and modifying file content, synchronized with disk.
        public string Content
        {
            get
            {
                LastAccessedDate = DateTime.Now; // Update last accessed timestamp on read.
                string filePath = GetFullPath(); // Get the full path of the file.
                if (System.IO.File.Exists(filePath)) // Check if the file exists on disk.
                {
                    _content = System.IO.File.ReadAllText(filePath); // Read content from disk.
                }
                return _content; // Return the file's content.
            }
            set
            {
                _content = value; // Update the in-memory content.
                LastModifiedDate = DateTime.Now; // Update last modified timestamp.
                string filePath = GetFullPath(); // Get the full path of the file.
                System.IO.File.WriteAllText(filePath, _content); // Write content to disk.
            }
        }

        // Constructor creates a file and optionally writes initial content to disk.
        public File(string name, Folder parent, string content = "") : base(name, parent)
        {
            _content = content; // Set initial content.
            string filePath = GetFullPath(); // Get the full path of the file.

            if (!System.IO.File.Exists(filePath)) // Check if the file doesn't exist on disk.
            {
                System.IO.File.WriteAllText(filePath, content); // Create the file with initial content.
            }
        }

        // Calculates the size of the file based on its content.
        public override long GetSize()
        {
            string filePath = GetFullPath(); // Get the full path of the file.
            if (System.IO.File.Exists(filePath)) // If the file exists on disk.
            {
                return new FileInfo(filePath).Length; // Return the file's size from disk.
            }
            return Content.Length; // Otherwise, return the in-memory content length.
        }

        // Updates the file's content and logs the action.
        public void Edit(string newContent)
        {
            Content = newContent; // Set new content (triggers write to disk).
            Console.WriteLine($"File '{Name}' content updated."); // Log the edit action.
        }

        // Appends content to the file.
        public void AppendContent(string contentToAppend)
        {
            Content += contentToAppend; // Append to existing content (triggers write to disk).
            Console.WriteLine($"Content appended to '{Name}'."); // Log the append action.
        }

        // Prepends content to the file.
        public void PrependContent(string contentToPrepend)
        {
            Content = contentToPrepend + Content; // Prepend to existing content (triggers write to disk).
            Console.WriteLine($"Content prepended to '{Name}'."); // Log the prepend action.
        }

        // Inserts content at a specific line number.
        public void InsertContent(int lineNumber, string contentToInsert)
        {
            var lines = Content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList(); // Split content into lines.
            if (lineNumber < 1 || lineNumber > lines.Count + 1) // Validate line number.
            {
                throw new ArgumentOutOfRangeException("Line number out of range.");
            }
            lines.Insert(lineNumber - 1, contentToInsert); // Insert content at the specified line.
            Content = string.Join(Environment.NewLine, lines); // Rejoin lines and update content.
            Console.WriteLine($"Content inserted at line {lineNumber} in '{Name}'."); // Log the insert action.
        }

        // Deletes a specific line from the file.
        public void DeleteLine(int lineNumber)
        {
            var lines = Content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList(); // Split content into lines.
            if (lineNumber < 1 || lineNumber > lines.Count) // Validate line number.
            {
                throw new ArgumentOutOfRangeException("Line number out of range.");
            }
            lines.RemoveAt(lineNumber - 1); // Remove the specified line.
            Content = string.Join(Environment.NewLine, lines); // Rejoin lines and update content.
            Console.WriteLine($"Line {lineNumber} deleted from '{Name}'."); // Log the delete action.
        }

        // Displays the file's content to the console.
        public void View()
        {
            LastAccessedDate = DateTime.Now; // Update last accessed timestamp.
            Console.WriteLine($"--- Content of {Name} ---"); // Display header.
            Console.WriteLine(Content); // Display file content.
            Console.WriteLine("--------------------------"); // Display footer.
        }
    }
}