namespace RaceDay.Core.DTOs;

public record CreateEventDto(string EventName, DateTime EventDate, string Location, string? Description, string EventType, int? MaxParticipants, DateTime? RegistrationDeadline);
public record UpdateEventDto(string EventName, DateTime EventDate, string Location, string? Description, string EventType, int? MaxParticipants, DateTime? RegistrationDeadline, string EventStatus);
public record EventResponseDto(int EventId, string EventName, DateTime EventDate, string Location, string? Description, string EventType, string EventStatus, int? MaxParticipants, DateTime? RegistrationDeadline, string OrganiserName, int CategoryCount);