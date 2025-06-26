using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace FileStorageSystem
{
    // Manages logging and retrieval of file access history.
    public class HistoryLogger
    {
        // Private field for the log file path.
        private string _logFilePath;

        // Public property for accessing the log file path.
        public string LogFilePath
        {
            get { return _logFilePath; }
            set { _logFilePath = value; }
        }

        // Constructor initializes the log file.
        public HistoryLogger(string logFileName = "file_access_history.log")
        {
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName); // Set the log file path.
            if (!System.IO.File.Exists(_logFilePath)) // Check if the log file exists.
            {
                System.IO.File.Create(_logFilePath).Dispose(); // Create an empty log file if it doesn't exist.
            }
        }

        // Logs a file access event with a timestamp and path.
        public void LogAccess(string filePath, DateTime accessTime)
        {
            try
            {
                // Append the access event to the log file.
                System.IO.File.AppendAllText(_logFilePath, $"{accessTime:yyyy-MM-dd HH:mm:ss} - Accessed: {filePath}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Log any errors during writing.
            }
        }

        // Retrieves the list of logged access events.
        public List<string> GetHistory()
        {
            try
            {
                return System.IO.File.ReadAllLines(_logFilePath).ToList(); // Read all lines from the log file.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Log any errors during reading.
                return new List<string>(); // Return an empty list if reading fails.
            }
        }
    }
}