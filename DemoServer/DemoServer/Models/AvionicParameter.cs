//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace DemoServer.Models
{
    [Serializable]
    public class AvionicParameter
    {
        [JsonPropertyName("k")]
        [XmlAttribute("k")]
        public string Key { get; set; }

        [JsonPropertyName("v")]
        [XmlAttribute("v")]
        public string Value { get; set; }

        [JsonPropertyName("t")]
        [XmlAttribute("t")]
        public string Timestamp { get; set; }

        [JsonPropertyName("s")]
        [XmlAttribute("s")]
        public string State { get; set; }
    }
}