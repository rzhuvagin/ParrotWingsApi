using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParrotWingsApi.Models;
using ParrotWingsApi.Services;

namespace ParrotWingsApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IAuthService _authService;

        public UsersController(UserContext context, IAuthService authService)
        {
            _context = context;
            if(!_context.Users.Any())
            {
                _context.Users.Add(new Models.User() { Id = Guid.NewGuid(), Balance = 500.0M, Email = "test@test.ru", Name = "TestUser", PasswordHash = "passwordhash" });
                _context.SaveChanges();
            }
            _authService = authService;
        }

        // GET: Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // GET: User-info
        [Authorize]
        [HttpGet("User-info")]
        public async Task<ActionResult<IEnumerable<User>>> UserInfo()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserList(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: Users/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: Users
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<SessionCreateResponseModel>> PostUser(CreateUserRequestModel model)
        {
            var user = new User()
            {
                Id = Guid.NewGuid(),
                Balance = 500.0M,
                Name = model.Username,
                Email = model.Email,
                PasswordHash = IAuthService.HashPassword(model.Password),
                Role = UserRoles.User,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var jwt = _authService.AuthenticateUser(new SessionCreateRequestModel() {Email = user.Email, Password = model.Password });
            if (string.IsNullOrEmpty(jwt))
                return BadRequest("Invalid username or password.");
            _context.Sessions.Add(new Session() { Token = jwt, CreatedAt = DateTime.UtcNow, UserId = user.Id });
            await _context.SaveChangesAsync();

            return Ok(new SessionCreateResponseModel() { Id_token = jwt });
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
