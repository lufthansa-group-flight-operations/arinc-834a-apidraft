//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using StapServer.Hardware;

namespace StapServer.Protocol
{
    public class StapClientConnection
    {
        private readonly ILogger logger;
        private readonly TcpClient tcpClient;
        private readonly Thread handleClientThread;
        private readonly NetworkStream stream;
        private readonly StreamReader reader;
        private readonly StreamWriter writer;
        private readonly IHardware hardware;

        private readonly Dictionary<string, Func<string[], string>> actions;

        private bool shouldrun;

        public int Id { get; }

        public bool Connected { get; private set; }

        public StapClientConnection(ILogger<StapClientConnection> logger, IHardware hardware, TcpClient tcpClient, int id)
        {
            this.logger = logger;
            this.logger.LogInformation("Create new StapClient: {0}", id);
            shouldrun = true;
            this.hardware = hardware;
            this.tcpClient = tcpClient;
            this.Id = id;

            actions = new Dictionary<string, Func<string[], string>>
            {
                { "transmit", Transmit },
                { "transmitex", TransmitEx },
                { "status", GetStatus },
                { "add", Subscribe },
                { "remove", Unsubscribe }
            };

            this.logger.LogDebug("Subscribe to HardwareReceiveEvent");
            hardware.A429DataReceivedEvent += HardwareA429DataReceivedEvent;
            stream = tcpClient.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };
            handleClientThread = new Thread(HandleClientRequest) { IsBackground = true };
            handleClientThread.Start();
        }

        private void HardwareA429DataReceivedEvent(int timetag, int channelId, byte label, uint payloadData)
        {
            try
            {
                writer.WriteLine($"data,{timetag},{channelId},{Convert.ToString(label, 8)},{payloadData:X6}");
                writer.Flush();
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Could not send data to STAP Client {Id}: {ex}");
                shouldrun = false;                
            }
        }

        /// <summary>
        /// Starts the STAP Client Handling.
        /// </summary>
        public void Start()
        {
            logger.LogDebug($"Start Client Connection ({Id})");
        }

        /// <summary>
        /// Stops the STAP Client handling and closes connecton.
        /// </summary>
        public void Stop()
        {
            logger.LogDebug($"Stop Client Connection ({Id})");
            try
            {
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Could not close Stap Client {Id}: {ex.Message}");
            }

            logger.LogDebug($"Stopped STAP client:  ({Id})");
        }

        private void HandleClientRequest()
        {
            var request = string.Empty;

            while (shouldrun)
            {
                try
                {
                    request = reader.ReadLine();

                    if (request == null)
                    {
                        logger.LogWarning($"Client Disconnected ({Id})");
                        shouldrun = false;
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(request))
                    {
                        continue;
                    }

                    var items = request.Split(',');

                    if (!actions.ContainsKey(items[0]))
                    {
                        logger.LogWarning($"Unknown client request ({Id}): {request}");
                        writer.WriteLine("err,unknown request");
                        continue;
                    }
                    
                    writer.WriteLine(actions[items[0]](items));
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Client Disconnected ({Id}): ", ex);
                    shouldrun = false;
                }
            }

            logger.LogDebug($"End with this Client. ({Id})");
            hardware.A429DataReceivedEvent -= HardwareA429DataReceivedEvent;
        }

        private string GetStatus(string[] items)
        {
            if (items.Length != 1)
            {
                return "err,invalid command";
            }
            logger.LogDebug($"GetStatus ({Id})");
            var a429Hardware = string.Empty;
            if (hardware.A429HardwareStatus.Receiver.Count > 0)
            {
                var hwdItems = new List<string>();
                hwdItems.AddRange(hardware.A429HardwareStatus.Receiver.Select(r => $"a429rx{{{r.LineId},{r.Speed}}}"));
                hwdItems.AddRange(hardware.A429HardwareStatus.Transmitter.Select(r => $"a429tx{{{r.LineId},{r.Speed}}}"));
                a429Hardware = string.Join(",", hwdItems);
            }

            var result = $"status,834.1,equipment{{{a429Hardware}}}";
            logger.LogDebug($"A834 Status: {result}");
            return result;
        }

        private string Transmit(string[] items)
        {
            try
            {
                // Check if command is correct
                if (items.Length != 4)
                {
                    return "err,invalid command";
                }
                var channelId = Convert.ToInt32(items[1]);
                var label = Convert.ToByte(items[2], 8);
                var payloadData = (uint)Convert.ToInt32(items[3], 16);                

                // Check if payload only contains 24 bit.
                if (payloadData > 0xffffff)
                {
                    logger.LogWarning($"Invalid data to transmit on channel {channelId} with label: {label} data: {payloadData}");
                    return "err,invalid data";
                }

                if (hardware.TransmitA429Data(channelId, label, payloadData))
                {
                    return "ok";
                }

                logger.LogInformation($"Failed to transmit data. ({Id})");
                return "err,hardware error";
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Invalid format of request: {string.Join(",", items)} ({Id})", ex);
                return "err,invalid format";
            }
        }

        private string TransmitEx(string[] items)
        {
            try
            {
                if (items.Length < 3)
                {
                    return "err,invalid command";
                }
                var channelId = Convert.ToInt32(items[1]);
                var totalWords = Convert.ToInt32(items[2]);
                logger.LogDebug($"Transmitex in Channel {channelId} with Total Words: {totalWords} item count: {items.Length}");

                // Check total count
                if (totalWords *2 != items.Length -3)
                {
                    return "err,total words count error";
                }

                for (int i = 3; i < items.Length; i+=2)
                {
                    Convert.ToByte(items[i], 8);
                    Convert.ToInt32(items[i+1], 16);
                }
                return "ok";
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Invalid format on transmitex: {string.Join(",",items)} ({Id})", ex);
                return "err,invalid format";
            }
        }

        private string Subscribe(string[] items)
        {
            try
            {
                if (items.Length !=3)
                {
                    return "err,invalid command";
                }
                var lineId = Convert.ToInt32(items[1]);
                if (items[2] == "all")
                {
                    logger.LogDebug($"Subscribe all Labels on channel: {lineId}");
                    bool result = false;
                    for (byte label = 0; ; label++)
                    {
                        result = hardware.SubscribeA429Data(lineId, label);
                        if (label == 255) break;                        
                    }
                    if (result)
                    {
                        return "ok";
                    }
                }
                else
                {
                    var label = Convert.ToByte(items[2], 8);
                    if (hardware.SubscribeA429Data(lineId, label))
                    {
                        return "ok";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Subscribe failed on channel: {Id} Error:{ex.Message}");
            }

            return "err";
        }

        private string Unsubscribe(string[] items)
        {
            try
            {
                if (items.Length != 3)
                {
                    return "err,invalid command";
                }
                var lineId = Convert.ToInt32(items[1]);
                if (items[2] == "all")
                {
                    logger.LogDebug($"Unsubscribe all Labels on channel: {lineId}");
                    bool result = false;
                    for (byte label = 0; ; label++)
                    {
                        result = hardware.UnSubscribeA429Data(lineId, label);
                        if (label == 255) break;
                    }
                    if (result)
                    {
                        return "ok";
                    }
                }
                else
                {
                    var label = Convert.ToByte(items[2], 8);
                    if (hardware.UnSubscribeA429Data(lineId, label))
                    {
                        return "ok";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Unsubscribe: {Id}", ex);
            }

            return "err";
        }
    }
}