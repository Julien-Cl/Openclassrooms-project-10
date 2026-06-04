using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MicroserviceBackAuth.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace MicroserviceBackAuth.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
  private readonly UserManager<IdentityUser> _userManager;
  private readonly IConfiguration _configuration;

  public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
  {
    _userManager = userManager;
    _configuration = configuration;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterRequest request)
  {
    var user = new IdentityUser
    {
      UserName = request.Email,
      Email = request.Email
    };

    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
      return BadRequest(result.Errors);

    return Ok();
  }

  [HttpPost("login")]
  public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
  {
    var user = await _userManager.FindByEmailAsync(request.Email);

    if (user == null)
      return Unauthorized();

    var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

    if (!passwordValid)
      return Unauthorized();

    var token = GenerateJwtToken(user);

    return new LoginResponse
    {
      Token = token
    };
  }

  private string GenerateJwtToken(IdentityUser user)
  {
    var jwtKey = _configuration["Jwt:Key"];
    var jwtIssuer = _configuration["Jwt:Issuer"];
    var jwtAudience = _configuration["Jwt:Audience"];

    if (string.IsNullOrWhiteSpace(jwtKey))
      throw new InvalidOperationException("Jwt:Key is missing.");

    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier, user.Id),
      new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}