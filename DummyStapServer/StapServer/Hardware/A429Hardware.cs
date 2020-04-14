//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System.Collections.Generic;

namespace StapServer.Hardware
{
    public class A429Hardware
    {
        public List<A429HardwareTransmitter> Transmitter { get; set; }

        public List<A429HardwareReceiver> Receiver { get; set; }
    }
}
