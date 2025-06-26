using System;
using System.Collections.Generic;
using System.Linq;

namespace FileStorageSystem
{
    // Provides an interactive text editor for modifying file content.
    public static class TextEditor
    {
        // Edits the content of a file interactively.
        public static string EditText(string initialContent)
        {
            // Display instructions for the text editor.
            Console.WriteLine("\n--- Text Editor (Type '_save_' on a new line to save file and exit, '_quit_' to exit without saving, '_insert_ <line_number>' to insert content, '_delete_ <line_number>' to delete line) ---");
            List<string> lines = new List<string>(initialContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None)); // Split content into lines.

            Console.WriteLine("Initial Content:");
            DisplayContentWithLineNumbers(lines); // Show the initial content with line numbers.

            while (true) // Loop to process user input.
            {
                Console.Write($"{lines.Count + 1}> "); // Prompt for input at the next line.
                string input = Console.ReadLine(); // Read user input.

                if (input.Equals("_save_", StringComparison.OrdinalIgnoreCase)) // Save and exit.
                {
                    Console.WriteLine("Changes saved");
                    return string.Join(Environment.NewLine, lines); // Return the modified content.
                }
                else if (input.Equals("_quit_", StringComparison.OrdinalIgnoreCase)) // Discard changes and exit.
                {
                    Console.WriteLine("Discard changes");
                    return initialContent; // Return the original content.
                }
                else if (input.StartsWith("_insert_ ", StringComparison.OrdinalIgnoreCase)) // Insert content at a line.
                {
                    string[] parts = input.Split(new char[] { ' ' }, 3); // Split command into parts.
                    if (parts.Length == 3 && int.TryParse(parts[1], out int lineNumber)) // Validate line number.
                    {
                        if (lineNumber >= 1 && lineNumber <= lines.Count + 1) // Check if line number is valid.
                        {
                            lines.Insert(lineNumber - 1, parts[2]); // Insert content at the specified line.
                            Console.WriteLine($"Line inserted at {lineNumber}.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid line number for insert.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid INSERT command. Usage: _insert_ <line_number> <content>");
                    }
                }
                else if (input.StartsWith("_delete_ ", StringComparison.OrdinalIgnoreCase)) // Delete a line.
                {
                    string[] parts = input.Split(' '); // Split command into parts.
                    if (parts.Length == 2 && int.TryParse(parts[1], out int lineNumber)) // Validate line number.
                    {
                        if (lineNumber >= 1 && lineNumber <= lines.Count) // Check if line number is valid.
                        {
                            lines.RemoveAt(lineNumber - 1); // Remove the specified line.
                            Console.WriteLine($"Line {lineNumber} deleted.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid line number for delete.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid DELETE command. Usage: _delete_ <line_number>");
                    }
                }
                else
                {
                    lines.Add(input); // Add input as a new line.
                }
                DisplayContentWithLineNumbers(lines); // Display updated content after each change.
            }
        }

        // Displays file content with line numbers.
        public static void ViewText(string content)
        {
            Console.WriteLine("\n--- Viewing File Content ---");
            DisplayContentWithLineNumbers(content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList()); // Split and display content.
            Console.WriteLine("----------------------------");
        }

        // Helper method to display content with line numbers.
        public static void DisplayContentWithLineNumbers(List<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {lines[i]}"); // Print each line with its number.
            }
        }
    }
}