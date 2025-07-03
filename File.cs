using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

// Namespace to organize the file storage system-related classes
namespace FileStorageSystem
{
    // File class inherits from FileSystemEntity to represent a file in the file system
    public class File : FileSystemEntity
    {
        // Private field to store the file's content
        private string _content;

        // Public property to access and modify the file's content
        public string Content
        {
            // Getter for the content
            get
            {
                // Update the last accessed date to the current time
                LastAccessedDate = DateTime.Now;
                // Get the full path of the file
                string filePath = GetFullPath();

                // If the file exists in the physical file system, read its content
                if (System.IO.File.Exists(filePath))
                {
                    _content = System.IO.File.ReadAllText(filePath);
                }

                // Return the content
                return _content;
            }
            // Setter for the content
            set
            {
                // Update the private content field
                _content = value;
                // Update the last modified date to the current time
                LastModifiedDate = DateTime.Now;
                // Get the full path of the file
                string filePath = GetFullPath();
                // Write the new content to the physical file
                System.IO.File.WriteAllText(filePath, _content);
            }
        }

        // Constructor for File, takes a name, parent folder, and optional content
        public File(string name, Folder parent, string content = "") : base(name, parent)
        {
            // Initialize the content field
            _content = content;
            // Get the full path of the file
            string filePath = GetFullPath();

            // If the file does not exist in the physical file system, create it
            if (!System.IO.File.Exists(filePath))
            {
                // Write the initial content to the file
                System.IO.File.WriteAllText(filePath, content);
            }
        }

        // Override the abstract GetSize method to calculate the file's size
        public override long GetSize()
        {
            // Get the full path of the file
            string filePath = GetFullPath();

            // If the file exists in the physical file system, return its size
            if (System.IO.File.Exists(filePath))
            {
                return new FileInfo(filePath).Length;
            }

            // Otherwise, return the length of the in-memory content
            return Content.Length;
        }

        // Method to edit the file's content
        public void Edit(string newContent)
        {
            // Update the file's content using the Content property
            Content = newContent;
            // Print a confirmation message
            Console.WriteLine($"File '{Name}' content updated.");
        }

        // Method to append content to the file
        public void AppendContent(string contentToAppend)
        {
            // Append the new content to the existing content
            Content += contentToAppend;
            // Print a confirmation message
            Console.WriteLine($"Content appended to '{Name}'.");
        }

        // Method to prepend content to the file
        public void PrependContent(string contentToPrepend)
        {
            // Prepend the new content to the existing content
            Content = contentToPrepend + Content;
            // Print a confirmation message
            Console.WriteLine($"Content prepended to '{Name}'.");
        }

        // Method to insert content at a specific line number
        public void InsertContent(int lineNumber, string contentToInsert)
        {
            // Split the content into lines
            var lines = Content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            // Validate the line number
            if (lineNumber < 1 || lineNumber > lines.Count + 1)
            {
                // Throw an exception if the line number is out of range
                throw new ArgumentOutOfRangeException("Line number out of range.");
            }

            // Insert the content at the specified line number (0-based index)
            lines.Insert(lineNumber - 1, contentToInsert);
            // Update the content with the new lines
            Content = string.Join(Environment.NewLine, lines);

            // Print a confirmation message
            Console.WriteLine($"Content inserted at line {lineNumber} in '{Name}'.");
        }

        // Method to delete a specific line from the file
        public void DeleteLine(int lineNumber)
        {
            // Split the content into lines
            var lines = Content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            // Validate the line number
            if (lineNumber < 1 || lineNumber > lines.Count)
            {
                // Throw an exception if the line number is out of range
                throw new ArgumentOutOfRangeException("Line number out of range.");
            }

            // Remove the line at the specified index (0-based)
            lines.RemoveAt(lineNumber - 1);

            // Update the content with the remaining lines
            Content = string.Join(Environment.NewLine, lines);

            // Print a confirmation message
            Console.WriteLine($"Line {lineNumber} deleted from '{Name}'.");
        }

        // Method to display the file's content
        public void View()
        {
            // Update the last accessed date to the current time
            LastAccessedDate = DateTime.Now;

            // Print a header for the file content
            Console.WriteLine($"--- Content of {Name} ---");

            // Print the file's content
            Console.WriteLine(Content);

            // Print a footer
            Console.WriteLine("--------------------------");
        }
    }
}