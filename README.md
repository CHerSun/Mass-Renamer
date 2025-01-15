Mass Renamer is a simple console tool to bulk rename files using either simple patterns or regex patterns. `mren` is the short name for it - it looks unique enough not to overlap with other commands.

## Why?

My workflows often produce quite a lot of similarly named files, which I need to rename using a pattern only, not file properties. Doing that manually is tedious.

There are quite a lot of tools for bulk renaming on Windows. Problem is:
 - they are too difficult, with miriad of options, which makes them difficult to use and set up, especially so for small simple renamings
 - most of them are GUI oriented - that's not always a desirable thing
 - I wanted something as simple as possible, keeping it repeatable.

You are free to use it as you wish. Any suggestions or pull requests are welcome.

## How to use

You can download the latest compiled executable from [Releases](https://github.com/CHerSun/Mass-Renamer/releases/latest) or compile yourself (see below). Put executable anywhere you like on your PATH variable. In console call `mren` with arguments:

- folder where to look for files (`.` for the current folder),
- source mask (see below),
- rename pattern (see below).

By default `mren` does a DRY RUN only, i.e. no action is actually taken. If you are satisfied with the preview of action - add `-y` option to apply.

> NOTE: Currently there's no way to undo changes. If you are using recursive `-r` mode or overwrite `-w` mode - make sure to ALWAYS DO A DRY RUN first.

## Source mask and Rename pattern

For source mask and rename pattern - there are 2 modes:

- simple pattern mode (default):
  - uses substitutions:
    - `%a` ... `%z` - any number of any characters, ungreedy; remembers what it matched to be used in rename pattern;
    - `%0` ... `%9` - 1 or more digits, greedy; remembers what it matched to be used in rename pattern;
    - `*` - any number of any characters, discarded, i.e. cannot be used in rename pattern;
    - `?` - any single character, discarded, i.e. cannot be used in rename pattern;
    - `%%` - escape `%` - i.e. results in single literal `%` symbol.
  - all other characters are treated literally;
  - simple patterns are converted into regex patterns internally; there could be some bugs with this conversion.
- regex patterns mode (`-p` option):
  - uses .NET (C#) regex patterns and substitutions syntax for both source mask and rename pattern; refer either to Microsoft docs or to regex101.com C# docs for regex syntax details.

In both modes we must match a full relative (to target folder) name string, so start (`^`) & end (`$`) anchors are added in the background. You don't need to add them.

Also, case INsensitive matching is used, hard-coded for now. Can be overriden with Regex Options flags right in the Source pattern (see [Specify options - inline](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options#specify-options)).

### Example 1 - simple pattern

Say we have a number of files called something like `Some title.mkv_snapshot_01.02.03.jpg` (MPC-HC screenshots). We want to rename those to `01.02.03 Some title.jpg` form for the sake of comparisson.

FIRST STEP - We can do a preview:

```sh
mren . "%A.mkv_snapshot_%B.jpg" "%B %A.jpg"
```

> NOTE: if `mren.exe` is not on your PATH variable - you might need to either run as `./mren.exe` if the file is located in the current folder, or to use a full path to the executable.

Which gives us output like:

```
Scanning for files in ".", using patterns "%A.mkv_snapshot_%B.jpg" ---> "%B %A.jpg".

Sample match:
  "Some title.mkv_snapshot_01.02.03.jpg"
  1: A = "Some title"
  2: B = "01.02.03"

Would rename files:
  "Some title.mkv_snapshot_01.02.03.jpg" ···> "01.02.03 Some title.jpg"

Files matched: 1 out of 1 found
Files to be renamed: 1
```

See the `Would rename files:` section for final results. Now we are satisfied with the preview and can add `-y` option to run the action:

```sh
mren . '%A.mkv_snapshot_%B.jpg' '%B %A.jpg' -y
```

### Example 1 - regex patterns

We can do the same thing as before using regex pattern and substitution directly (`-p` option).

Let's do the dry run using regex (`-p` option):

```sh
mren . "(.*)\.mkv_snapshot_(.*)\.jpg" '$2 $1.jpg' -p
```

> NOTE: Regex pattern in the command above is intentionally simplified to not use named caption groups. If you are an advanced regex user - you can use full power of C# flavor regex.

> NOTE the single quotes `'` used in the command above - shells (PowerShell here) can interfere with program execution by substituting environment variables - `$1` and `$2` in this case - would be replaced with empty strings if we use normal quotes `"`. The program would receive ` .jpg` instead of `$2 $1.jpg` if `"` quotes were used.

We'll get a similar output from regex:

```
Scanning for files in ".", using patterns "(.*)\.mkv_snapshot_(.*)\.jpg" ---> "$2 $1.jpg".

Sample match:
  "Some title.mkv_snapshot_01.02.03.jpg"
  1: 1 = "Some title"
  2: 2 = "01.02.03"

Would rename files:
  "Some title.mkv_snapshot_01.02.03.jpg" ···> "01.02.03 Some title.jpg"

Files matched: 1 out of 1 found
Files to be renamed: 1
```

The only difference is that we now have full regex power (well, complexity too).

## Syntax

`mren` syntax help that you can get via the program itself:

```
Description:
  Mass Renamer - a tool to rename files in bulk using either glob-like or regex patterns.

Usage:
  mren <TargetFolder> <SourceMask> <RenamePattern> [options]

Arguments:
  <TargetFolder>   The target folder where to rename files. Relative and absolute paths could be used.
  <SourceMask>     The source mask for matching files.
                   In glob-like mode (default) pattern must match full filename. Pattern supports named matches in form of %A ... %Z for any text, %0 ... %9
                   for numeric matches, %% for % escaping. You can also use '*' and '?' as wildcards, but those will be omitted in the result.
                   Alternatively you can use '-p' flag and use C# regex pattern directly.
  <RenamePattern>  The pattern to rename files to.
                   Glob-like pattern (default) allows to use named matches from SourceMask in form of %A ... %Z, %0 ... %9. You can use %% for % escaping in
                   this mode.
                   Alternatively you can use '-p' flag and use C# regex substitutions directly.

Options:
  -y, --apply      Apply changes. Dry run otherwise.
  -r, --recursive  Process files recursively (with sub-folders).
                   If specified - filename relative to TargetFolder will be used.
                   You SHOULD do a dry run (without '-y'), as logic is a bit different.
  -p, --pattern    Treat SourceMask and RenamePattern as regex PATTERNS directly. '-p' to avoid confusion with recursive.
  -w, --overwrite  Overwrite files during renaming, if target already exists.
                   CARE !!! DESTRUCTIVE !!!
  --version        Show version information
  -?, -h, --help   Show help and usage information
```

## Compilation from sources

I've set up a single-file publishing with only `en-US` locale, reliant on .NET 8.0. To get the executable yourself (~350 KiB):

- clone the repo
- go into solution or project folder
- run `dotnet publish` (.NET 8.0 SDK must be installed)

Other build options:

- Build a self-contained executable - set SelfContained to true in csproj file. Results in ~60 MiB file. It won't be reliant on .NET framework. Could be trimmed to reduce the size.
- Build an NativeAOT executable - in csproj file uncomment PublishAot, comment PublishSingleFile. Results in ~4 MiB file, natively compiled. It won't be reliant on .NET framework.
