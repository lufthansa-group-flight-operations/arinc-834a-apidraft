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
    [XmlRoot("message_list")]
    [DebuggerDisplay("{SelfLink}")]
    public class MessageList : BaseRestResource
    {
        [JsonPropertyName("messages")]
        [XmlArray("messages")]
        [XmlArrayItem("message")]
        public Message[] Messages { get; set; }
    }
}