﻿using System;
using System.Threading;
using Binance.Market;
using Binance.WebSocket.Events;
using Microsoft.Extensions.Logging;

namespace Binance.WebSocket.Manager
{
    internal sealed class CandlestickWebSocketClientAdapter : WebSocketClientAdapter<ICandlestickWebSocketClient>, ICandlestickWebSocketClient
    {
        #region Public Events

        public event EventHandler<CandlestickEventArgs> Candlestick
        {
            add { Client.Candlestick += value; }
            remove { Client.Candlestick -= value; }
        }

        #endregion Public Events

        #region Constructors

        public CandlestickWebSocketClientAdapter(IBinanceWebSocketClientManager manager, ICandlestickWebSocketClient client, ILogger<IBinanceWebSocketClientManager> logger = null, Action<Exception> onError = null)
            : base(manager, client, logger, onError)
        { }

        #endregion Constructors

        #region Public Methods

        public async void Subscribe(string symbol, CandlestickInterval interval, Action<CandlestickEventArgs> callback)
        {
            try
            {
                Logger?.LogDebug($"{nameof(CandlestickWebSocketClientAdapter)}.{nameof(Subscribe)}: Cancel streaming...  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                await _controller.CancelAsync()
                    .ConfigureAwait(false);

                Client.Subscribe(symbol, interval, callback);

                if (!Manager.IsAutoStreamingDisabled && !_controller.IsActive)
                {
                    Logger?.LogDebug($"{nameof(CandlestickWebSocketClientAdapter)}.{nameof(Subscribe)}: Begin streaming...  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                    _controller.Begin();
                }
            }
            catch (OperationCanceledException) { /* ignored */ }
            catch (Exception e)
            {
                Logger?.LogError(e, $"{nameof(CandlestickWebSocketClientAdapter)}.{nameof(Subscribe)}: Failed.  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                OnError?.Invoke(e);
            }
        }

        public async void Unsubscribe(string symbol, CandlestickInterval interval, Action<CandlestickEventArgs> callback)
        {
            try
            {
                Logger?.LogDebug($"{nameof(CandlestickWebSocketClientAdapter)}.{nameof(Unsubscribe)}: Cancel streaming...  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                await _controller.CancelAsync()
                    .ConfigureAwait(false);

                Client.Unsubscribe(symbol, interval, callback);

                if (!Manager.IsAutoStreamingDisabled && !_controller.IsActive)
                {
                    Logger?.LogDebug($"{nameof(CandlestickWebSocketClientAdapter)}.{nameof(Unsubscribe)}: Begin streaming...  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                    _controller.Begin();
                }
            }
            catch (OperationCanceledException) { /* ignored */ }
            catch (Exception e)
            {
                Logger?.LogError(e, $"{nameof(CandlestickWebSocketClientAdapter)}.{nameof(Unsubscribe)}: Failed.  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                OnError?.Invoke(e);
            }
        }

        #endregion Public Methods
    }
}
