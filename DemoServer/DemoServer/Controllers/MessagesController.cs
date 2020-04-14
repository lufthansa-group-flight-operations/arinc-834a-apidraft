//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DemoServer.DataAccess;
using DemoServer.DataAccess.Models;
using DemoServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DemoServer.Controllers
{
    /// <summary>
    /// Controller for handling message requests.
    /// </summary>
    [Route("api/v1/messages")]
    [ApiController]
    [Produces("application/json", "application/xml")]
    public class MessagesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly DatabaseContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesController"/> class.
        /// </summary>
        /// <param name="logger">Logger to use.</param>
        /// <param name="context">Database context to use.</param>
        /// <param name="mapper">Object mapper</param>
        public MessagesController(ILogger<MessagesController> logger, DatabaseContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet(Name = nameof(GetMessages))]
        public MessageList GetMessages()
        {
            var messages = _context.Messages.Select(e => _mapper.Map<Message>(e)).ToList();
            foreach (var msg in messages)
            {
                msg.SelfLink = Url.Link(nameof(GetMessageById), new { id = msg.Id });
            }

            return new MessageList
            {
                SelfLink = Url.Link(nameof(GetMessages), new { }),
                Messages = messages.ToArray()
            };
        }

        [HttpGet("{id}", Name = nameof(GetMessageById))]
        public async Task<IActionResult> GetMessageById([FromRoute] ulong id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var message = await _context.Messages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            var result = _mapper.Map<Message>(message);
            result.SelfLink = Url.Link(nameof(GetMessageById), new { id = result.Id });

            return Ok(result);
        }

        [HttpPut("{id}", Name = nameof(PutMessage))]
        public async Task<IActionResult> PutMessage([FromRoute] ulong id, [FromBody] Message message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != message.Id)
            {
                return BadRequest();
            }

            _context.Entry(_mapper.Map<MessageEntity>(message)).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MessageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost(Name = nameof(PostMessage))]
        public async Task<IActionResult> PostMessage([FromBody] Message message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var messageEntity = _mapper.Map<MessageEntity>(message);
            _context.Messages.Add(messageEntity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessageById), new { id = messageEntity.Id }, message);
        }

        [HttpDelete("{id}", Name = nameof(DeleteMessage))]
        public async Task<IActionResult> DeleteMessage([FromRoute] ulong id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var messageEntity = await _context.Messages.FindAsync(id);
            if (messageEntity == null)
            {
                return NotFound();
            }

            _context.Messages.Remove(messageEntity);
            await _context.SaveChangesAsync();

            var message = _mapper.Map<Message>(messageEntity);

            return Ok(message);
        }

        private bool MessageExists(ulong id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }
    }
}