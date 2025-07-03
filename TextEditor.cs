using System;
using System.Collections.Generic;
using System.Linq;

// Namespace to organize the file storage system-related classes
namespace FileStorageSystem
{
    // Static class to provide text editing functionality
    public static class TextEditor
    {
        // Method to display the text editor prompt and instructions
        public static void Prompt()
        {
            // Print instructions for using the text editor
            Console.WriteLine("\n--- Text Editor (Type '_save_' on a new line to save file and exit, '_quit_' to exit without saving, '_insert_ <line_number>' to insert content, '_delete_ <line_number>' to delete line) ---");
        }

        // Method to edit text interactively
        public static string EditText(string initialContent)
        {
            // Display the editor prompt
            Prompt();
            // Split the initial content into lines
            List<string> lines = new List<string>(initialContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

            // Display the initial content with line numbers
            Console.WriteLine("Initial Content:");
            DisplayContentWithLineNumbers(lines);

            // Main loop to process user input
            while (true)
            {
                // Prompt the user for input at the next line number
                Console.Write($"{lines.Count + 1}> ");
                // Read the user's input
                string input = Console.ReadLine();

                // If the user wants to save and exit
                if (input.Equals("_save_", StringComparison.OrdinalIgnoreCase))
                {
                    // Print a confirmation message
                    Console.WriteLine("Changes saved");
                    // Return the edited content as a single string
                    return string.Join(Environment.NewLine, lines);
                }
                // If the user wants to quit without saving
                else if (input.Equals("_quit_", StringComparison.OrdinalIgnoreCase))
                {
                    // Print a confirmation message
                    Console.WriteLine("Discard changes");
                    // Return the original content
                    return initialContent;
                }
                // If the user wants to insert content at a specific line
                else if (input.StartsWith("_insert_ ", StringComparison.OrdinalIgnoreCase))
                {
                    // Split the input into parts (command, line number, content)
                    string[] parts = input.Split(new char[] { ' ' }, 3);

                    // Check if the input has the correct format and a valid line number
                    if (parts.Length == 3 && int.TryParse(parts[1], out int lineNumber))
                    {
                        // Validate the line number
                        if (lineNumber >= 1 && lineNumber <= lines.Count + 1)
                        {
                            // Insert the content at the specified line (0-based index)
                            lines.Insert(lineNumber - 1, parts[2]);
                            // Print a confirmation message
                            Console.WriteLine($"Line inserted at {lineNumber}.");
                        }
                        else
                        {
                            // Print an error message for invalid line number
                            Console.WriteLine("Invalid line number for insert.");
                        }
                    }
                    else
                    {
                        // Print usage instructions for the insert command
                        Console.WriteLine("Invalid INSERT command. Usage: _insert_ <line_number> <content>");
                    }
                }
                // If the user wants to delete a specific line
                else if (input.StartsWith("_delete_ ", StringComparison.OrdinalIgnoreCase))
                {
                    // Split the input into parts (command, line number)
                    string[] parts = input.Split(' ');

                    // Check if the input has the correct format and a valid line number
                    if (parts.Length == 2 && int.TryParse(parts[1], out int lineNumber))
                    {
                        // Validate the line number
                        if (lineNumber >= 1 && lineNumber <= lines.Count)
                        {
                            // Remove the line at the specified index (0-based)
                            lines.RemoveAt(lineNumber - 1);
                            // Print a confirmation message
                            Console.WriteLine($"Line {lineNumber} deleted.");
                        }
                        else
                        {
                            // Print an error message for invalid line number
                            Console.WriteLine("Invalid line number for delete.");
                        }
                    }
                    else
                    {
                        // Print usage instructions for the delete command
                        Console.WriteLine("Invalid DELETE command. Usage: _delete_ <line_number>");
                    }
                }
                else
                {
                    // Add the input as a new line
                    lines.Add(input);
                    // Clear the console for a clean display
                    Console.Clear();
                    // Redisplay the prompt
                    Prompt();
                }
                // Display the current content with line numbers
                DisplayContentWithLineNumbers(lines);
            }
        }

        // Method to view text content with line numbers
        public static void ViewText(string content)
        {
            // Print a header for viewing content
            Console.WriteLine("\n--- Viewing File Content ---");
            // Split the content into lines and display with line numbers
            DisplayContentWithLineNumbers(content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList());
            // Print a footer
            Console.WriteLine("----------------------------");
        }

        // Method to display a list of lines with line numbers
        public static void DisplayContentWithLineNumbers(List<string> lines)
        {
            // Iterate through the lines
            for (int i = 0; i < lines.Count; i++)
            {
                // Print each line with its line number (1-based)
                Console.WriteLine($"{i + 1}: {lines[i]}");
            }
        }
    }
}