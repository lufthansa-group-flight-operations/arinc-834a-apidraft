//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using DemoServer.Models;
using DemoServer.WebSockets;

namespace DemoServer.DataAccess
{
    public interface IAvionicDataSource
    {
        /// <summary>
        /// Gets the known paramters names.
        /// </summary>
        AvionicParameter[] KnownParams { get; }

        void Subscribe(IWebSocketClientHandlerAcParameter clientHandler);

        void Unsubscribe(IWebSocketClientHandlerAcParameter clientHandler);

        /// <summary>
        /// Gets all AvionicParameters.
        /// </summary>
        /// <returns>Array of avionic parameter.</returns>
        AvionicParameter[] GetParameters();
        
        /// <summary>
        /// Gets a single Paramter.
        /// </summary>
        /// <param name="paramName">Name of paramter.</param>
        /// <returns>Return null if no matching parameter is found.</returns>
        AvionicParameter? GetParameter(string paramName);
    }
}