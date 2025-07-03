using System;
using System.Data;
using System.Linq;

// Namespace to organize the file storage system-related classes
namespace FileStorageSystem
{
    // Enum defining the supported commands for the file system interface
    public enum Commands
    {
        Help,              // Display help information
        PrintWorkingDirectory, // Print the current working directory
        CreateFolder,      // Create a new folder
        CreateFile,        // Create a new file
        Remove,            // Delete a file or folder
        Rename,            // Rename a file or folder
        Move,              // Move a file or folder
        Copy,              // Copy a file or folder
        Navigate,          // Change the current directory
        ListContent,       // List contents of a directory
        ReadFiles,         // Read the contents of a file
        FileEditor,        // Edit a file's contents
        SearchFiles,       // Search for files or folders
        ShowHistory,       // Display file access history
        Exit,              // Exit the program
        Unknown            // Handle unrecognized commands
    }

    public class Program
    {
        // Method to map user input to a command enum
        public static Commands GetCommands(string command)
        {
            // Convert the command to lowercase and use a switch expression to map it
            return command.ToLower() switch
            {
                "help" => Commands.Help,                    // Map "help" to Help command
                "pwd" => Commands.PrintWorkingDirectory,    // Map "pwd" to PrintWorkingDirectory command
                "mkdir" => Commands.CreateFolder,           // Map "mkdir" to CreateFolder command
                "touch" => Commands.CreateFile,             // Map "touch" to CreateFile command
                "rm" => Commands.Remove,                    // Map "rm" to Remove command
                "rename" => Commands.Rename,                // Map "rename" to Rename command
                "mv" => Commands.Move,                      // Map "mv" to Move command
                "cp" => Commands.Copy,                      // Map "cp" to Copy command
                "cd" => Commands.Navigate,                  // Map "cd" to Navigate command
                "ls" => Commands.ListContent,               // Map "ls" to ListContent command
                "cat" => Commands.ReadFiles,                // Map "cat" to ReadFiles command
                "nano" => Commands.FileEditor,              // Map "nano" to FileEditor command
                "search" => Commands.SearchFiles,           // Map "search" to SearchFiles command
                "history" => Commands.ShowHistory,          // Map "history" to ShowHistory command
                "exit" => Commands.Exit,                    // Map "exit" to Exit command
                _ => Commands.Unknown                       // Default to Unknown for unrecognized commands
            };
        }

        // Main entry point of the program
        public static void Main(string[] args)
        {
            // Create a new FileManager instance to manage the file system
            FileManager fileManager = new FileManager();
            // Print the program title and author
            Console.WriteLine("C# File Storage System\nAuthor: Peh Yan Bin");
            // Print the initial working directory
            fileManager.PrintWorkingDirectory();

            // Main loop to continuously accept user commands
            while (true)
            {
                // Display the current directory path as a prompt
                Console.Write($"\n{fileManager.CurrentFolder.GetFullPath()}> ");
                // Read the user's input command
                string commandLine = Console.ReadLine()?.Trim();

                // Skip empty or null input
                if (string.IsNullOrEmpty(commandLine))
                {
                    continue;
                }

                // Split the input into parts (command and arguments)
                string[] parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                // Get the command enum based on the first part
                Commands command = GetCommands(parts[0].ToLower());

                // Wrap command execution in a try-catch to handle errors
                try
                {
                    // Switch on the command to execute the appropriate action
                    switch (command)
                    {
                        // Handle the "help" command
                        case Commands.Help:
                            fileManager.DisplayHelp();
                            break;

                        // Handle the "pwd" command
                        case Commands.PrintWorkingDirectory:
                            fileManager.PrintWorkingDirectory();
                            break;

                        // Handle the "mkdir" command
                        case Commands.CreateFolder:
                            if (parts.Length > 1)
                            {
                                // Create a folder with the provided name
                                fileManager.CreateFolder(parts[1]);
                            }
                            else
                            {
                                // Print usage instructions if no folder name is provided
                                Console.WriteLine("Usage: mkdir <folder_name>");
                            }
                            break;

                        // Handle the "touch" command
                        case Commands.CreateFile:
                            if (parts.Length > 1)
                            {
                                // Combine arguments after the file name as content
                                string content = parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : "";
                                // Create a file with the provided name and optional content
                                fileManager.CreateFile(parts[1], content);
                            }
                            else
                            {
                                // Print usage instructions if no file name is provided
                                Console.WriteLine("Usage: touch <file_name> [content]");
                            }
                            break;

                        // Handle the "rm" command
                        case Commands.Remove:
                            if (parts.Length > 1)
                            {
                                // Delete the entity at the specified path
                                fileManager.DeleteEntity(parts[1]);
                            }
                            else
                            {
                                // Print usage instructions if no path is provided
                                Console.WriteLine("Usage: rm <path>");
                            }
                            break;

                        // Handle the "rename" command
                        case Commands.Rename:
                            if (parts.Length == 3)
                            {
                                // Rename the entity at the specified path to the new name
                                fileManager.RenameEntity(parts[1], parts[2]);
                            }
                            else
                            {
                                // Print usage instructions if incorrect arguments are provided
                                Console.WriteLine("Usage: rename <path> <new_name>");
                            }
                            break;

                        // Handle the "mv" command
                        case Commands.Move:
                            if (parts.Length == 3)
                            {
                                // Move the entity from source path to destination path
                                fileManager.MoveEntity(parts[1], parts[2]);
                            }
                            else
                            {
                                // Print usage instructions if incorrect arguments are provided
                                Console.WriteLine("Usage: mv <source_path> <destination_path>");
                            }
                            break;

                        // Handle the "cp" command
                        case Commands.Copy:
                            if (parts.Length == 3)
                            {
                                // Copy the entity from source path to destination path
                                fileManager.CopyEntity(parts[1], parts[2]);
                            }
                            else
                            {
                                // Print usage instructions if incorrect arguments are provided
                                Console.WriteLine("Usage: cp <source_path> <destination_path>");
                            }
                            break;

                        // Handle the "cd" command
                        case Commands.Navigate:
                            if (parts.Length > 1)
                            {
                                // Navigate to the specified path
                                fileManager.NavigateTo(parts[1]);
                            }
                            else
                            {
                                // Print usage instructions if no path is provided
                                Console.WriteLine("Usage: cd <path>");
                            }
                            break;

                        // Handle the "ls" command
                        case Commands.ListContent:
                            // Default path is the current directory
                            string lsPath = ".";
                            // Flag for detailed listing
                            bool detailed = false;

                            // Check if additional arguments are provided
                            if (parts.Length > 1)
                            {
                                // If the second argument is "-l", enable detailed view
                                if (parts[1] == "-l")
                                {
                                    detailed = true;
                                    // If a path is provided after "-l", use it
                                    if (parts.Length > 2)
                                    {
                                        lsPath = parts[2];
                                    }
                                }
                                else
                                {
                                    // Use the provided path
                                    lsPath = parts[1];
                                }
                            }
                            // List the contents of the specified path
                            fileManager.ListContents(lsPath, detailed);
                            break;

                        // Handle the "cat" command
                        case Commands.ReadFiles:
                            if (parts.Length > 1)
                            {
                                // Read and display the content of the specified file
                                fileManager.ReadFileContent(parts[1]);
                            }
                            else
                            {
                                // Print usage instructions if no file path is provided
                                Console.WriteLine("Usage: cat <file_path>");
                            }
                            break;

                        // Handle the "nano" command
                        case Commands.FileEditor:
                            if (parts.Length > 1)
                            {
                                // Edit the content of the specified file
                                fileManager.EditFileContent(parts[1]);
                            }
                            else
                            {
                                // Print usage instructions if no file path is provided
                                Console.WriteLine("Usage: nano <file_path>");
                            }
                            break;

                        // Handle the "search" command
                        case Commands.SearchFiles:
                            if (parts.Length > 1)
                            {
                                // Search for files/folders matching the provided term
                                string searchTerm = parts[1];
                                fileManager.SearchFiles(searchTerm);
                            }
                            else
                            {
                                // Print usage instructions if no search term is provided
                                Console.WriteLine("Usage: search <term>");
                            }
                            break;

                        // Handle the "history" command
                        case Commands.ShowHistory:
                            // Display the file access history
                            fileManager.DisplayHistory();
                            break;

                        // Handle the "exit" command
                        case Commands.Exit:
                            // Print shutdown message and exit the program
                            Console.WriteLine("File System shutted down.");
                            return;

                        // Handle unknown commands
                        default:
                            // Inform the user of an unrecognized command
                            Console.WriteLine($"Error: Unknown command. Type 'help' for available commands.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Print any errors that occur during command execution
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}