// Ignore Spelling: Renamer

using System.CommandLine;
using System.Text.RegularExpressions;

namespace Mass_Renamer
{
    static class Program
    {
        static int Main(string[] args)
        {
            // Check for test mode
            if (args.Length == 1 && (args[0] == "-t" || args[0] == "--test"))
            {
                return Tests.PatternTests.RunAllTests();
            }

            // Define options
            var applyOption = new Option<bool>(
                                    ["--apply", "-y"],
                                    "Apply changes. Dry run otherwise.");
            var recursiveOption = new Option<bool>(
                                    ["--recursive", "-r"],
                                    "Process files recursively (with sub-folders).\n"
                                    + "If specified - filename relative to current folder will be used.\n"
                                    + "You SHOULD do a dry run (without '-y'), as logic is a bit different.");
            var isRegexOption = new Option<bool>(
                                    ["--pattern", "-p"],
                                    "Treat SourceMask and RenamePattern as regex PATTERNS directly. '-p' to avoid confusion with recursive.");
            var overwriteOption = new Option<bool>(
                                    ["--overwrite", "-w"],
                                    "Overwrite files during renaming, if target already exists.\n"
                                    + "CARE !!! DESTRUCTIVE !!!");
            // Define arguments
            var sourceMaskArgument = new Argument<string>(
                                    "SourceMask",
                                    "The source mask for matching files.\n\n"
                                    + "In simple patterns mode (default) pattern must match full filename. "
                                    + "Pattern supports named matches in form of:\n"
                                    + "- %A ... %Z for any text greedy, can be used in RenamePattern\n"
                                    + "- %a ... %z for any text UNgreedy, can be used in RenamePattern\n"
                                    + "- %0 ... %9 for numeric matches, can be used in RenamePattern\n"
                                    + "- %% for % escaping\n"
                                    + "- * for any text, discarded\n"
                                    + "- ? for any single character, discarded\n\n"
                                    + "Alternatively you can use '-p' flag and use C# regex pattern directly.");
            var renamePatternArgument = new Argument<string>(
                                    "RenamePattern",
                                    "The pattern to rename files to.\n\n"
                                    + "Simple pattern (default) allows to use named matches from SourceMask in form of %A ... %Z, %a ... %z, %0 ... %9 from SourceMask.\n"
                                    + "You can use %% for % escaping in this mode.\n\n"
                                    + "Alternatively you can use '-p' flag and use C# regex substitutions directly.");
            // Assemble the root command
            var rootCommand = new RootCommand("Mass Renamer - a tool to rename files in bulk using either simple or regex patterns.")
            {
                applyOption,
                recursiveOption,
                isRegexOption,
                overwriteOption,
                sourceMaskArgument,
                renamePatternArgument
            };

            // Set actual handler and run the command
            rootCommand.SetHandler(Act,
                                   applyOption, recursiveOption, isRegexOption, overwriteOption, sourceMaskArgument, renamePatternArgument);
            return rootCommand.Invoke(args);
        }

        /// <summary> Convert a sourceMask pattern string to a regex string </summary>
        /// <exception cref="ArgumentException">Thrown when an invalid pattern is encountered</exception>
        static string Pattern_SourceToRegex(string pattern)
        {
            // Escape the pattern first, then use regex to replace our placeholders in one pass
            string escaped = Regex.Escape(pattern);

            return Regex.Replace(escaped, @"%(%)|%([a-z])|%([A-Z])|%([0-9])|(\\[*?])|%(.)", m =>
            {
                // %% - escaped percent
                if (m.Groups[1].Success)
                    return "%";
                // %[a-z] - lowercase = non-greedy capturing group
                if (m.Groups[2].Success)
                {
                    char c = m.Groups[2].Value[0];
                    return $"(?<{c}>.*?)";
                }
                // %[A-Z] - uppercase = greedy capturing group
                if (m.Groups[3].Success)
                {
                    char c = m.Groups[3].Value[0];
                    return $"(?<{c}>.*)";
                }
                // %[0-9] - digits = named group capturing digits only
                if (m.Groups[4].Success)
                {
                    char c = m.Groups[4].Value[0];
                    return $"(?<d{c}>\\d+)";
                }
                // \\[*?] - escaped wildcards (after Regex.Escape, * becomes \*, ? becomes \?)
                if (m.Groups[5].Success)
                    return m.Value == "\\*" ? ".*" : ".";
                // %X - any other character after % is invalid
                if (m.Groups[6].Success)
                    throw new ArgumentException($"Invalid placeholder '%{m.Groups[6].Value}' in source mask. Valid placeholders: %A-%Z (greedy), %a-%z (non-greedy), %0-%9 (digits), %% (escaped percent), * (any), ? (single).");
                // Should never reach here
                throw new ArgumentException($"Unknown pattern: {m.Value}");
            });
        }

        /// <summary> Convert a renamePattern string to a regex string </summary>
        /// <exception cref="ArgumentException">Thrown when an invalid pattern is encountered</exception>
        static string Pattern_RenameToRegex(string pattern)
        {
            // Escape $ and \ for regex replacement, then use regex to replace placeholders in one pass
            string result = pattern.Replace("$", "$$").Replace(@"\", @"\\");

            return Regex.Replace(result, @"%(%)|%([a-z])|%([A-Z])|%([0-9])|%(.)", m =>
            {
                // %% - escaped percent
                if (m.Groups[1].Success)
                    return "%";
                // %[a-z] - lowercase = non-greedy reference
                if (m.Groups[2].Success)
                {
                    char c = m.Groups[2].Value[0];
                    return $"${{{c}}}";
                }
                // %[A-Z] - uppercase = greedy reference
                if (m.Groups[3].Success)
                {
                    char c = m.Groups[3].Value[0];
                    return $"${{{c}}}";
                }
                // %[0-9] - digits = digit group reference
                if (m.Groups[4].Success)
                {
                    char c = m.Groups[4].Value[0];
                    return $"${{d{c}}}";
                }
                // %X - any other character after % is invalid
                if (m.Groups[5].Success)
                    throw new ArgumentException($"Invalid placeholder '%{m.Groups[5].Value}' in rename pattern. Valid placeholders: %A-%Z, %a-%z, %0-%9, %% (escaped percent).");
                // Should never reach here
                throw new ArgumentException($"Unknown pattern: {m.Value}");
            });
        }

        /// <summary> Take action with the given arguments </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async static Task<int> Act(bool apply, bool recursive, bool isRegex, bool overwrite, string sourceMask, string renamePattern)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var targetFolder = new DirectoryInfo(Environment.CurrentDirectory);

            if (!targetFolder.Exists)
            {
                Console.WriteLine($"Folder \"{targetFolder}\" was not found.");
                return 3;
            }

            HashSet<DirectoryInfo> createdFolders = [];
            HashSet<string> renamedFiles = [];

            string renamePatternRegexString;
            string sourceMaskRegexString;
            try
            {
                renamePatternRegexString = isRegex ? renamePattern : Pattern_RenameToRegex(renamePattern);
                sourceMaskRegexString = isRegex ? sourceMask : Pattern_SourceToRegex(sourceMask);
            }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                return 4;
            }
            // TODO: Add RegexOptions flags control to CommandLine Options
            var sourceMaskRegex = new Regex($"^{sourceMaskRegexString}$", RegexOptions.IgnoreCase);

            Console.Write($"Scanning for files in \"{targetFolder}\"");
            if (recursive)
                Console.Write(" recursively");
            if (apply)
                Console.WriteLine($", using patterns \"{sourceMask}\" ---> \"{renamePattern}\".");
            else
                Console.WriteLine($", using patterns \"{sourceMask}\" ···> \"{renamePattern}\" (DRY RUN).");

            // Pre-scan: collect matched files and calculate max lengths
            var files = Directory.GetFiles(targetFolder.FullName, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            var matchedFiles = new List<(string File, string RelativePath, string NewFileName, string NewFilePath, bool IsDuplicate)>();
            int maxLenSource = 0;
            int maxLenNew = 0;

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(targetFolder.FullName, file);
                var match = sourceMaskRegex.Match(relativePath);
                if (match.Success)
                {
                    var relativePathDisplay = $"\"{relativePath}\"";
                    maxLenSource = Math.Max(maxLenSource, relativePathDisplay.Length);

                    // TODO: Ensure substitution groups used in renamePattern are present in sourceMask
                    // Currently they are just printed as "${C}", for example, if not present in the sourceMask
                    var newFileName = sourceMaskRegex.Replace(relativePath, renamePatternRegexString);
                    var newFilePath = Path.Combine(Environment.CurrentDirectory, newFileName);

                    var newFileNameDisplay = $"\"{newFileName}\"";
                    maxLenNew = Math.Max(maxLenNew, newFileNameDisplay.Length);

                    var isDuplicate = renamedFiles.Contains(newFilePath);
                    renamedFiles.Add(newFilePath);

                    matchedFiles.Add((file, relativePath, newFileName, newFilePath, isDuplicate));
                }
            }

            // Action phase: process matched files
            int filesRenamed = 0;
            int fileErrors = 0;
            int fileDuplicates = matchedFiles.Count(f => f.IsDuplicate);

            if (matchedFiles.Count > 0)
            {
                var firstMatch = matchedFiles[0];
                var sampleMatch = sourceMaskRegex.Match(firstMatch.RelativePath);

                Console.WriteLine();
                Console.WriteLine("Sample match:");
                Console.WriteLine($"  \"{firstMatch.RelativePath}\"");
                for (int i = 1; i < sampleMatch.Groups.Count; i++)
                {
                    Console.Write($"  {i}: ");
                    Console.Write($"{sampleMatch.Groups[i].Name} = ");
                    Console.WriteLine($"\"{sampleMatch.Groups[i].Value}\"");
                }

                Console.WriteLine();
                Console.WriteLine(apply ? "Renaming files:" : "Would rename files:");

                foreach (var matched in matchedFiles)
                {
                    var relativePathDisplay = $"\"{matched.RelativePath}\"";
                    var newFileNameDisplay = $"\"{matched.NewFileName}\"";

                    Console.Write($"  {relativePathDisplay.PadRight(maxLenSource)} ");

                    if (apply)
                    {
                        try
                        {
                            // Create parent folders if needed. Only once per folder.
                            DirectoryInfo parentFolder = new(Path.GetDirectoryName(matched.NewFilePath)!);
                            if (!createdFolders.Contains(parentFolder) && !parentFolder.Exists)
                                parentFolder.Create();
                            createdFolders.Add(parentFolder);

                            // Try to rename the file
                            File.Move(matched.File, matched.NewFilePath, overwrite);
                            filesRenamed++;

                            // Report success
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("---> ");
                            if (matched.IsDuplicate)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"{newFileNameDisplay} (duplicate)");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ResetColor();
                                Console.WriteLine($"{newFileNameDisplay}");
                            }
                        }
                        catch (Exception e)
                        {
                            fileErrors++;
                            // Report failure
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("-X-> ");
                            Console.ResetColor();
                            Console.WriteLine($"{newFileNameDisplay.PadRight(maxLenNew)} : {e.Message}");
                        }
                    }
                    else
                    {
                        filesRenamed++;
                        // Show what would be done
                        Console.Write("···> ");
                        if (matched.IsDuplicate)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"{newFileNameDisplay} (duplicate)");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine($"{newFileNameDisplay}");
                        }
                    }
                }
            }

            // Report results summary
            Console.WriteLine();
            var renameText = apply ? "renamed" : "to be renamed";
            Console.WriteLine($"Files matched: {matchedFiles.Count} out of {files.Length} found");
            Console.WriteLine($"Files {renameText}: {filesRenamed}");
            if (fileDuplicates > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Duplicate names: {fileDuplicates}");
                Console.ResetColor();
            }
            if (fileErrors > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed renaming: {fileErrors}");
                Console.ResetColor();
            }

            // Return error code
            if (fileErrors > 0)
                return 2;
            if (fileDuplicates > 0)
                return 1;
            return 0;
        }
    }
}
