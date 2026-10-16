namespace RaceDay.Core.DTOs;

public record CreateResultDto(TimeSpan? FinishTime, int? OverallPosition, int? CategoryPosition);
public record UpdateResultDto(TimeSpan? FinishTime, int? OverallPosition, int? CategoryPosition, string Status);
public record ResultResponseDto(int ResultId, int EnrolmentId, string ParticipantName, string EventName, string CategoryName, TimeSpan? FinishTime, int? OverallPosition, int? CategoryPosition, string Status);