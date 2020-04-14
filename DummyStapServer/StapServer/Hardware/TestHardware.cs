//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace StapServer.Hardware
{
    public class TestHardware : IHardware
    {
        private readonly ILogger logger;

        public event A429DataReceivedDelegate A429DataReceivedEvent;

        public A429Hardware A429HardwareStatus => new A429Hardware
        {
            Receiver = new List<A429HardwareReceiver>
            {
                new A429HardwareReceiver { LineId = 0, Speed = A429HardwareSpeed.low },
                new A429HardwareReceiver { LineId = 1, Speed = A429HardwareSpeed.high }
            },
            Transmitter = new List<A429HardwareTransmitter>
            {
                new A429HardwareTransmitter { LineId = 0, Speed = A429HardwareSpeed.high, State = A429TransmitterState.free },
                new A429HardwareTransmitter { LineId = 1, Speed = A429HardwareSpeed.low, State = A429TransmitterState.locked },
                new A429HardwareTransmitter { LineId = 2, Speed = A429HardwareSpeed.high, State = A429TransmitterState.owned }
            }
        };

        public TestHardware(ILogger<TestHardware> logger)
        {
            this.logger = logger;
        }

        public bool Init()
        {
            logger.LogDebug("INIT");
            return true;
        }

        public bool Start()
        {
            logger.LogDebug("START");
            return true;
        }

        public bool Stop()
        {
            logger.LogDebug("STOP");
            return true;
        }

        public bool TransmitA429Data(int channelId, byte label, uint payloadData)
        {
            logger.LogDebug("Transmit: {0} {1} {2}", channelId, Convert.ToString(label, 8), payloadData.ToString("X6"));
            if (A429HardwareStatus.Transmitter.Exists(x => x.LineId == channelId))
            {
                return true;
            }
            else
            {
                logger.LogWarning($"Invalid Transmitter: {channelId}");
                return false;
            }
        }

        public bool SubscribeA429Data(int channelId, byte label)
        {

            logger.LogDebug("Subscribe: Channel:{0} Label: {1}", channelId, Convert.ToString(label, 8));
            if (A429HardwareStatus.Receiver.Exists(x => x.LineId == channelId ))
            {
                return true;
            }
            else
            {
                logger.LogWarning($"Could not Subscribe: Invalid Receiver with lineId: {channelId}");
                return false;
            }
        }

        public bool UnSubscribeA429Data(int channelId, byte label)
        {
            logger.LogDebug("Unsubscribe: Channel:{0} Label: {1}", channelId, Convert.ToString(label, 8));
            if (A429HardwareStatus.Receiver.Exists(x => x.LineId == channelId))
            {
                return true;
            }
            else
            {
                logger.LogWarning($"Could not unsubscribe: Invalid Receiver with lineId: {channelId}");
                return false;
            }            
        }
    }
}