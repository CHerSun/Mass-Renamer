// Ignore Spelling: Renamer

using System.CommandLine;
using System.Text.RegularExpressions;

namespace Mass_Renamer
{
    static class Program
    {
        static int Main(string[] args)
        {
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1192:Unnecessary usage of verbatim string literal", Justification = "<Pending>")]
        static string Pattern_SourceToRegex(string pattern)
        {
            // Escape the pattern first, then use regex to replace our placeholders in one pass
            string escaped = Regex.Escape(pattern);

            return Regex.Replace(escaped, @"%([A-Za-z0-9])|([*?])|%", m =>
            {
                // %[A-Za-z0-9] - named placeholders like %A, %a, %0
                if (m.Groups[1].Success)
                {
                    char c = m.Groups[1].Value[0];
                    bool isUpper = char.IsUpper(c);
                    bool isDigit = char.IsDigit(c);

                    if (isDigit)
                        return $"(?<d{c}>\\d+)";
                    // Uppercase = greedy, lowercase = non-greedy
                    return $"(?<{c}>.*?)";
                }
                // [*?] - wildcards
                if (m.Groups[2].Success)
                    return m.Value == "*" ? ".*?" : ".";
                // %% - escaped percent
                return "%";
            });
        }

        /// <summary> Convert a renamePattern string to a regex string </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1192:Unnecessary usage of verbatim string literal", Justification = "<Pending>")]
        static string Pattern_RenameToRegex(string pattern)
        {
            // Escape $ and \ for regex replacement, then use regex to replace placeholders in one pass
            string result = pattern.Replace("$", "$$").Replace(@"\", @"\\");

            return Regex.Replace(result, @"%([A-Za-z0-9])|%%", m =>
            {
                // %[A-Za-z0-9] - named placeholders
                if (m.Groups[1].Success)
                {
                    char c = m.Groups[1].Value[0];
                    if (char.IsDigit(c))
                        return $"${{d{c}}}";
                    return $"${{{c}}}";
                }
                // %% - escaped percent
                return "%";
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

            var renamePatternRegexString = isRegex ? renamePattern : Pattern_RenameToRegex(renamePattern);
            var sourceMaskRegexString = isRegex ? sourceMask : Pattern_SourceToRegex(sourceMask);
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
