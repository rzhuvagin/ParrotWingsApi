using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace ParrotWingsApi.Models
{
    public class CreateUserRequestModel
    {
        [NotNull]
        public string Username { get; set; }
        [NotNull]
        public string Password { get; set; }
        [NotNull]
        public string Email { get; set; }
    }
}
