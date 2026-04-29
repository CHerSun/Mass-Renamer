using System.Text.RegularExpressions;

namespace Mass_Renamer.Tests
{
    // TODO: Consider using a proper unit testing framework like NUnit or xUnit for better test organization and reporting. This is a simple custom test runner for demonstration purposes.

    /// <summary>
    /// Unit tests for Pattern_SourceToRegex and Pattern_RenameToRegex functions.
    /// Run with: dotnet run -- -t
    /// </summary>
    static class PatternTests
    {
        public static int RunAllTests()
        {
            int passed = 0;
            int failed = 0;

            Console.WriteLine("=== Pattern Conversion Tests ===\n");

            // ============================================
            // Pattern_SourceToRegex Tests
            // ============================================
            Console.WriteLine("--- Pattern_SourceToRegex Tests ---\n");

            // %% - escaped percent
            RunTest("Source: %% → %", () =>
            {
                var result = Pattern_SourceToRegex("%%");
                AssertEqual(result, "%");
            }, ref passed, ref failed);

            // %a-%z - lowercase = non-greedy
            RunTest("Source: %a → (?<a>.*?)", () =>
            {
                var result = Pattern_SourceToRegex("%a");
                AssertEqual(result, @"(?<a>.*?)");
            }, ref passed, ref failed);

            RunTest("Source: %z → (?<z>.*?)", () =>
            {
                var result = Pattern_SourceToRegex("%z");
                AssertEqual(result, @"(?<z>.*?)");
            }, ref passed, ref failed);

            // %A-%Z - uppercase = greedy
            RunTest("Source: %A → (?<A>.*)", () =>
            {
                var result = Pattern_SourceToRegex("%A");
                AssertEqual(result, @"(?<A>.*)");
            }, ref passed, ref failed);

            RunTest("Source: %Z → (?<Z>.*)", () =>
            {
                var result = Pattern_SourceToRegex("%Z");
                AssertEqual(result, @"(?<Z>.*)");
            }, ref passed, ref failed);

            // %0-%9 - digits only
            RunTest("Source: %0 → (?<d0>\\d+)", () =>
            {
                var result = Pattern_SourceToRegex("%0");
                AssertEqual(result, @"(?<d0>\d+)");
            }, ref passed, ref failed);

            RunTest("Source: %9 → (?<d9>\\d+)", () =>
            {
                var result = Pattern_SourceToRegex("%9");
                AssertEqual(result, @"(?<d9>\d+)");
            }, ref passed, ref failed);

            // * - any text (discarded)
            RunTest("Source: * → .*", () =>
            {
                var result = Pattern_SourceToRegex("*");
                AssertEqual(result, ".*");
            }, ref passed, ref failed);

            // ? - single character (discarded)
            RunTest("Source: ? → .", () =>
            {
                var result = Pattern_SourceToRegex("?");
                AssertEqual(result, ".");
            }, ref passed, ref failed);

            // Combined patterns
            RunTest("Source: %A%a%0 → (?<A>.*)(?<a>.*?)(?<d0>\\d+)", () =>
            {
                var result = Pattern_SourceToRegex("%A%a%0");
                AssertEqual(result, @"(?<A>.*)(?<a>.*?)(?<d0>\d+)");
            }, ref passed, ref failed);

            // Note: . is escaped by Regex.Escape, so .txt becomes \.txt
            RunTest("Source: file_%A.txt → file_(?<A>.*)\\.txt", () =>
            {
                var result = Pattern_SourceToRegex("file_%A.txt");
                AssertEqual(result, @"file_(?<A>.*)\.txt");
            }, ref passed, ref failed);

            // ============================================
            // Pattern_RenameToRegex Tests
            // ============================================
            Console.WriteLine("\n--- Pattern_RenameToRegex Tests ---\n");

            // %% - escaped percent
            RunTest("Rename: %% → %", () =>
            {
                var result = Pattern_RenameToRegex("%%");
                AssertEqual(result, "%");
            }, ref passed, ref failed);

            // %a-%z - lowercase = non-greedy reference
            RunTest("Rename: %a → ${a}", () =>
            {
                var result = Pattern_RenameToRegex("%a");
                AssertEqual(result, "${a}");
            }, ref passed, ref failed);

            RunTest("Rename: %z → ${z}", () =>
            {
                var result = Pattern_RenameToRegex("%z");
                AssertEqual(result, "${z}");
            }, ref passed, ref failed);

            // %A-%Z - uppercase = greedy reference
            RunTest("Rename: %A → ${A}", () =>
            {
                var result = Pattern_RenameToRegex("%A");
                AssertEqual(result, "${A}");
            }, ref passed, ref failed);

            RunTest("Rename: %Z → ${Z}", () =>
            {
                var result = Pattern_RenameToRegex("%Z");
                AssertEqual(result, "${Z}");
            }, ref passed, ref failed);

            // %0-%9 - digits reference
            RunTest("Rename: %0 → ${d0}", () =>
            {
                var result = Pattern_RenameToRegex("%0");
                AssertEqual(result, "${d0}");
            }, ref passed, ref failed);

            RunTest("Rename: %9 → ${d9}", () =>
            {
                var result = Pattern_RenameToRegex("%9");
                AssertEqual(result, "${d9}");
            }, ref passed, ref failed);

            // Combined patterns
            RunTest("Rename: %A%a%0 → ${A}${a}${d0}", () =>
            {
                var result = Pattern_RenameToRegex("%A%a%0");
                AssertEqual(result, "${A}${a}${d0}");
            }, ref passed, ref failed);

            RunTest("Rename: prefix_%A_suffix → prefix_${A}_suffix", () =>
            {
                var result = Pattern_RenameToRegex("prefix_%A_suffix");
                AssertEqual(result, "prefix_${A}_suffix");
            }, ref passed, ref failed);

            // ============================================
            // Exception Tests
            // ============================================
            Console.WriteLine("\n--- Exception Tests ---\n");

            // Invalid placeholder in source (symbols that are NOT a-z, A-Z, 0-9)
            RunTest("Source: %! throws ArgumentException", () =>
            {
                try
                {
                    Pattern_SourceToRegex("%!");
                    throw new Exception("Expected ArgumentException was not thrown");
                }
                catch (ArgumentException ex)
                {
                    if (!ex.Message.Contains("Invalid placeholder"))
                        throw new Exception($"Wrong exception message: {ex.Message}");
                }
            }, ref passed, ref failed);

            RunTest("Source: %@ throws ArgumentException", () =>
            {
                try
                {
                    Pattern_SourceToRegex("%@");
                    throw new Exception("Expected ArgumentException was not thrown");
                }
                catch (ArgumentException ex)
                {
                    if (!ex.Message.Contains("Invalid placeholder"))
                        throw new Exception($"Wrong exception message: {ex.Message}");
                }
            }, ref passed, ref failed);

            // Invalid placeholder in rename (symbols that are NOT a-z, A-Z, 0-9)
            RunTest("Rename: %! throws ArgumentException", () =>
            {
                try
                {
                    Pattern_RenameToRegex("%!");
                    throw new Exception("Expected ArgumentException was not thrown");
                }
                catch (ArgumentException ex)
                {
                    if (!ex.Message.Contains("Invalid placeholder"))
                        throw new Exception($"Wrong exception message: {ex.Message}");
                }
            }, ref passed, ref failed);

            RunTest("Rename: %@ throws ArgumentException", () =>
            {
                try
                {
                    Pattern_RenameToRegex("%@");
                    throw new Exception("Expected ArgumentException was not thrown");
                }
                catch (ArgumentException ex)
                {
                    if (!ex.Message.Contains("Invalid placeholder"))
                        throw new Exception($"Wrong exception message: {ex.Message}");
                }
            }, ref passed, ref failed);

            // Note: %X and %A are VALID (uppercase = greedy), not exceptions
            RunTest("Source: %X is valid (uppercase = greedy)", () =>
            {
                var result = Pattern_SourceToRegex("%X");
                AssertEqual(result, @"(?<X>.*)");
            }, ref passed, ref failed);

            RunTest("Rename: %X is valid (uppercase = greedy)", () =>
            {
                var result = Pattern_RenameToRegex("%X");
                AssertEqual(result, "${X}");
            }, ref passed, ref failed);

            // ============================================
            // Integration Tests (Regex behavior)
            // ============================================
            Console.WriteLine("\n--- Integration Tests ---\n");

            // Test greedy vs non-greedy matching
            // Note: Non-greedy vs greedy only differs when there's content AFTER the placeholder
            RunTest("Integration: Uppercase %A is greedy", () =>
            {
                var source = Pattern_SourceToRegex("%AX*");
                var regex = new Regex($"^{source}$", RegexOptions.IgnoreCase);
                var match = regex.Match("abXaX");
                AssertEqual(match.Groups["A"].Value, "abXa");
            }, ref passed, ref failed);

            RunTest("Integration: Lowercase %a is non-greedy (with suffix)", () =>
            {
                // When there's content AFTER the placeholder, non-greedy will match as little as possible
                var source = Pattern_SourceToRegex("%aX*");
                var regex = new Regex($"^{source}$", RegexOptions.IgnoreCase);
                var match = regex.Match("abXaX");
                // Non-greedy matches as little as possible before X
                AssertEqual(match.Groups["a"].Value, "ab");
            }, ref passed, ref failed);

            RunTest("Integration: Digit %0 captures only digits", () =>
            {
                var source = Pattern_SourceToRegex("%0*");
                var regex = new Regex($"^{source}$", RegexOptions.IgnoreCase);
                var match = regex.Match("123abc");
                AssertEqual(match.Groups["d0"].Value, "123");
            }, ref passed, ref failed);

            RunTest("Integration: Digit %0 rejects non-digits", () =>
            {
                var source = Pattern_SourceToRegex("%0");
                var regex = new Regex($"^{source}$", RegexOptions.IgnoreCase);
                var match = regex.Match("abc");
                AssertEqual(match.Success, false);
            }, ref passed, ref failed);

            // ============================================
            // Summary
            // ============================================
            Console.WriteLine("\n=== Test Summary ===");
            Console.WriteLine($"Passed: {passed}");
            Console.WriteLine($"Failed: {failed}");
            Console.WriteLine($"Total:  {passed + failed}");

            return failed > 0 ? 1 : 0;
        }

        static void RunTest(string name, Action test, ref int passed, ref int failed)
        {
            try
            {
                test();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  [PASS] {name}");
                Console.ResetColor();
                passed++;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [FAIL] {name}");
                Console.WriteLine($"         {ex.Message}");
                Console.ResetColor();
                failed++;
            }
        }

        static void AssertEqual<T>(T actual, T expected)
        {
            if (!EqualityComparer<T>.Default.Equals(actual, expected))
            {
                throw new Exception($"Expected: '{expected}', Actual: '{actual}'");
            }
        }

        // ============================================
        // Copied pattern functions for testing (must match main file)
        // ============================================

        /// <summary> Convert a sourceMask pattern string to a regex string </summary>
        /// <exception cref="ArgumentException">Thrown when an invalid pattern is encountered</exception>
        static string Pattern_SourceToRegex(string pattern)
        {
            string escaped = Regex.Escape(pattern);

            // Use a pattern that requires ALL percent-prefixed sequences to match
            // If nothing matches, the entire match fails and we get no replacement
            // We need to match any % followed by a character to detect invalid ones
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
    }
}