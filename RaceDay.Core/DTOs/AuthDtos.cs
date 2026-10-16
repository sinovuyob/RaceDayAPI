namespace RaceDay.Core.DTOs;

public record RegisterDto(string Email, string Password, string FullName, string Role, DateTime? DateOfBirth, string? PhoneNumber);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, UserDto User);