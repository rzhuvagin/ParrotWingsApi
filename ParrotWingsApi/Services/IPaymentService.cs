using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ParrotWingsApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TokenApp;

namespace ParrotWingsApi.Services
{
    public interface IPaymentService
    {
        public User FindUser(Guid id);
        public Task<List<Transaction>> GetTransactionListAsync(User user);
        public Task<Transaction> CreateTransactionAsync(User sender, User recipient, decimal amount);
    }

    public class PaymentService : IPaymentService
    {
        private UserContext _userContext;
        private TransactionContext _transactionContext;
        public PaymentService(UserContext userContext, TransactionContext transactionContext)
        {
            _userContext = userContext;
            _transactionContext = transactionContext;

            if (!_transactionContext.Transactions.Any())
            {
                _transactionContext.Transactions.Add(new Transaction() { Id = Guid.NewGuid(), Sender = Guid.NewGuid(), Recipient = Guid.NewGuid(), Amount = 666M, BalanceRecipient = 1922M });
                _transactionContext.SaveChanges();
            }
        }

        public User FindUser(Guid id)
        {
            var user = _userContext.Users.Find(id);
            return user;
        }

        public async Task<List<Transaction>> GetTransactionListAsync(User user)
        {
            var list = await _transactionContext.Transactions.Where(tr => tr.Sender == user.Id || tr.Recipient == user.Id).ToListAsync();
            return list;
        }

        public async Task<Transaction> CreateTransactionAsync(User sender, User recipient, decimal amount)
        {
            var transaction = new Transaction() { Id = Guid.NewGuid(), Sender = sender.Id, Recipient = recipient.Id, Amount = amount, BalanceSender = sender.Balance, BalanceRecipient = recipient.Balance, CreatedAt = DateTime.Now };
            await _transactionContext.AddAsync(transaction);
            sender.Balance -= amount;
            _userContext.Users.Update(sender);
            recipient.Balance += amount;
            _userContext.Users.Update(recipient);
            await _transactionContext.SaveChangesAsync();
            await _userContext.SaveChangesAsync();
            return transaction;
        }
    }
}
