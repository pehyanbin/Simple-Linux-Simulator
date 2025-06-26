using System;
using System.Data;
using System.Linq;

namespace FileStorageSystem
{
    // Defines an enum for supported commands.
    public enum Commands
    {
        Help,
        PrintWorkingDirectory,
        CreateFolder,
        CreateFile,
        Remove,
        Rename,
        Move,
        Copy,
        Navigate,
        ListContent,
        ReadFiles,
        FileEditor,
        SearchFiles,
        ShowHistory,
        Exit,
        Unknown
    }

    public class Program
    {
        // Maps user input to a command enum value.
        private static Commands GetCommands(string command)
        {
            return command.ToLower() switch // Convert command to lowercase and match to enum.
            {
                "help" => Commands.Help,
                "pwd" => Commands.PrintWorkingDirectory,
                "mkdir" => Commands.CreateFolder,
                "touch" => Commands.CreateFile,
                "rm" => Commands.Remove,
                "rename" => Commands.Rename,
                "mv" => Commands.Move,
                "cp" => Commands.Copy,
                "cd" => Commands.Navigate,
                "ls" => Commands.ListContent,
                "cat" => Commands.ReadFiles,
                "nano" => Commands.FileEditor,
                "search" => Commands.SearchFiles,
                "history" => Commands.ShowHistory,
                "exit" => Commands.Exit,
                _ => Commands.Unknown // Default for unrecognized commands.
            };
        }

        // Main entry point for the application.
        public static void Main(string[] args)
        {
            FileManager fileManager = new FileManager(); // Initialize the file manager.
            Console.WriteLine("C# File Storage System\nAuthor: Peh Yan Bin"); // Display welcome message.
            fileManager.PrintWorkingDirectory(); // Show the initial working directory.

            while (true) // Main loop for user input.
            {
                Console.Write($"\n{fileManager.CurrentFolder.GetFullPath()}> "); // Display prompt with current path.
                string commandLine = Console.ReadLine()?.Trim(); // Read user input.

                if (string.IsNullOrEmpty(commandLine)) // Skip empty input.
                {
                    continue;
                }

                string[] parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries); // Split input into parts.
                Commands command = GetCommands(parts[0].ToLower()); // Get the command enum value.

                try
                {
                    switch (command) // Handle the command.
                    {
                        case Commands.Help:
                            fileManager.DisplayHelp(); // Show the help message.
                            break;
                        case Commands.PrintWorkingDirectory:
                            fileManager.PrintWorkingDirectory(); // Show the current directory.
                            break;
                        case Commands.CreateFolder:
                            if (parts.Length > 1) // Check for folder name.
                            {
                                fileManager.CreateFolder(parts[1]); // Create a new folder.
                            }
                            else
                            {
                                Console.WriteLine("Usage: mkdir <folder_name>"); // Show usage if invalid.
                            }
                            break;
                        case Commands.CreateFile:
                            if (parts.Length > 1) // Check for file name.
                            {
                                string content = parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : ""; // Get optional content.
                                fileManager.CreateFile(parts[1], content); // Create a new file.
                            }
                            else
                            {
                                Console.WriteLine("Usage: touch <file_name> [content]"); // Show usage if invalid.
                            }
                            break;
                        case Commands.Remove:
                            if (parts.Length > 1) // Check for path.
                            {
                                fileManager.DeleteEntity(parts[1]); // Delete the entity.
                            }
                            else
                            {
                                Console.WriteLine("Usage: rm <path>"); // Show usage if invalid.
                            }
                            break;
                        case Commands.Rename:
                            if (parts.Length == 3) // Check for path and new name.
                            {
                                fileManager.RenameEntity(parts[1], parts[2]); // Rename the entity.
                            }
                            else
                            {
                                Console.WriteLine("Usage: rename <path> <new_name>"); // Show usage if invalid.
                            }
                            break;
                        case Commands.Move:
                            if (parts.Length == 3) // Check for source and destination paths.
                            {
                                fileManager.MoveEntity(parts[1], parts[2]); // Move the entity.
                            }
                            else
                            {
                                Console.WriteLine("Usage: mv <source_path> <destination_path>"); // Show usage if invalid.
                            }
                            break;
                        case Commands.Copy:
                            if (parts.Length == 3) // Check for source and destination paths.
                            {
                                fileManager.CopyEntity(parts[1], parts[2]); // Copy the entity.
                            }
                            else
                            {
                                Console.WriteLine("Usage: cp <source_path> <destination_path>"); // Show usage if invalid.
                            }
                            break;
                        case Commands.Navigate:
                            if (parts.Length > 1) // Check for path.
                            {
                                fileManager.NavigateTo(parts[1]); // Change directory.
                            }
                            else
                            {
                                Console.WriteLine("Usage: cd <path>"); // Show usage if invalid.
                            }
                            break;
                        case Commands.ListContent:
                            string lsPath = "."; // Default to current directory.
                            bool detailed = false; // Default to simple view.
                            if (parts.Length > 1) // Check for additional arguments.
                            {
                                if (parts[1] == "-l") // Check for detailed view flag.
                                {
                                    detailed = true;
                                    if (parts.Length > 2)
                                    {
                                        lsPath = parts[2]; // Use specified path if provided.
                                    }
                                }
                                else
                                {
                                    lsPath = parts[1]; // Use specified path.
                                }
                            }
                            fileManager.ListContents(lsPath, detailed); // List folder contents.
                            break;
                        case Commands.ReadFiles:
                            if (parts.Length > 1) // Check for file path.
                            {
                                fileManager.ReadFileContent(parts[1]); // Read and display file content.
                            }
                            else
                            {
                                Console.WriteLine("Usage: cat <file_path>"); // Show usage if invalid.
                            }
                            break;
                        case Commands.FileEditor:
                            if (parts.Length > 1) // Check for file path.
                            {
                                fileManager.EditFileContent(parts[1]); // Edit file content.
                            }
                            else
                            {
                                Console.WriteLine("Usage: nano <file_path>"); // Show usage if invalid.
                            }
                            break;
                        case Commands.SearchFiles:
                            if (parts.Length > 1) // Check for search term.
                            {
                                string searchTerm = parts[1];
                                fileManager.SearchFiles(searchTerm); // Search for files and folders.
                            }
                            else
                            {
                                Console.WriteLine("Usage: search <term>"); // Show usage if invalid.
                            }
                            break;
                        case Commands.ShowHistory:
                            fileManager.DisplayHistory(); // Show file access history.
                            break;
                        case Commands.Exit:
                            Console.WriteLine("File System shutted down."); // Display exit message.
                            return; // Exit the application.
                        default:
                            Console.WriteLine($"Error: Unknown command '{command}'. Type 'help' for available commands."); // Handle unknown commands.
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}"); // Log any errors during command execution.
                }
            }
        }
    }
}