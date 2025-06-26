using System;
using System.Data;
using System.Linq;

namespace FileStorageSystem
{
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
        private static Commands GetCommands(string command)
        {
            return command.ToLower() switch
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
                _ => Commands.Unknown
            };
        }


        public static void Main(string[] args)
        {
            FileManager fileManager = new FileManager();
            Console.WriteLine("C# File Storage System\nAuthor: Peh Yan Bin");
            fileManager.PrintWorkingDirectory();

            while (true)
            {
                Console.Write($"\n{fileManager.CurrentFolder.GetFullPath()}> ");
                string commandLine = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(commandLine))
                {
                    continue;
                }

                string[] parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Commands command = GetCommands(parts[0].ToLower());

                try
                {
                    switch (command)
                    {
                        case Commands.Help:
                            fileManager.DisplayHelp();
                            break;
                        case Commands.PrintWorkingDirectory:
                            fileManager.PrintWorkingDirectory();
                            break;
                        case Commands.CreateFolder:
                            if (parts.Length > 1)
                            {
                                fileManager.CreateFolder(parts[1]);
                            }
                            else
                            {
                                Console.WriteLine("Usage: mkdir <folder_name>");
                            }
                            break;
                        case Commands.CreateFile:
                            if (parts.Length > 1)
                            {
                                string content = parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : "";
                                fileManager.CreateFile(parts[1], content);
                            }
                            else
                            {
                                Console.WriteLine("Usage: touch <file_name> [content]");
                            }
                            break;
                        case Commands.Remove:
                            if (parts.Length > 1)
                            {
                                fileManager.DeleteEntity(parts[1]);
                            }
                            else
                            {
                                Console.WriteLine("Usage: rm <path>");
                            }
                            break;
                        case Commands.Rename:
                            if (parts.Length == 3)
                            {
                                fileManager.RenameEntity(parts[1], parts[2]);
                            }
                            else
                            {
                                Console.WriteLine("Usage: rename <path> <new_name>");
                            }
                            break;
                        case Commands.Move:
                            if (parts.Length == 3)
                            {
                                fileManager.MoveEntity(parts[1], parts[2]);
                            }
                            else
                            {
                                Console.WriteLine("Usage: mv <source_path> <destination_path>");
                            }
                            break;
                        case Commands.Copy:
                            if (parts.Length == 3)
                            {
                                fileManager.CopyEntity(parts[1], parts[2]);
                            }
                            else
                            {
                                Console.WriteLine("Usage: cp <source_path> <destination_path>");
                            }
                            break;
                        case Commands.Navigate:
                            if (parts.Length > 1)
                            {
                                fileManager.NavigateTo(parts[1]);
                            }
                            else
                            {
                                Console.WriteLine("Usage: cd <path>");
                            }
                            break;
                        case Commands.ListContent:
                            string lsPath = ".";
                            bool detailed = false;
                            if (parts.Length > 1)
                            {
                                if (parts[1] == "-l")
                                {
                                    detailed = true;
                                    if (parts.Length > 2)
                                    {
                                        lsPath = parts[2];
                                    }
                                }
                                else
                                {
                                    lsPath = parts[1];
                                }
                            }
                            fileManager.ListContents(lsPath, detailed);
                            break;
                        case Commands.ReadFiles:
                            if (parts.Length > 1)
                            {
                                fileManager.ReadFileContent(parts[1]);
                            }
                            else
                            {
                                Console.WriteLine("Usage: cat <file_path>");
                            }
                            break;
                        case Commands.FileEditor:
                            if (parts.Length > 1)
                            {
                                fileManager.EditFileContent(parts[1]);
                            }
                            else
                            {
                                Console.WriteLine("Usage: nano <file_path>");
                            }
                            break;
                        case Commands.SearchFiles:
                            if (parts.Length > 1)
                            {
                                string searchTerm = parts[1];
                                fileManager.SearchFiles(searchTerm);
                            }
                            else
                            {
                                Console.WriteLine("Usage: search <term>");
                            }
                            break;
                        case Commands.ShowHistory:
                            fileManager.DisplayHistory();
                            break;
                        case Commands.Exit:
                            Console.WriteLine("File System shutted down.");
                            return;
                        default:
                            Console.WriteLine($"Error: Unknown command '{command}'. Type 'help' for available commands.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}