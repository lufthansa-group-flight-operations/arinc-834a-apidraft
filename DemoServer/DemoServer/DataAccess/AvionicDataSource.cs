//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Timers;
using DemoServer.Controllers;
using DemoServer.Models;
using DemoServer.WebSockets;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace DemoServer.DataAccess
{
    public class AvionicDataSource : IAvionicDataSource, IDisposable
    {
        private readonly ILogger<AircraftParameterController> _logger;
        private readonly Timer _avcTimer;
        private readonly List<IWebSocketClientHandlerAcParameter> _webSocketClientList = new List<IWebSocketClientHandlerAcParameter>();
        private readonly DateTime _unixTimeStart = new DateTime(1970, 1, 1);        
        private readonly Dictionary<string, AvionicParameter> _parameters = new Dictionary<string, AvionicParameter>();

        private int pos = 0;

        public AvionicParameter[] KnownParams { get; private set; }

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
        public void Subscribe(IWebSocketClientHandlerAcParameter clientHandler)
        {
            _webSocketClientList.Add(clientHandler);
        }

        /// <inheritdoc />
        public void Unsubscribe(IWebSocketClientHandlerAcParameter clientHandler)
        {
            if (_webSocketClientList.Contains(clientHandler))
            {
                _webSocketClientList.Remove(clientHandler);
            }
        }

        /// <inheritdoc />
        public AvionicParameter? GetParameter(string paramName)
        {
            AvionicParameter result;            
            if (_parameters.TryGetValue(paramName, out result))
            {
                return result;
            }

            return null;
        }

        /// <inheritdoc />
        public AvionicParameter[] GetParameters()
        {
            return _parameters.Values.ToArray();
        }

        

        public void Dispose()
        {
            _avcTimer?.Stop();
            _avcTimer?.Dispose();
        }

        private void Init()
        {
            KnownParams = new[]
            {
                new AvionicParameter { Name = "airline_id", Settable = false},
                new AvionicParameter { Name = "ac_icao24", Settable = false},
                new AvionicParameter { Name = "ac_reg", Settable = false},
                new AvionicParameter { Name = "ac_type", Settable = false},
                new AvionicParameter { Name = "time_utc" , Settable = false},
                new AvionicParameter { Name = "gnss_time_utc", Settable = false},
                new AvionicParameter { Name = "date_utc", Settable = false},
                new AvionicParameter { Name = "gnss_date_utc", Settable = false},
                new AvionicParameter { Name = "origin", Settable = false},
                new AvionicParameter { Name = "destination", Settable = false},
                new AvionicParameter { Name = "dist_to_dest", Settable = false},
                new AvionicParameter { Name = "dist_to_wypt", Settable = false},
                new AvionicParameter { Name = "time_to_dest", Settable = false},
                new AvionicParameter { Name = "time_to_waypt", Settable = false},
                new AvionicParameter { Name = "flight_no", Settable = false},
                new AvionicParameter { Name = "EGT1", Settable = false}
            };


            var tempParams = new[]
            {
                new AvionicParameter { Name = "airline_id", Value = "avalue", Timestamp = 12312153213 },
                new AvionicParameter { Name = "ac_icao24", Value = "A0AB42", Timestamp = 17235463835 },
                new AvionicParameter { Name = "ac_reg", Value = "N142MS", Timestamp = 15684651325 },
                new AvionicParameter { Name = "ac_type", Value = "P28U", Timestamp = 15684651325 },
                new AvionicParameter { Name = "time_utc", Value = "11:55:00", Timestamp = 15684651325 },
                new AvionicParameter { Name = "gnss_time_utc", Value = "11:55:00", Timestamp = 15684651325 },
                new AvionicParameter { Name = "date_utc", Value = "25.09.2021", Timestamp = 15684651325 },
                new AvionicParameter { Name = "gnss_date_utc", Value = "25.09.2021", Timestamp = 15684651325 },
                new AvionicParameter { Name = "origin", Value = "KMIA", Timestamp = 15684651325 },
                new AvionicParameter { Name = "destination", Value = "KFMY", Timestamp = 15684651325 },
                new AvionicParameter { Name = "dist_to_dest", Value = "120", Timestamp = 15684651325 },
                new AvionicParameter { Name = "dist_to_wypt", Value = "20", Timestamp = 15684651325 },
                new AvionicParameter { Name = "time_to_dest", Value = (int)60, Timestamp = 15684651325 },
                new AvionicParameter { Name = "time_to_waypt", Value = "10", Timestamp = 15684651325 },
                new AvionicParameter { Name = "flight_no", Value = "", Timestamp = 0 },
                new AvionicParameter { Name = "EGT1", Value = "0", Timestamp = 0 }
            };

            foreach (var p in tempParams)
            {
                _parameters.Add(p.Name, p);                
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
                var timeStamp = DateTime.UtcNow.Subtract(_unixTimeStart).TotalMilliseconds;

                foreach (var item in _parameters)
                {
                    switch (item.Key)
                    {
                        case "date_utc":
                            var newDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                            UpdateItem("date_utc", newDate, (long)timeStamp);
                            break;
                        case "time_utc":
                            var newTime = DateTime.UtcNow.ToString("HH:mm:ss.ff");
                            UpdateItem("time_utc", newTime, (long)timeStamp);
                            break;
                        default:
                            UpdateItem(item.Key, item.Value.Value, (long)timeStamp);
                            break;
                    }

                }

                // Untersetzungsgestriebe
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
        private void UpdateItem(string key, object value, long timeStamp)
        {
            if (!_parameters.ContainsKey(key))
            {
                return;
            }

            var item = _parameters[key];
            item.Timestamp = timeStamp;
            item.Value = value;            

            foreach (var client in _webSocketClientList)
            {
                client.UpdateParameter(item);
                //client.UpdateParameter(new AvionicParameterData { Name = item.Name, Timestamp = item.Timestamp, Value = item.Value });
            }
        }
    }

    public record struct AvionicParameterData
    {
        
        public string Name { get; init; }

        
        public object? Value { get; set; }

        
        public long? Timestamp { get; set; }

        
        
        public bool? Settable { get; set; }
    }
}