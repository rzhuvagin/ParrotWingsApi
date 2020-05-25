using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParrotWingsApi.Models;
using ParrotWingsApi.Services;

namespace ParrotWingsApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {

        private readonly UserContext _context;
        private readonly IAuthService _authService;

        public SessionsController(UserContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // POST: session/create
        [HttpPost("create")]
        public async Task<ActionResult<SessionCreateResponseModel>> Create(SessionCreateRequestModel model)
        {
            var user = _authService.FindUser(model.Email);
            if (user == null)
                return BadRequest("Invalid username or password.");
            user.PasswordHash = IAuthService.HashPassword(model.Password);
            var jwt = _authService.AuthenticateUser(model);
            if (string.IsNullOrEmpty(jwt))
                return BadRequest("Invalid username or password.");
            _context.Sessions.RemoveRange(_context.Sessions.Where(sess => sess.UserId == user.Id));
            _context.Sessions.Add(new Session() { Token = jwt, CreatedAt = DateTime.UtcNow, UserId = user.Id});
            await _context.SaveChangesAsync();

            return Ok(new SessionCreateResponseModel() { Id_token = jwt });
        }
    }
}