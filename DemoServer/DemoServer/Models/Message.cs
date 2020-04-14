//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace DemoServer.Models
{
    [Serializable]
    [XmlRoot("message")]
    [DebuggerDisplay("{SelfLink}")]
    public class Message : BaseRestResource
    {
        /// <summary>
        /// Gets or sets the internal id of the resource.
        /// </summary>
        [JsonPropertyName("id")]
        [XmlElement("id")]
        public ulong? Id { get; set; }

        /// <summary>
        /// Gets or sets the message name.
        /// </summary>
        [JsonPropertyName("name")]
        [XmlElement("name")]
        public string Name { get; set; }
    }
}