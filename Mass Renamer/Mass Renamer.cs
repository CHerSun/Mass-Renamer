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
                                    + "If specified - filename relative to TargetFolder will be used.\n"
                                    + "You SHOULD do a dry run (without '-y'), as logic is a bit different.");
            var isRegexOption = new Option<bool>(
                                    ["--pattern", "-p"],
                                    "Treat SourceMask and RenamePattern as regex PATTERNS directly. '-p' to avoid confusion with recursive.");
            var overwriteOption = new Option<bool>(
                                    ["--overwrite", "-w"],
                                    "Overwrite files during renaming, if target already exists.\n"
                                    + "CARE !!! DESTRUCTIVE !!!");
            // Define arguments
            var targetFolderArgument = new Argument<DirectoryInfo>(
                                    "TargetFolder",
                                    "The target folder where to rename files. Relative and absolute paths could be used.");
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
                targetFolderArgument,
                sourceMaskArgument,
                renamePatternArgument
            };

            // Set actual handler and run the command
            rootCommand.SetHandler(Act,
                                   applyOption, recursiveOption, isRegexOption, overwriteOption, targetFolderArgument, sourceMaskArgument, renamePatternArgument);
            return rootCommand.Invoke(args);
        }

        /// <summary> Convert a sourceMask pattern string to a regex string </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1192:Unnecessary usage of verbatim string literal", Justification = "<Pending>")]
        static string SourceToRegexPattern(string pattern)
        {
            // HACK: This is dumb and ugly, but we do this only once per run and I don't know how to do it better currently.
            // TODO: Think of a better solution.
            return Regex.Escape(pattern)
                .Replace(@"%", @"\%")
                .Replace(@"\%\%", "%")
                .Replace(@"\%A", @"(?<A>.*)")
                .Replace(@"\%B", @"(?<B>.*)")
                .Replace(@"\%C", @"(?<C>.*)")
                .Replace(@"\%D", @"(?<D>.*)")
                .Replace(@"\%E", @"(?<E>.*)")
                .Replace(@"\%F", @"(?<F>.*)")
                .Replace(@"\%G", @"(?<G>.*)")
                .Replace(@"\%H", @"(?<H>.*)")
                .Replace(@"\%I", @"(?<I>.*)")
                .Replace(@"\%J", @"(?<J>.*)")
                .Replace(@"\%K", @"(?<K>.*)")
                .Replace(@"\%L", @"(?<L>.*)")
                .Replace(@"\%M", @"(?<M>.*)")
                .Replace(@"\%N", @"(?<N>.*)")
                .Replace(@"\%O", @"(?<O>.*)")
                .Replace(@"\%P", @"(?<P>.*)")
                .Replace(@"\%Q", @"(?<Q>.*)")
                .Replace(@"\%R", @"(?<R>.*)")
                .Replace(@"\%S", @"(?<S>.*)")
                .Replace(@"\%T", @"(?<T>.*)")
                .Replace(@"\%U", @"(?<U>.*)")
                .Replace(@"\%V", @"(?<V>.*)")
                .Replace(@"\%W", @"(?<W>.*)")
                .Replace(@"\%X", @"(?<X>.*)")
                .Replace(@"\%Y", @"(?<Y>.*)")
                .Replace(@"\%Z", @"(?<Z>.*)")
                .Replace(@"\%a", @"(?<a>.*?)")
                .Replace(@"\%b", @"(?<b>.*?)")
                .Replace(@"\%c", @"(?<c>.*?)")
                .Replace(@"\%d", @"(?<d>.*?)")
                .Replace(@"\%e", @"(?<e>.*?)")
                .Replace(@"\%f", @"(?<f>.*?)")
                .Replace(@"\%g", @"(?<g>.*?)")
                .Replace(@"\%h", @"(?<h>.*?)")
                .Replace(@"\%i", @"(?<i>.*?)")
                .Replace(@"\%j", @"(?<j>.*?)")
                .Replace(@"\%k", @"(?<k>.*?)")
                .Replace(@"\%l", @"(?<l>.*?)")
                .Replace(@"\%m", @"(?<m>.*?)")
                .Replace(@"\%n", @"(?<n>.*?)")
                .Replace(@"\%o", @"(?<o>.*?)")
                .Replace(@"\%p", @"(?<p>.*?)")
                .Replace(@"\%q", @"(?<q>.*?)")
                .Replace(@"\%r", @"(?<r>.*?)")
                .Replace(@"\%s", @"(?<s>.*?)")
                .Replace(@"\%t", @"(?<t>.*?)")
                .Replace(@"\%u", @"(?<u>.*?)")
                .Replace(@"\%v", @"(?<v>.*?)")
                .Replace(@"\%w", @"(?<w>.*?)")
                .Replace(@"\%x", @"(?<x>.*?)")
                .Replace(@"\%y", @"(?<y>.*?)")
                .Replace(@"\%z", @"(?<z>.*?)")
                .Replace(@"\%0", @"(?<d0>\d+)")
                .Replace(@"\%1", @"(?<d1>\d+)")
                .Replace(@"\%2", @"(?<d2>\d+)")
                .Replace(@"\%3", @"(?<d3>\d+)")
                .Replace(@"\%4", @"(?<d4>\d+)")
                .Replace(@"\%5", @"(?<d5>\d+)")
                .Replace(@"\%6", @"(?<d6>\d+)")
                .Replace(@"\%7", @"(?<d7>\d+)")
                .Replace(@"\%8", @"(?<d8>\d+)")
                .Replace(@"\%9", @"(?<d9>\d+)")
                .Replace(@"\*", @".*?")
                .Replace(@"\?", @".");
        }

        /// <summary> Convert a renamePattern string to a regex string </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1192:Unnecessary usage of verbatim string literal", Justification = "<Pending>")]
        static string TargetToRegexPattern(string pattern)
        {
            // HACK: This is dumb and ugly, but we do this only once and I don't know how to do it better currently.
            // TODO: Think of a better solution.
            return pattern
                .Replace(@"$", @"$$")
                .Replace(@"\", @"\\")
                .Replace(@"%", @"\%")
                .Replace(@"\%\%", "%")
                .Replace(@"\%A", @"${A}")
                .Replace(@"\%B", @"${B}")
                .Replace(@"\%C", @"${C}")
                .Replace(@"\%D", @"${D}")
                .Replace(@"\%E", @"${E}")
                .Replace(@"\%F", @"${F}")
                .Replace(@"\%G", @"${G}")
                .Replace(@"\%H", @"${H}")
                .Replace(@"\%I", @"${I}")
                .Replace(@"\%J", @"${J}")
                .Replace(@"\%K", @"${K}")
                .Replace(@"\%L", @"${L}")
                .Replace(@"\%M", @"${M}")
                .Replace(@"\%N", @"${N}")
                .Replace(@"\%O", @"${O}")
                .Replace(@"\%P", @"${P}")
                .Replace(@"\%Q", @"${Q}")
                .Replace(@"\%R", @"${R}")
                .Replace(@"\%S", @"${S}")
                .Replace(@"\%T", @"${T}")
                .Replace(@"\%U", @"${U}")
                .Replace(@"\%V", @"${V}")
                .Replace(@"\%W", @"${W}")
                .Replace(@"\%X", @"${X}")
                .Replace(@"\%Y", @"${Y}")
                .Replace(@"\%Z", @"${Z}")
                .Replace(@"\%a", @"${a}")
                .Replace(@"\%b", @"${b}")
                .Replace(@"\%c", @"${c}")
                .Replace(@"\%d", @"${d}")
                .Replace(@"\%e", @"${e}")
                .Replace(@"\%f", @"${f}")
                .Replace(@"\%g", @"${g}")
                .Replace(@"\%h", @"${h}")
                .Replace(@"\%i", @"${i}")
                .Replace(@"\%j", @"${j}")
                .Replace(@"\%k", @"${k}")
                .Replace(@"\%l", @"${l}")
                .Replace(@"\%m", @"${m}")
                .Replace(@"\%n", @"${n}")
                .Replace(@"\%o", @"${o}")
                .Replace(@"\%p", @"${p}")
                .Replace(@"\%q", @"${q}")
                .Replace(@"\%r", @"${r}")
                .Replace(@"\%s", @"${s}")
                .Replace(@"\%t", @"${t}")
                .Replace(@"\%u", @"${u}")
                .Replace(@"\%v", @"${v}")
                .Replace(@"\%w", @"${w}")
                .Replace(@"\%x", @"${x}")
                .Replace(@"\%y", @"${y}")
                .Replace(@"\%z", @"${z}")
                .Replace(@"\%0", @"${d0}")
                .Replace(@"\%1", @"${d1}")
                .Replace(@"\%2", @"${d2}")
                .Replace(@"\%3", @"${d3}")
                .Replace(@"\%4", @"${d4}")
                .Replace(@"\%5", @"${d5}")
                .Replace(@"\%6", @"${d6}")
                .Replace(@"\%7", @"${d7}")
                .Replace(@"\%8", @"${d8}")
                .Replace(@"\%9", @"${d9}")
                .Replace(@"\%", @"%");
        }

        /// <summary> Take action with the given arguments </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async static Task<int> Act(bool apply, bool recursive, bool isRegex, bool overwrite, DirectoryInfo targetFolder, string sourceMask, string renamePattern)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (!targetFolder.Exists)
            {
                Console.WriteLine($"Folder \"{targetFolder}\" was not found.");
                return 3;
            }

            HashSet<DirectoryInfo> createdFolders = [];
            HashSet<string> renamedFiles = [];

            var renamePatternRegexString = isRegex ? renamePattern : TargetToRegexPattern(renamePattern);
            var sourceMaskRegexString = isRegex ? sourceMask : SourceToRegexPattern(sourceMask);
            // TODO: Add RegexOptions flags control to CommandLine Options
            var sourceMaskRegex = new Regex($"^{sourceMaskRegexString}$", RegexOptions.IgnoreCase);

            Console.Write($"Scanning for files in \"{targetFolder}\"");
            if (recursive)
                Console.Write(" recursively");
            Console.WriteLine($", using patterns \"{sourceMask}\" ---> \"{renamePattern}\".");

            bool firstMatch = true;
            int maxLenSource = 0;
            int maxLenNew = 0;
            int filesRenamed = 0;
            int filesMatched = 0;
            int fileErrors = 0;
            int fileDuplicates = 0;

            var files = Directory.GetFiles(targetFolder.FullName, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(targetFolder.FullName, file);
                var match = sourceMaskRegex.Match(relativePath);
                if (match.Success)
                {
                    filesMatched++;

                    var relativePathDisplay = $"\"{relativePath}\"";
                    maxLenSource = Math.Max(maxLenSource, relativePathDisplay.Length);

                    // TODO: Ensure substitution groups used in renamePattern are present in sourceMask
                    // Currently they are just printed as "${C}", for example, if not present in the sourceMask
                    var newFileName = sourceMaskRegex.Replace(relativePath, renamePatternRegexString);
                    var newFilePath = Path.Combine(targetFolder.FullName, newFileName);

                    var newFileNameDisplay = $"\"{newFileName}\"";
                    maxLenNew = Math.Max(maxLenNew, newFileNameDisplay.Length);

                    var isDuplicate = renamedFiles.Contains(newFilePath);
                    renamedFiles.Add(newFilePath);
                    if (isDuplicate)
                        fileDuplicates++;

                    if (firstMatch)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Sample match:");
                        Console.WriteLine($"  {relativePathDisplay}");
                        for (int i = 1; i < match.Groups.Count; i++)
                        {
                            Console.Write($"  {i}: ");
                            Console.Write($"{match.Groups[i].Name} = ");
                            Console.WriteLine($"\"{match.Groups[i].Value}\"");
                        }

                        Console.WriteLine();
                        Console.WriteLine(apply ? "Renaming files:" : "Would rename files:");
                        firstMatch = false;
                    }
                    Console.Write($"  {relativePathDisplay.PadRight(maxLenSource)} ");

                    if (apply)
                    {
                        try
                        {
                            // Create parent folders if needed. Only once per folder.
                            DirectoryInfo parentFolder = new(Path.GetDirectoryName(newFilePath)!);
                            if (!createdFolders.Contains(parentFolder) && !parentFolder.Exists)
                                parentFolder.Create();
                            createdFolders.Add(parentFolder);

                            // Try to rename the file
                            File.Move(file, newFilePath, overwrite);
                            filesRenamed++;

                            // Report success
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("---> ");
                            if (isDuplicate)
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
                        if (isDuplicate)
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
            Console.WriteLine($"Files matched: {filesMatched} out of {files.Length} found");
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
