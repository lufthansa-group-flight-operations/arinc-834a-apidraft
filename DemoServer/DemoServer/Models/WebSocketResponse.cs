//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public class WebSocketResponse
    {
        [JsonPropertyName("returnCode")]
        public string ReturnCode { get; set; }


        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("unknownParameters")]
        public string[] UnKnownParameters { get; set; }
    }

    public enum WebSocketResponseCode
    {
        Ok,
        Error
    }

    public enum WebSocketSubscriptionErrorCode
    {
        TimeOut = 1002,
        BadFormat = 1007,
        BadRequest = 1008,
        TooLarge = 1009,
        InvalidPeriod = 4010,
        AllunknownParameters = 4011
    }
}
