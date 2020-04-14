//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoServer.DataAccess.Models
{
    /// <summary>
    /// Model class which represents a message.
    /// </summary>
    [Table("messages")]
    public class MessageEntity
    {
        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        [Key]
        [Column("id")]
        public ulong Id { get; set; }

        /// <summary>
        /// Gets or sets the message name.
        /// </summary>
        [Column("name")]
        [Required]
        public string Name { get; set; }
    }
}