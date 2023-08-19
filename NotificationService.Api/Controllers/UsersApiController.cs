using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Api.Data;
using NotificationService.Api.Notifications;

namespace NotificationService.Api.Controllers
{
    [ApiController]
    [Route("users/[action]")]
    public class UsersApiController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersApiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] string userName)
        {
            var accountId = Guid.NewGuid();
            var newUser = new User(Guid.NewGuid(), userName);

            // BUG: This gets executed twice
            await _mediator.Publish(new NewUserNotification(newUser, accountId));

            // These all get executed once as expected.
            await _mediator.Publish(new UserAuditLogNotification("New User Created", newUser.Id));
            await _mediator.Publish(new AccountAuditLogNotification("User Joined Account", accountId));
            await _mediator.Publish(new AuditLogNotification("User Created", true));

            return Accepted();
        }
    }
}