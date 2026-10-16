namespace RaceDay.Core.DTOs;

public record CreateEnrolmentDto(int CategoryId);
public record EnrolmentResponseDto(int EnrolmentId, int ParticipantId, string ParticipantName, int EventId, string EventName, int CategoryId, string CategoryName, DateTime EntryDate, string Status, int? BibNumber, string PaymentStatus);