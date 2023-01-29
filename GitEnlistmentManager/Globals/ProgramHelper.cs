using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Globals
{
    public static class ProgramHelper
    {
        // TODO: is it possible to just redirect calls to the main runprogram in mainwindow.cs to here too?
        public static async Task<bool> RunProgram(
            string? programPath,
            string? arguments,
            Dictionary<string, string>? tokens,
            bool openNewWindow,
            string? workingFolder,
            Func<string, Task>? metaOutputHandler = null,
            Func<string, Task>? outputHandler = null,
            Func<string, Task>? errorHandler = null
            )
        {
            metaOutputHandler ??= async (s) => { await Task.Delay(0).ConfigureAwait(false); };
            outputHandler ??= async (s) => { await Task.Delay(0).ConfigureAwait(false); };
            errorHandler ??= async (s) => { await Task.Delay(0).ConfigureAwait(false); };

            // Replace tokens that come from GEM. These are in the format of {token}
            if (tokens != null)
            {
                foreach (var token in tokens)
                {
                    programPath = programPath?.Replace($"{{{token.Key}}}", token.Value, StringComparison.OrdinalIgnoreCase);
                    arguments = arguments?.Replace($"{{{token.Key}}}", token.Value, StringComparison.OrdinalIgnoreCase);
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

                programPath = programPath?.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
                arguments = arguments?.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
            }

            if (workingFolder != null)
            {
                // I have no idea why this line needs an extra Environment.NewLine added but the others don't
                await metaOutputHandler($"cd \"{workingFolder}\"{Environment.NewLine}").ConfigureAwait(false);
            }
            await metaOutputHandler($"\"{programPath}\" {arguments}").ConfigureAwait(false);

            bool useShellExecute = false;
            // We need shell execute to open urls
            if (programPath != null && programPath.StartsWith("http"))
            {
                useShellExecute = true;
            }

            using Process process = new();
            process.StartInfo = new()
            {
                FileName = programPath,
                Arguments = arguments,
                UseShellExecute = useShellExecute,
                WindowStyle = openNewWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                RedirectStandardOutput = !useShellExecute,
                RedirectStandardError = !useShellExecute,
                CreateNoWindow = !openNewWindow,
                WorkingDirectory = workingFolder
            };

            if (!useShellExecute)
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
            process.Start();
            if (!useShellExecute)
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
            // Exit code 0 is success. This works for git, but won't work for things like RoboCopy.
            return exitCode == 0;
        }
    }
}
