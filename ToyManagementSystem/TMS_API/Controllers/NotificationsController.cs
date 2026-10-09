using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS_API.DAL;
using TMS_SharedLibrary.Models;

namespace TMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepo _repo;

        public NotificationsController(INotificationRepo repo)
        {
            _repo = repo;
        }

        // GET: api/Notifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> Get()
        {
            var notifications = await _repo.GetAllAsync();
            return Ok(notifications);
        }

        // GET: api/Notifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> Get(int id)
        {
            var notification = await _repo.FindByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            return Ok(notification);
        }

        // GET: api/Notifications/ByRecipient/5
        [HttpGet("ByRecipient/{recipientId}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetByRecipient(int recipientId)
        {
            var notifications = await _repo.GetByRecipientIdAsync(recipientId);
            return Ok(notifications);
        }

        // POST: api/Notifications
        [HttpPost]
        public async Task<ActionResult<Notification>> Post([FromBody] Notification notification)
        {
            if (notification == null)
            {
                return BadRequest("Notification data is required");
            }

            notification.DateSent = DateTime.Now;
            var createdNotification = await _repo.CreateAsync(notification);
            
            return CreatedAtAction(nameof(Get), new { id = createdNotification.NotificationId }, createdNotification);
        }

        // PUT: api/Notifications/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Notification notification)
        {
            if (notification == null || id != notification.NotificationId)
            {
                return BadRequest("Invalid notification data");
            }

            var existingNotification = await _repo.FindByIdAsync(id);
            if (existingNotification == null)
            {
                return NotFound();
            }

            await _repo.UpdateAsync(notification);
            return NoContent();
        }

        // DELETE: api/Notifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _repo.FindByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
