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
    [XmlRoot("parameter_list")]
    public class AvionicParameters
    {
        [JsonPropertyName("parameters")]
        [XmlArray("parameters")]
        [XmlArrayItem("parameter")]
        public AvionicParameter[] Parameters { get; set; }
    }
}