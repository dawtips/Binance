﻿using System.Threading;
using System.Threading.Tasks;
using Binance.Api;
using Binance.Client.Events;

// ReSharper disable once CheckNamespace
namespace Binance.WebSocket.Manager
{
    public static class UserDataWebSocketClientManagerExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task SubscribeAsync(this IUserDataWebSocketManager manager, IBinanceApiUser user, CancellationToken token = default)
            => manager.SubscribeAsync<UserDataEventArgs>(user, null, token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task UnsubscribeAsync(this IUserDataWebSocketManager manager, IBinanceApiUser user, CancellationToken token = default)
            => manager.UnsubscribeAsync<UserDataEventArgs>(user, null, token);
    }
}
