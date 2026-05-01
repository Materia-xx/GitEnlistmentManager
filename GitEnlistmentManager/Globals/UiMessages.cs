using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace GitEnlistmentManager.Globals
{
    /// <summary>
    /// Indirection over <see cref="MessageBox.Show(string)"/> so that errors raised while a
    /// command set is being dispatched from MCP can be captured and returned in the MCP
    /// response instead of popping a modal dialog.
    ///
    /// CLI invocations (and any flow that does not install a sink) keep the original
    /// MessageBox behavior. Each invocation establishes its own sink via
    /// <see cref="CaptureErrors"/>, and the sink is stored in an <see cref="AsyncLocal{T}"/>
    /// so concurrent CLI and MCP flows do not bleed into each other.
    ///
    /// <para>
    /// Gotcha — fire-and-forget work: <see cref="AsyncLocal{T}"/> flows along the logical
    /// async control flow via <see cref="System.Threading.ExecutionContext"/>. If a command
    /// (or anything it calls) spawns work with <c>Task.Run</c>, <c>_ = SomeAsync()</c>, or
    /// <c>async void</c> and does NOT await it before the surrounding
    /// <see cref="CaptureErrors"/> scope is disposed, the spawned work inherits the captured
    /// sink at the moment it was spawned. After the scope disposes, errors raised by that
    /// work land in a sink list nobody is observing anymore (silent drop from the user's
    /// perspective), and they will NOT bleed into a different concurrent caller's sink — but
    /// they also will NOT be surfaced as a MessageBox.
    /// </para>
    /// <para>
    /// Rule of thumb for command authors: await all async work that can call
    /// <see cref="ShowError(string)"/>, or capture errors locally inside the spawned task.
    /// Avoid <c>async void</c> in command code paths.
    /// </para>
    /// </summary>
    public static class UiMessages
    {
        private static readonly AsyncLocal<List<string>?> currentErrorSink = new AsyncLocal<List<string>?>();
        private static readonly AsyncLocal<List<string>?> currentInfoSink = new AsyncLocal<List<string>?>();

        /// <summary>
        /// Push an error sink onto the current async flow. Errors reported via
        /// <see cref="ShowError(string)"/> while the returned scope is alive go to the sink
        /// list instead of <see cref="MessageBox.Show(string)"/>.
        /// </summary>
        public static IDisposable CaptureErrors(List<string> sink)
        {
            var prior = currentErrorSink.Value;
            currentErrorSink.Value = sink;
            return new SinkScope(s => currentErrorSink.Value = s, prior);
        }

        /// <summary>
        /// Push an info sink onto the current async flow. Informational messages reported via
        /// <see cref="ShowInfo(string)"/> while the returned scope is alive go to the sink
        /// list. Info messages have no GUI/CLI surface — when no sink is installed,
        /// <see cref="ShowInfo(string)"/> is a no-op. The channel exists so MCP responses can
        /// describe what happened (e.g. "Launched devenv.exe (pid 1234), not awaiting") without
        /// popping dialogs at users running from the GUI or CLI.
        ///
        /// Same fire-and-forget gotcha as <see cref="CaptureErrors"/>: messages produced by
        /// un-awaited spawned work after the scope disposes are silently dropped.
        /// </summary>
        public static IDisposable CaptureInfo(List<string> sink)
        {
            var prior = currentInfoSink.Value;
            currentInfoSink.Value = sink;
            return new SinkScope(s => currentInfoSink.Value = s, prior);
        }

        /// <summary>
        /// Report an error to the user. If an error sink is installed for the current async
        /// flow the message is added to the sink; otherwise it is shown as a modal MessageBox.
        /// </summary>
        public static void ShowError(string message)
        {
            var sink = currentErrorSink.Value;
            if (sink != null)
            {
                sink.Add(message);
                return;
            }

            MessageBox.Show(message);
        }

        /// <summary>
        /// Report informational text intended for MCP callers. If an info sink is installed
        /// the message is added; otherwise the call is a no-op (no MessageBox, no log). Use
        /// for structured success details that an AI consumer may find useful (process IDs,
        /// counts, warnings that don't justify failing the dispatch, etc.).
        /// </summary>
        public static void ShowInfo(string message)
        {
            var sink = currentInfoSink.Value;
            if (sink != null)
            {
                sink.Add(message);
            }
        }

        private sealed class SinkScope : IDisposable
        {
            private readonly Action<List<string>?> restore;
            private readonly List<string>? prior;
            private bool disposed;

            public SinkScope(Action<List<string>?> restore, List<string>? prior)
            {
                this.restore = restore;
                this.prior = prior;
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }
                disposed = true;
                restore(prior);
            }
        }
    }
}
