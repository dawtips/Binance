﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Binance.Utility
{
    public class RetryTaskController : TaskController, IRetryTaskController
    {
        #region Public Events

        public event EventHandler<PausingEventArgs> Pausing
        {
            add
            {
                if (_pausing == null || !_pausing.GetInvocationList().Contains(value))
                {
                    _pausing += value;
                }
            }
            remove => _pausing -= value;
        }
        private EventHandler<PausingEventArgs> _pausing;

        public event EventHandler<EventArgs> Resuming
        {
            add
            {
                if (_resuming == null || !_resuming.GetInvocationList().Contains(value))
                {
                    _resuming += value;
                }
            }
            remove => _resuming -= value;
        }
        private EventHandler<EventArgs> _resuming;

        #endregion Public Events

        #region Public Properties

        public int RetryDelayMilliseconds { get; set; } = 5000;

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="logger"></param>
        public RetryTaskController(Func<CancellationToken, Task> action = null, ILogger<RetryTaskController> logger = null)
            : base(action, logger)
        { }

        #endregion Constructors

        #region Public Methods

        public override void Begin(Func<CancellationToken, Task> action = null)
        {
            ThrowIfDisposed();

            lock (Sync)
            {
                if (IsActive)
                    return;

                if (action != null)
                    Action = action;

                Throw.IfNull(Action, nameof(action));

                IsActive = true;

                Cts?.Dispose();

                Cts = new CancellationTokenSource();
            }

            Task = Task.Run(async () =>
            {
                Logger?.LogDebug($"{nameof(RetryTaskController)}: Task beginning...  [thread: {Thread.CurrentThread.ManagedThreadId}]");

                while (!Cts.IsCancellationRequested)
                {
                    try
                    {
                        await Action(Cts.Token)
                            .ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { /* ignore */ }
                    catch (Exception e)
                    {
                        Logger?.LogWarning(e, $"{nameof(RetryTaskController)}: Unhandled action exception.  [thread: {Thread.CurrentThread.ManagedThreadId}]");

                        if (!Cts.IsCancellationRequested)
                        {
                            OnError(e);
                        }
                    }

                    try
                    {
                        if (!Cts.IsCancellationRequested)
                        {
                            Logger?.LogDebug($"{nameof(RetryTaskController)}: Task pausing...  [thread: {Thread.CurrentThread.ManagedThreadId}]");

                            await DelayAsync(Cts.Token)
                                .ConfigureAwait(false);
                        }
                    }
                    catch { /* ignore */ }

                    if (!Cts.IsCancellationRequested)
                    {
                        Logger?.LogDebug($"{nameof(RetryTaskController)}: Task resuming...  [thread: {Thread.CurrentThread.ManagedThreadId}]");

                        OnResuming();
                    }
                }

                Logger?.LogDebug($"{nameof(RetryTaskController)}: Task complete.  [thread: {Thread.CurrentThread.ManagedThreadId}]");
            });
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual Task DelayAsync(CancellationToken token)
        {
            // Notify listeners.
            OnPausing(TimeSpan.FromMilliseconds(RetryDelayMilliseconds));

            return Task.Delay(RetryDelayMilliseconds, token);
        }

        /// <summary>
        /// Raise a pausing event.
        /// </summary>
        /// <param name="timeSpan"></param>
        protected void OnPausing(TimeSpan timeSpan)
        {
            try { _pausing?.Invoke(this, new PausingEventArgs(timeSpan)); }
            catch { /* ignore */ }
        }

        /// <summary>
        /// Raise a resuming event.
        /// </summary>
        protected void OnResuming()
        {
            try { _resuming?.Invoke(this, EventArgs.Empty); }
            catch { /* ignore */ }
        }

        #endregion Protected Methods
    }
}
