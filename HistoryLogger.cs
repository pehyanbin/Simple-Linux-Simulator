using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

// Namespace to organize the file storage system-related classes
namespace FileStorageSystem
{
    // Class to log and retrieve file access history
    public class HistoryLogger
    {
        // Private field to store the path of the log file
        private string _logFilePath;

        // Public property to access and modify the log file path
        public string LogFilePath
        {
            // Getter returns the private log file path
            get { return _logFilePath; }
            // Setter updates the private log file path
            set { _logFilePath = value; }
        }

        // Constructor for HistoryLogger, takes an optional log file name
        public HistoryLogger(string logFileName = "file_access_history.log")
        {
            // Set the log file path by combining the base directory with the file name
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);

            // If the log file does not exist, create it
            if (!System.IO.File.Exists(_logFilePath))
            {
                // Create an empty file and dispose of the file stream
                System.IO.File.Create(_logFilePath).Dispose();
            }
        }

        // Method to log a file access event
        public void LogAccess(string filePath, DateTime accessTime)
        {
            try
            {
                // Append a log entry with the access time and file path to the log file
                System.IO.File.AppendAllText(_logFilePath, $"{accessTime:yyyy-MM-dd HH:mm:ss} - Accessed: {filePath}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                // Print an error message if logging fails
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Method to retrieve the file access history
        public List<string> GetHistory()
        {
            try
            {
                // Read all lines from the log file and return them as a list
                return System.IO.File.ReadAllLines(_logFilePath).ToList();
            }
            catch (Exception ex)
            {
                // Print an error message if reading the history fails
                Console.WriteLine($"Error: {ex.Message}");
                // Return an empty list in case of failure
                return new List<string>();
            }
        }
    }
}