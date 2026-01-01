using TaklaciGuvercin.Domain.Common;

namespace TaklaciGuvercin.Domain.Entities;

public class Player : BaseEntity
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    // In-game currency
    public int Coins { get; private set; }
    public int PremiumCurrency { get; private set; }

    // Stats
    public int TotalBirdsOwned { get; private set; }
    public int TotalEncountersWon { get; private set; }
    public int TotalEncountersLost { get; private set; }
    public int TotalBirdsLost { get; private set; }
    public int TotalBirdsLooted { get; private set; }

    // Progression
    public int Level { get; private set; }
    public int Experience { get; private set; }
    public int CoopCapacity { get; private set; }

    public DateTime LastLoginAt { get; private set; }
    public bool IsOnline { get; private set; }

    private Player() { }

    public static Player Create(string username, string email, string passwordHash)
    {
        return new Player
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Coins = 1000,
            PremiumCurrency = 0,
            Level = 1,
            Experience = 0,
            CoopCapacity = 10,
            LastLoginAt = DateTime.UtcNow,
            IsOnline = false
        };
    }

    public void AddCoins(int amount)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        Coins += amount;
        SetUpdated();
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        if (Coins < amount) return false;

        Coins -= amount;
        SetUpdated();
        return true;
    }

    public void AddExperience(int xp)
    {
        Experience += xp;
        CheckLevelUp();
        SetUpdated();
    }

    private void CheckLevelUp()
    {
        int xpRequired = Level * 100;
        while (Experience >= xpRequired)
        {
            Experience -= xpRequired;
            Level++;
            CoopCapacity += 2;
            xpRequired = Level * 100;
        }
    }

    public void RecordEncounterWin()
    {
        TotalEncountersWon++;
        SetUpdated();
    }

    public void RecordEncounterLoss()
    {
        TotalEncountersLost++;
        SetUpdated();
    }

    public void RecordBirdLost()
    {
        TotalBirdsLost++;
        SetUpdated();
    }

    public void RecordBirdLooted()
    {
        TotalBirdsLooted++;
        SetUpdated();
    }

    public void SetOnline(bool online)
    {
        IsOnline = online;
        if (online) LastLoginAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        SetUpdated();
    }
}
