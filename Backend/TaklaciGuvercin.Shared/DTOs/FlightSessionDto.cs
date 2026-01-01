namespace TaklaciGuvercin.Shared.DTOs;

public class FlightSessionDto
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public List<BirdSummaryDto> Birds { get; set; } = new();
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsActive { get; set; }
    public int EncountersCount { get; set; }
}

public class StartFlightRequest
{
    public List<Guid> BirdIds { get; set; } = new();
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int DurationMinutes { get; set; }
}

public class FlightPositionUpdate
{
    public Guid SessionId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
}
