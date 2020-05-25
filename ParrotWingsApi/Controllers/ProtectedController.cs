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
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ProtectedController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IAuthService _authService;
        private readonly IPaymentService _paymentService;

        public ProtectedController(UserContext context, IAuthService authService, IPaymentService paymentService)
        {
            _context = context;
            _authService = authService;
            _paymentService = paymentService;
        }

        // GET: api/Protected/user-info
        [HttpGet("user-info")]
        public ActionResult<UserInfoResponseModel> GetUserInfo()
        {
            string email = HttpContext.User.Identity.Name;
            var user = _authService.FindUser(email);
            if (user == null)
                return Unauthorized();
            return new UserInfoResponseModel() { User_info_token = new UserInfoToken() { Id = user.Id, Email = user.Email, Name = user.Name, Balance = user.Balance } };
        }

        // POST: api/Protected/users/list
        [HttpPost("users/list")]
        public async Task<ActionResult<IEnumerable<FilterResponseModel>>> GetFilteredUsers(FilterRequestModel model)
        {
            var userList = await _context.Users.Where(u => u.Name.StartsWith(model.Filter)).ToListAsync();
            return userList.Select(u => new FilterResponseModel() { Id = u.Id, Name = u.Name }).ToArray();
        }

        // GET: api/Protected/transactions
        [HttpGet("transactions")]
        public async Task<ActionResult<List<CreateTransactionResponseModel>>> GetTransactions()
        {
            string email = HttpContext.User.Identity.Name;
            var user = _authService.FindUser(email);

            var transactionList = await _paymentService.GetTransactionListAsync(user);
            var transactionTokenList = transactionList.Select(tr => new TransactionToken
            {
                Id = tr.Id,
                Date = tr.CreatedAt.ToString(),
                Amount = user.Id == tr.Sender ? -tr.Amount : tr.Amount,
                Balance = user.Id == tr.Sender ? tr.BalanceSender : tr.BalanceRecipient,
                Username = _context.Users.FirstOrDefault(u => u.Id == tr.Recipient).Name
            });
            return Ok(new CreateTransactionResponseModel() { Trans_token = transactionTokenList.ToArray() });
        }

        // POST: api/Protected/transactions
        [HttpPost("transactions")]
        public async Task<ActionResult<CreateTransactionResponseModel>> CreateTransaction(CreateTransactionRequestModel model)
        {
            if (model.Amount <= 0)
                return BadRequest("Invalid amount.");
            string email = HttpContext.User.Identity.Name;
            var sender = _authService.FindUser(email);
            var recipient = _authService.FindUserByName(model.Name);
            if (recipient == null)
                return BadRequest("Invalid username of recipient.");
            if (model.Amount > sender.Balance)
                return BadRequest("Amount greater than your balance.");
            Transaction transaction = await _paymentService.CreateTransactionAsync(sender, recipient, model.Amount);
            if (transaction == null)
                return BadRequest("Something went wroong.");

            return new CreateTransactionResponseModel()
            {
                Trans_token = new TransactionToken[]
                {
                    new TransactionToken()
                    {
                        Id = transaction.Id,
                        Date = transaction.CreatedAt.ToString(),
                        Amount = transaction.Amount,
                        Balance = transaction.BalanceRecipient,
                        Username = model.Name
                    }
                }
            };
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
