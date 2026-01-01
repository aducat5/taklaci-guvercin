using TaklaciGuvercin.Domain.Common;

namespace TaklaciGuvercin.Domain.Entities;

public class FlightSession : BaseEntity
{
    public Guid PlayerId { get; private set; }
    public List<Guid> BirdIds { get; private set; } = new();

    // Airspace position (for encounter detection)
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public double Altitude { get; private set; }

    // Flight timing
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public TimeSpan Duration { get; private set; }

    // Flight status
    public bool IsActive { get; private set; }
    public int EncountersCount { get; private set; }

    private FlightSession() { }

    public static FlightSession Create(
        Guid playerId,
        List<Guid> birdIds,
        double latitude,
        double longitude,
        TimeSpan duration)
    {
        return new FlightSession
        {
            PlayerId = playerId,
            BirdIds = birdIds,
            Latitude = latitude,
            Longitude = longitude,
            Altitude = 100,
            StartedAt = DateTime.UtcNow,
            Duration = duration,
            IsActive = true,
            EncountersCount = 0
        };
    }

    public void UpdatePosition(double latitude, double longitude, double altitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        Altitude = altitude;
        SetUpdated();
    }

    public void RecordEncounter()
    {
        EncountersCount++;
        SetUpdated();
    }

    public void End()
    {
        IsActive = false;
        EndedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow > StartedAt.Add(Duration);
    }

    public double GetDistanceTo(FlightSession other)
    {
        const double earthRadius = 6371000; // meters

        var lat1 = Latitude * Math.PI / 180;
        var lat2 = other.Latitude * Math.PI / 180;
        var deltaLat = (other.Latitude - Latitude) * Math.PI / 180;
        var deltaLon = (other.Longitude - Longitude) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadius * c;
    }

    public bool IsInEncounterRange(FlightSession other, double rangeMeters = 500)
    {
        if (!IsActive || !other.IsActive) return false;
        if (PlayerId == other.PlayerId) return false;

        return GetDistanceTo(other) <= rangeMeters;
    }
}
