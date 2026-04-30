using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Globals
{
    public static class ProgramHelper
    {
        public static string? ResolveTokens(string? input, Dictionary<string, string>? tokens)
        {
            if (input == null)
            {
                return null;
            }

            // Replace tokens that come from GEM. These are in the format of {token}
            if (tokens != null)
            {
                foreach (var token in tokens)
                {
                    input = input.Replace($"{{{token.Key}}}", token.Value, StringComparison.OrdinalIgnoreCase);
                }
            }

            // Replace environment variables formatted like %comspec%
            var envVarsCaseSensitive = Environment.GetEnvironmentVariables();
            var envVarsCaseInsensitive = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var envVarName in envVarsCaseSensitive.Keys)
            {
                var name = envVarName.ToString();
                var value = envVarsCaseSensitive[envVarName]?.ToString();
                if (value != null && name != null)
                {
                    envVarsCaseInsensitive[name] = value;
                }
            }
            foreach (var envVarName in envVarsCaseInsensitive.Keys)
            {
                var find = $"%{envVarName}%";
                var replace = envVarsCaseInsensitive[envVarName];

                input = input.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
            }

            return input;
        }

        public static async Task<bool> RunProgram(
            string? programPath,
            string? arguments,
            Dictionary<string, string>? tokens,
            bool useShellExecute,
            bool openNewWindow,
            string? workingDirectory,
            Func<string, Task>? metaOutputHandler = null,
            Func<string, Task>? outputHandler = null,
            Func<string, Task>? errorHandler = null,
            // Exit code 0 is success. This works for git, but won't work for things like RoboCopy.
            int successfulExitCode = 0,
            // When true, the spawned process is launched and this method returns immediately
            // without awaiting WaitForExitAsync. Useful for long-lived UI launchers (devenv,
            // dev command prompts, diff GUIs, browser-launched URLs) so MCP callers don't
            // block until the user closes the launched program. Stdout/stderr are NOT
            // redirected when fire-and-forget is in effect (the process inherits handles or
            // discards them depending on shell-execute), so live capture is unavailable.
            bool fireAndForget = false
            )
        {
            metaOutputHandler ??= async (s) => { await Task.Delay(0).ConfigureAwait(false); };
            outputHandler ??= async (s) => { await Task.Delay(0).ConfigureAwait(false); };
            errorHandler ??= async (s) => { await Task.Delay(0).ConfigureAwait(false); };

            programPath = ProgramHelper.ResolveTokens(programPath, tokens);
            arguments = ProgramHelper.ResolveTokens(arguments, tokens);
            workingDirectory = ProgramHelper.ResolveTokens(workingDirectory, tokens);

            if (workingDirectory != null)
            {
                await metaOutputHandler($"cd \"{workingDirectory}\"").ConfigureAwait(false);
            }
            await metaOutputHandler($"\"{programPath}\" {arguments}").ConfigureAwait(false);

            var redirectStreams = !useShellExecute && !fireAndForget;

            using Process process = new();
            process.StartInfo = new()
            {
                FileName = programPath,
                Arguments = arguments,
                UseShellExecute = useShellExecute,
                WindowStyle = openNewWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                RedirectStandardOutput = redirectStreams,
                RedirectStandardError = redirectStreams,
                CreateNoWindow = !openNewWindow,
                WorkingDirectory = workingDirectory
            };

            if (redirectStreams)
            {
                process.OutputDataReceived += new DataReceivedEventHandler(async (s,e) =>
                {
                    if (e.Data != null)
                    {
                        await outputHandler(e.Data).ConfigureAwait(false);
                    }
                });
                process.ErrorDataReceived += new DataReceivedEventHandler(async (s, e) =>
                {
                    if (e.Data != null)
                    {
                        await errorHandler(e.Data).ConfigureAwait(false);
                    }
                });
            }

            if (!useShellExecute)
            {
                // UseShellExecute must be false in order to use environment variables
                // Inject the path to where GEM is running from into the environment path so it's callable from the commandline.
                // A commandset that opens the VS command prompt could make use of this so gem is callable from that commandline
                var gemExe = Assembly.GetExecutingAssembly().Location;

                string? gemExeDirectory = null;
                if (gemExe != null)
                {
                    gemExeDirectory = new FileInfo(gemExe)?.Directory?.FullName;
                }
                process.StartInfo.Environment["Path"] = $"{Environment.GetEnvironmentVariable("Path")};{gemExeDirectory}";
            }

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                UiMessages.ShowError(ex.ToString());
                return false;
            }

            if (fireAndForget)
            {
                int? pid = null;
                try { pid = process.Id; } catch { }
                UiMessages.ShowInfo($"Launched '{programPath}' (pid {pid?.ToString() ?? "?"}). Process not awaited; success only indicates the launcher process started.");
                await metaOutputHandler(string.Empty).ConfigureAwait(false);
                return true;
            }

            if (redirectStreams)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            await process.WaitForExitAsync().ConfigureAwait(false);
            process.Refresh();

            var exitCode = -1;
            if (process.HasExited)
            {
                exitCode = process.ExitCode;
            }
            process.Close();

            await metaOutputHandler(string.Empty).ConfigureAwait(false);
            return exitCode == successfulExitCode;
        }

        public static async Task OpenDirectory(string path)
        {
            await ProgramHelper.RunProgram(
                programPath: "Explorer.exe",
                arguments: path,
                tokens: null,
                useShellExecute: false,
                workingDirectory: null,
                openNewWindow: true
                ).ConfigureAwait(false);
        }
    }
}
