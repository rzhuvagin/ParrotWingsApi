using Microsoft.AspNetCore.Http;
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
    public interface IAuthService
    {
        public string AuthenticateUser(SessionCreateRequestModel user);

        public User FindUser(string email);
        public User FindUserByName(string name);

        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return buffer3.SequenceEqual(buffer4);
        }
    }

    public class AuthService : IAuthService
    {
        private UserContext _userContext;
        public AuthService(UserContext userContext)
        {
            _userContext = userContext;


            if (!_userContext.Users.Any())
            {
                _userContext.Users.Add(new Models.User() { Id = Guid.NewGuid(), Balance = 500.0M, Email = "test@test.ru", Name = "TestUser", PasswordHash = IAuthService.HashPassword("passwordhash"), Role = UserRoles.User });
                _userContext.Users.Add(new Models.User() { Id = Guid.NewGuid(), Balance = 99999999.0M, Email = "Super@dmin", Name = "-=[SuperAdmin]=-", PasswordHash = IAuthService.HashPassword("adminpass"), Role = UserRoles.Superadmin });
                _userContext.SaveChanges();
            }
        }

        public string AuthenticateUser(SessionCreateRequestModel model)
        {
            var identity = GetIdentity(model.Email, model.Password); 
            if (identity == null)
            {
                return null;
            }
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        public User FindUser(string email)
        {
            if (String.IsNullOrWhiteSpace(email))
                return null;
            var user = _userContext.Users.Where(user => user.Email == email).FirstOrDefault();
            return user;
        }

        public User FindUserByName(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return null;
            var user = _userContext.Users.FirstOrDefault(user => user.Name == name);
            return user;
        }

        private ClaimsIdentity GetIdentity(string email, string password)
        {
            User person = _userContext.Users.FirstOrDefault(x => x.Email == email);
            if (person != null)
            {
                if (!IAuthService.VerifyHashedPassword(person.PasswordHash, password))
                    return null;
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, person.Email),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role)
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            // если пользователя не найдено
            return null;
        }
    }
}
