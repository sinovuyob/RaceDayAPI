using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Infrastructure.Data;
using RaceDay.Infrastructure.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthController(RaceDayDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "Email already exists" });

        if (dto.Role != "Organiser" && dto.Role != "Participant")
            return BadRequest(new { message = "Role must be Organiser or Participant" });

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            Role = dto.Role,
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return StatusCode(201, new AuthResponseDto(token, new UserDto(user.UserId, user.Email, user.FullName, user.Role, user.PhoneNumber)));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials" });

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponseDto(token, new UserDto(user.UserId, user.Email, user.FullName, user.Role, user.PhoneNumber)));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new UserDto(user.UserId, user.Email, user.FullName, user.Role, user.PhoneNumber));
    }
}