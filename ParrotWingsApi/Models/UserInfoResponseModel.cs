using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParrotWingsApi.Models
{
    public class UserInfoResponseModel
    {
        public UserInfoToken User_info_token { get; set; }
    }

    public class UserInfoToken
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public decimal Balance { get; set; }
    }
}
