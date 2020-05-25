using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParrotWingsApi.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public decimal Balance { get; set; }
        public string Role { get; set; }
    }

    public static class UserRoles
    {
        public const string User = "User";
        public const string Admin = "Admin";
        public const string Superadmin = "Superadmin";
    }
}
