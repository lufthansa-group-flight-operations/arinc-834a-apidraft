//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    [Serializable]
    public record struct AvionicParameter
    {
        [JsonPropertyName("name")]
        //[XmlAttribute("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        //[XmlAttribute("value")]
        public object? Value { get; set; }

        [JsonPropertyName("timestamp")]
        //[XmlAttribute("timestamp")]
        public long? Timestamp { get; set; }

        [JsonPropertyName("settable")]
        //[XmlAttribute("settable")]
        public bool? Settable { get; set; }
    }
}