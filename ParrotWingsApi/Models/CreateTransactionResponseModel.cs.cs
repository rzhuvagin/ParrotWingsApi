using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParrotWingsApi.Models
{
    public class CreateTransactionResponseModel
    {
        // trans_token:{id, date, username, amount, balance}}
        public TransactionToken[] Trans_token { get; set; }
    }

    public class TransactionToken
    {
        public Guid Id { get; set; }
        public string Date { get; set; }
        public string Username { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
    }
}
