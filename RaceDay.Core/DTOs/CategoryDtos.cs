namespace RaceDay.Core.DTOs;

public record CreateCategoryDto(string CategoryName, decimal DistanceKm, decimal EntryFee, int? AgeMin, int? AgeMax, string? GenderRestriction, int? Capacity);
public record UpdateCategoryDto(string CategoryName, decimal DistanceKm, decimal EntryFee, int? AgeMin, int? AgeMax, string? GenderRestriction, int? Capacity);
public record CategoryResponseDto(int CategoryId, int EventId, string CategoryName, decimal DistanceKm, decimal EntryFee, int? AgeMin, int? AgeMax, string? GenderRestriction, int? Capacity, int EnrolledCount);