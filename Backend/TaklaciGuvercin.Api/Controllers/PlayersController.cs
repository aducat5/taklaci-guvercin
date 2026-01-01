using Microsoft.AspNetCore.Mvc;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Shared.Common;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public PlayersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<PlayerDto>>> GetPlayer(Guid id)
    {
        var player = await _unitOfWork.Players.GetByIdAsync(id);
        if (player == null)
            return NotFound(Result.Failure<PlayerDto>("Player not found"));

        return Ok(Result.Success(MapToDto(player)));
    }

    [HttpPost("register")]
    public async Task<ActionResult<Result<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        if (await _unitOfWork.Players.EmailExistsAsync(request.Email))
            return BadRequest(Result.Failure<AuthResponse>("Email already in use"));

        if (await _unitOfWork.Players.UsernameExistsAsync(request.Username))
            return BadRequest(Result.Failure<AuthResponse>("Username already taken"));

        // In production, hash the password properly with BCrypt
        var passwordHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(request.Password)));

        var player = Player.Create(request.Username, request.Email, passwordHash);
        await _unitOfWork.Players.AddAsync(player);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.Success(new AuthResponse
        {
            Success = true,
            Token = GenerateToken(player.Id),
            RefreshToken = Guid.NewGuid().ToString(),
            Player = MapToDto(player)
        }));
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var player = await _unitOfWork.Players.GetByEmailAsync(request.Email);
        if (player == null)
            return Unauthorized(Result.Failure<AuthResponse>("Invalid credentials"));

        var passwordHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(request.Password)));

        if (player.PasswordHash != passwordHash)
            return Unauthorized(Result.Failure<AuthResponse>("Invalid credentials"));

        player.SetOnline(true);
        _unitOfWork.Players.Update(player);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.Success(new AuthResponse
        {
            Success = true,
            Token = GenerateToken(player.Id),
            RefreshToken = Guid.NewGuid().ToString(),
            Player = MapToDto(player)
        }));
    }

    [HttpPost("{id}/logout")]
    public async Task<ActionResult<Result>> Logout(Guid id)
    {
        var player = await _unitOfWork.Players.GetByIdAsync(id);
        if (player == null)
            return NotFound(Result.Failure("Player not found"));

        player.SetOnline(false);
        _unitOfWork.Players.Update(player);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.Success());
    }

    [HttpPut("{id}/coins")]
    public async Task<ActionResult<Result<PlayerDto>>> AddCoins(Guid id, [FromBody] int amount)
    {
        var player = await _unitOfWork.Players.GetByIdAsync(id);
        if (player == null)
            return NotFound(Result.Failure<PlayerDto>("Player not found"));

        player.AddCoins(amount);
        _unitOfWork.Players.Update(player);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.Success(MapToDto(player)));
    }

    private static string GenerateToken(Guid playerId)
    {
        // Simplified token for MVP - use proper JWT in production
        return Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{playerId}:{DateTime.UtcNow.AddDays(7):O}"));
    }

    private static PlayerDto MapToDto(Player player) => new()
    {
        Id = player.Id,
        Username = player.Username,
        Coins = player.Coins,
        PremiumCurrency = player.PremiumCurrency,
        Level = player.Level,
        Experience = player.Experience,
        CoopCapacity = player.CoopCapacity,
        Stats = new PlayerStatsDto
        {
            TotalBirdsOwned = player.TotalBirdsOwned,
            TotalEncountersWon = player.TotalEncountersWon,
            TotalEncountersLost = player.TotalEncountersLost,
            TotalBirdsLost = player.TotalBirdsLost,
            TotalBirdsLooted = player.TotalBirdsLooted
        }
    };
}
