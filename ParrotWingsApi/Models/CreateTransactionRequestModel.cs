using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParrotWingsApi.Models
{
    public class CreateTransactionRequestModel
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
}
