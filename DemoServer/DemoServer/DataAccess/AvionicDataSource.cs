//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using DemoServer.Controllers;
using DemoServer.Models;
using DemoServer.Websocket;
using Microsoft.Extensions.Logging;

namespace DemoServer.DataAccess
{
    public class AvionicDataSource : IAvionicDataSource, IDisposable
    {
        private readonly ILogger<AircraftParameterController> _logger;
        private readonly Timer _avcTimer;
        private readonly List<IWebSocketClientHandler> _webSocketClientList = new List<IWebSocketClientHandler>();
        private readonly DateTime _unixTimeStart = new DateTime(1970, 1, 1);
        private readonly List<AvionicParameterInfo> _infoList = new List<AvionicParameterInfo>();
        private readonly Dictionary<string, AvionicParameter> _parameters = new Dictionary<string, AvionicParameter>();

        private int pos = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvionicDataSource"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public AvionicDataSource(ILogger<AircraftParameterController> logger)
        {
            _logger = logger;

            // Initialize some sample data.
            Init();

            // Create a timer that simulates avionic data.
            // Calls every 100ms = 10Hz.
            _avcTimer = new Timer(100);
            _avcTimer.Elapsed += SimulateData;
            _avcTimer.AutoReset = true;
            _avcTimer.Start();
        }

        /// <inheritdoc />
        public void Subscribe(IWebSocketClientHandler clientHandler)
        {
            _webSocketClientList.Add(clientHandler);
        }

        /// <inheritdoc />
        public void Unsubscribe(IWebSocketClientHandler clientHandler)
        {
            if (_webSocketClientList.Contains(clientHandler))
            {
                _webSocketClientList.Remove(clientHandler);
            }
        }

        /// <inheritdoc />
        public AvionicParameter[] GetParameters()
        {
            return _parameters.Values.ToArray();
        }

        /// <inheritdoc />
        public AvionicParameterInfo[] GetParameterInfos()
        {
            return _infoList.ToArray();
        }

        public void Dispose()
        {
            _avcTimer?.Stop();
            _avcTimer?.Dispose();
        }

        private void Init()
        {
            _infoList.AddRange(
                new[]
                {
                    new AvionicParameterInfo
                    {
                        Key = "akey",
                        Description = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod " +
                                      "tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua.",
                        Type = "String",
                        UnitOfMeasurement = ""
                    },
                    new AvionicParameterInfo
                    {
                        Key = "bkey",
                        Description = "At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no " +
                                      "sea takimata sanctus est Lorem ipsum dolor sit amet.",
                        Type = "Number",
                        UnitOfMeasurement = "kg"
                    },
                    new AvionicParameterInfo
                    {
                        Key = "ckey",
                        Description = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor " +
                                      "invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam " +
                                      "et justo duo dolores et ea rebum.",
                        Type = "Number",
                        UnitOfMeasurement = "knots"
                    },
                    new AvionicParameterInfo
                    {
                        Key = "Date",
                        Description = "Date as Defined in A834 Generic Aicraft Parameters.",
                        Type = "String",
                        UnitOfMeasurement = "DD:MM:YY"
                    },
                    new AvionicParameterInfo
                    {
                        Key = "TimeGMT",
                        Description = "Time as Defined in A834 Generic Aicraft Parameters.",
                        Type = "String",
                        UnitOfMeasurement = "HH:MM:SS.SS"
                    },
                    new AvionicParameterInfo
                    {
                        Key = "EGT1",
                        Description = "Representing RPI Board Temperature.",
                        Type = "Number",
                        UnitOfMeasurement = "°C"
                    }
                });

            var tempParams = new[]
            {
                new AvionicParameter { Key = "akey", Value = "avalue", State = "1", Timestamp = "12312153213" },
                new AvionicParameter { Key = "bkey", Value = "bvalue", State = "0", Timestamp = "17235463835" },
                new AvionicParameter { Key = "ckey", Value = "cvalue", State = "3", Timestamp = "15684651325" },
                new AvionicParameter { Key = "dkey", Value = "dvalue", State = "2", Timestamp = "15684651325" },
                new AvionicParameter { Key = "ekey", Value = "evalue", State = "1", Timestamp = "15684651325" },
                new AvionicParameter { Key = "fkey", Value = "fvalue", State = "1", Timestamp = "15684651325" },
                new AvionicParameter { Key = "gkey", Value = "gvalue", State = "0", Timestamp = "15684651325" },
                new AvionicParameter { Key = "hkey", Value = "hvalue", State = "2", Timestamp = "15684651325" },
                new AvionicParameter { Key = "jkey", Value = "jvalue", State = "3", Timestamp = "15684651325" },
                new AvionicParameter { Key = "Date", Value = "00:00:00", State = "0", Timestamp = "0" },
                new AvionicParameter { Key = "TimeGMT", Value = "00:00:00.00", State = "0", Timestamp = "0" },
                new AvionicParameter { Key = "EGT1", Value = "0", State = "0", Timestamp = "0" }
            };

            foreach (var p in tempParams)
            {
                _parameters.Add(p.Key, p);
            }
        }

        /// <summary>
        /// Method that is called if avcTimer is triggered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimulateData(object sender, ElapsedEventArgs e)
        {
            try
            {
                var timeStamp = $"{DateTime.UtcNow.Subtract(_unixTimeStart).TotalMilliseconds:.0}";

                switch (pos++ % 10)
                {
                    case 1:
                        // Update a value
                        break;
                    case 2:
                        // Update a value
                        break;
                    case 3:
                        // Update a value
                        break;
                    case 4:
                        // Update a value
                        break;
                    case 5:
                        // Update a value
                        break;
                    case 6:
                        // Update a value
                        break;
                    case 7:
                        // Update a value
                        break;
                    case 8:
                        // Update a value
                        break;
                    case 9:
                        // Update a value
                        break;
                    default:
                        var newDate = DateTime.UtcNow.ToString("yy-MM-dd");
                        var newTime = DateTime.UtcNow.ToString("HH:mm:ss.ff");
                        UpdateItem("Date", newDate, "1", timeStamp);
                        UpdateItem("TimeGMT", newTime, "1", timeStamp);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Updating Data: ", ex);
            }
        }

        /// <summary>
        /// Updates the the parameter in the parameter list.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <param name="timeStamp"></param>
        private void UpdateItem(string key, string value, string state, string timeStamp)
        {
            if (!_parameters.ContainsKey(key))
            {
                return;
            }

            var item = _parameters[key];
            item.Timestamp = timeStamp;
            item.Value = value;
            item.State = state;

            foreach (var client in _webSocketClientList)
            {
                client.UpdateParameter(item);
            }
        }
    }
}