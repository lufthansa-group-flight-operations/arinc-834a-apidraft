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
    //[XmlRoot("parameter_list")]
    public class AvionicParameters
    {
        [JsonPropertyName("parameters")]
        public AvionicParameter[] Parameters { get; set; }

        [JsonPropertyName("unknownParameters")]
        public string[]? UnknownParameters { get; set; }
    }
}