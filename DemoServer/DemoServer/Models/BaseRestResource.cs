//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace DemoServer.Models
{
    /// <summary>
    /// Base class for all REST resources.
    /// </summary>
    [Serializable]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class BaseRestResource
    {
        /// <summary>
        /// Gets or sets a URL that can be used to access the resource again.
        /// </summary>
        [JsonPropertyName("selfLink")]
        [XmlElement("selfLink")]
        public string SelfLink { get; set; }
    }
}