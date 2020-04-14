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
    public class AvionicParameterInfo
    {
        [JsonPropertyName("key")]
        [XmlAttribute("key")]
        public string Key { get; set; }

        [JsonPropertyName("description")]
        [XmlAttribute("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        [XmlAttribute("type")]
        public string Type { get; set; }

        [JsonPropertyName("uom")]
        [XmlAttribute("uom")]
        public string UnitOfMeasurement { get; set; }
    }
}