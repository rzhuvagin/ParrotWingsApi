using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParrotWingsApi.Models
{
    public class Session
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid UserId { get; set; }

        public string Token { get; set; }
    }
}
