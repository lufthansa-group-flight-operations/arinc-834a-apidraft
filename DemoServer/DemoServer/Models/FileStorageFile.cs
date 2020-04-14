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
    [XmlRoot("file")]
    public class FileStorageFile
    {
        [JsonPropertyName("name")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [JsonPropertyName("size")]
        [XmlAttribute("size")]
        public string Size { get; set; }

        [JsonPropertyName("last_change")]
        [XmlAttribute("last_change")]
        public string LastChange { get; set; }
    }
}