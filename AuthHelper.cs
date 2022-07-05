using AllocationSystem.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AllocationSystem.WebApi;

public static class AuthConstants
{
    public const string JWT_SECRET_KEY = "x36NvWlph#!%$^BAbbi2jXj$HW$#eTv6g#$NNQ42Q$^%$^";

    public static class Roles
    {
        public const string ADMINS = "Admins";
        public const string USERS = "Users";
        public const string SUPERVISORS = "Supervisors";
    }
}

public static class AuthHelper
{
    public static string GetJwtToken(User user)
    {
        JwtSecurityTokenHandler tokenHandler = new();
        byte[] key = Encoding.ASCII.GetBytes(AuthConstants.JWT_SECRET_KEY);

        string role = user.IsAdmin ? AuthConstants.Roles.ADMINS : AuthConstants.Roles.USERS;

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("UserID", user.ID.ToString())
        };
        claims.Add(new Claim(ClaimTypes.Role, role));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims.ToArray()),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static string CreatePashwordHashSha256(string userPassword)
    {
        var hash = new StringBuilder();
        using (SHA256 mySHA256 = SHA256.Create())
        {
            byte[] crypto = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(userPassword));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }
        return hash.ToString().ToUpper();
    }
}
public static class ClaimsPrincipalExtensions
{
    public static string SubjectId(this ClaimsPrincipal user) { return user?.Claims?.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase))?.Value; }

    public static T GetLoggedInUserId<T>(this ClaimsPrincipal principal)
    {
        if (principal == null)
            throw new ArgumentNullException(nameof(principal));

        var loggedInUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (typeof(T) == typeof(string))
        {
            return (T)Convert.ChangeType(loggedInUserId, typeof(T));
        }
        else if (typeof(T) == typeof(int) || typeof(T) == typeof(long))
        {
            return loggedInUserId != null ? (T)Convert.ChangeType(loggedInUserId, typeof(T)) : (T)Convert.ChangeType(0, typeof(T));
        }
        else
        {
            throw new Exception("Invalid type provided");
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var isUserAuthorized = context.HttpContext.User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.ToUpper() != "ADMIN");

        if (!isUserAuthorized)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
