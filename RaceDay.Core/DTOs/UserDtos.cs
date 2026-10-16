namespace RaceDay.Core.DTOs;

public record UserDto(int UserId, string Email, string FullName, string Role, string? PhoneNumber);
public record UpdateUserDto(string FullName, DateTime? DateOfBirth, string? PhoneNumber);
public record UpdatePasswordDto(string CurrentPassword, string NewPassword);