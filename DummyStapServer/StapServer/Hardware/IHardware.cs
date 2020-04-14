//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

namespace StapServer.Hardware
{
    public delegate void A429DataReceivedDelegate(int timetag, int channelId, byte label, uint payloadData);

    public interface IHardware
    {
        event A429DataReceivedDelegate A429DataReceivedEvent;

        A429Hardware A429HardwareStatus { get; }

        bool Init();

        bool Start();

        bool Stop();

        bool TransmitA429Data(int channelId, byte label, uint payloadData);

        bool SubscribeA429Data(int channelId, byte label);

        bool UnSubscribeA429Data(int channelId, byte label);
    }
}