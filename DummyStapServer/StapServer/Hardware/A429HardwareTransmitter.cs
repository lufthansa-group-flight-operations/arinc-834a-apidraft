//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

namespace StapServer.Hardware
{
    public class A429HardwareTransmitter
    {
        public int LineId { get; set; }

        public A429HardwareSpeed Speed { get; set; }

        public A429TransmitterState State { get; set; }
    }
}
