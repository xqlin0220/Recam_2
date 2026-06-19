using System.Security.Claims;
using System.Text;
using Remp.Common.Utilities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Remp.Common.Helpers;

public static class JwtTokenHelper
{
    public static string GenerateToken(
        string userId,
        string email,
        IList<string> roles,
        JwtSettings jwtSettings)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),   
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,           
            audience: jwtSettings.Audience,      
            claims: claims,                   
            expires: expires,                   
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static DateTime GetExpiryTime(JwtSettings jwtSettings)
    {
        return DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes);
    }
}