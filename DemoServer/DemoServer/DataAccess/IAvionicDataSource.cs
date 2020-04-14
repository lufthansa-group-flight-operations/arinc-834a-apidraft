//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using DemoServer.Models;
using DemoServer.Websocket;

namespace DemoServer.DataAccess
{
    public interface IAvionicDataSource
    {
        void Subscribe(IWebSocketClientHandler clientHandler);

        void Unsubscribe(IWebSocketClientHandler clientHandler);

        /// <summary>
        /// Adds a Websocket Client to the List of websocket Clients.
        /// </summary>
        /// <returns>Array of avionic parameter.</returns>
        AvionicParameter[] GetParameters();

        /// <summary>
        /// Gets the parameter for HTTP requests.
        /// </summary>
        /// <returns>Array of avionic parameter infos.</returns>
        AvionicParameterInfo[] GetParameterInfos();
    }
}