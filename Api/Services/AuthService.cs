using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Proyecto_Grupo_gris.Api.DTOs.Auth;
using Proyecto_Grupo_gris.Api.DTOs.Users;
using Proyecto_Grupo_gris.Api.Services.Interfaces;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, IMapper mapper, IConfiguration configuration)
    {
        _userManager = userManager;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> AuthenticateAsync(AuthRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new InvalidOperationException("Credenciales inválidas.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateToken(user, roles);

        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiresMinutes"] ?? "120")),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Nombre = user.Nombre,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                City = user.City,
                Country = user.Country,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            }
        };
    }

    public async Task<AuthResponseDto> AuthenticateExternalAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateToken(user, roles);

        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiresMinutes"] ?? "120")),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Nombre = user.Nombre,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                City = user.City,
                Country = user.Country,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            }
        };
    }

    public async Task<UserDto> RegisterAsync(CreateUserDto request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
        {
            throw new InvalidOperationException("El correo ya está registrado.");
        }

        var user = _mapper.Map<ApplicationUser>(request);
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var error = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(error);
        }

        await _userManager.AddToRoleAsync(user, "User");

        return _mapper.Map<UserDto>(user);
    }

    private string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        var secret = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret no configurado.");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "EcoRutApi";
        var audience = _configuration["JwtSettings:Audience"] ?? "EcoRutClients";
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpiresMinutes"] ?? "120");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim("Nombre", user.Nombre),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
