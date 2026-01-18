using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    /// <summary>
    /// This is used to suspend the main loop when a command is run
    /// </summary>
    public sealed class AsyncSuspendGate
    {
        /// <summary>The synchronization object</summary>
        private readonly object syncRoot = new();
        /// <summary>The suspend count</summary>
        private int suspendCount;
        /// <summary>Completed when not suspended; pending when suspended.</summary>
        private TaskCompletionSource<bool> resumeTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSuspendGate"/> class.
        /// </summary>
        public AsyncSuspendGate()
        {
            resumeTcs.TrySetResult(true); // start "running"
        }

        /// <summary>
        /// Gets a value indicating whether this instance is suspended.
        /// </summary>
        public bool IsSuspended
        {
            get { lock (syncRoot) return suspendCount > 0; }
        }

        /// <summary>
        /// Waits the until resumed.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        public Task WaitUntilResumedAsync(CancellationToken ct = default)
        {
            Task task;
            lock (syncRoot) task = resumeTcs.Task;
            return ct.CanBeCanceled ? task.WaitAsync(ct) : task;
        }

        /// <summary>
        /// Suspends this instance.
        /// </summary>
        public IDisposable Suspend()
        {
            lock (syncRoot)
            {
                if (suspendCount == 0) resumeTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
                suspendCount++;
            }

            return new Releaser(this);
        }

        /// <summary>
        /// Releases this instance.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Suspend count underflow.</exception>
        private void Release()
        {
            TaskCompletionSource<bool>? toComplete = null;

            lock (syncRoot)
            {
                if (suspendCount <= 0) throw new InvalidOperationException("Suspend count underflow.");
                suspendCount--;
                if (suspendCount == 0) toComplete = resumeTcs;
            }

            toComplete?.TrySetResult(true);
        }

        /// <summary>
        /// Class to release the suspend gate when disposed
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        private sealed class Releaser : IDisposable
        {
            private AsyncSuspendGate? gate;
            public Releaser(AsyncSuspendGate gate) => this.gate = gate;
            public void Dispose() => Interlocked.Exchange(ref gate, null)?.Release();
        }
    }
}
