using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TaklaciGuvercin.Api;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.Managers
{
    /// <summary>
    /// Manages flight sessions, position updates, and encounter handling.
    /// </summary>
    public class FlightManager : MonoBehaviour
    {
        private static FlightManager _instance;
        public static FlightManager Instance => _instance;

        // Current flight state
        public FlightSessionDto CurrentFlight { get; private set; }
        public bool IsFlying => CurrentFlight != null && CurrentFlight.IsActive;

        // Position tracking
        [SerializeField] private float positionUpdateInterval = 5f;
        private Coroutine _positionUpdateCoroutine;

        // Simulated position (for testing without GPS)
        private double _currentLatitude = 41.0082; // Istanbul default
        private double _currentLongitude = 28.9784;
        private double _currentAltitude = 100;

        // Events
        public event Action<FlightSessionDto> OnFlightStarted;
        public event Action<string> OnFlightEnded;
        public event Action<FlightPositionUpdate> OnPositionUpdated;
        public event Action<EncounterNotification> OnEncounterDetected;
        public event Action<EncounterResultNotification> OnEncounterResolved;
        public event Action<List<FlightSessionDto>> OnNearbyFlightsUpdated;
        public event Action<string> OnError;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            // Subscribe to SignalR events
            if (SignalRClient.Instance != null)
            {
                SignalRClient.Instance.OnFlightStarted += HandleFlightStarted;
                SignalRClient.Instance.OnFlightEnded += HandleFlightEnded;
                SignalRClient.Instance.OnPositionUpdated += HandlePositionUpdated;
                SignalRClient.Instance.OnEncounterDetected += HandleEncounterDetected;
                SignalRClient.Instance.OnEncounterResult += HandleEncounterResult;
            }
        }

        private void OnDestroy()
        {
            if (SignalRClient.Instance != null)
            {
                SignalRClient.Instance.OnFlightStarted -= HandleFlightStarted;
                SignalRClient.Instance.OnFlightEnded -= HandleFlightEnded;
                SignalRClient.Instance.OnPositionUpdated -= HandlePositionUpdated;
                SignalRClient.Instance.OnEncounterDetected -= HandleEncounterDetected;
                SignalRClient.Instance.OnEncounterResult -= HandleEncounterResult;
            }
        }

        #region Flight Operations

        public void StartFlight(List<string> birdIds, int durationMinutes = 30)
        {
            if (IsFlying)
            {
                OnError?.Invoke("Already in flight");
                return;
            }

            if (birdIds == null || birdIds.Count == 0)
            {
                OnError?.Invoke("No birds selected for flight");
                return;
            }

            Debug.Log($"Starting flight with {birdIds.Count} birds for {durationMinutes} minutes...");

            ApiClient.Instance.StartFlight(birdIds, _currentLatitude, _currentLongitude, durationMinutes,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        CurrentFlight = result.Value;
                        Debug.Log($"Flight started: {CurrentFlight.Id}");

                        // Join active flights group for position updates
                        SignalRClient.Instance?.JoinActiveFlights();

                        // Start position update loop
                        StartPositionUpdates();

                        OnFlightStarted?.Invoke(CurrentFlight);
                        GameManager.Instance.ChangeState(GameState.Flight);
                    }
                    else
                    {
                        OnError?.Invoke(result.Error ?? "Failed to start flight");
                    }
                },
                error => OnError?.Invoke(error));
        }

        public void EndFlight()
        {
            if (!IsFlying)
            {
                OnError?.Invoke("No active flight");
                return;
            }

            string sessionId = CurrentFlight.Id;
            Debug.Log($"Ending flight: {sessionId}...");

            ApiClient.Instance.EndFlight(sessionId,
                result =>
                {
                    if (result.IsSuccess)
                    {
                        StopPositionUpdates();
                        SignalRClient.Instance?.LeaveActiveFlights();

                        CurrentFlight = null;
                        Debug.Log("Flight ended");

                        OnFlightEnded?.Invoke(sessionId);
                        GameManager.Instance.ChangeState(GameState.Coop);

                        // Refresh birds to get updated stamina
                        BirdManager.Instance?.LoadPlayerBirds();
                    }
                    else
                    {
                        OnError?.Invoke(result.Error ?? "Failed to end flight");
                    }
                },
                error => OnError?.Invoke(error));
        }

        public void CheckActiveFlightSession()
        {
            if (!GameManager.Instance.IsLoggedIn) return;

            string playerId = GameManager.Instance.CurrentPlayer.Id;

            ApiClient.Instance.GetActiveFlightSession(playerId,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        CurrentFlight = result.Value;
                        Debug.Log($"Restored active flight: {CurrentFlight.Id}");

                        SignalRClient.Instance?.JoinActiveFlights();
                        StartPositionUpdates();

                        OnFlightStarted?.Invoke(CurrentFlight);
                        GameManager.Instance.ChangeState(GameState.Flight);
                    }
                },
                error => Debug.Log("No active flight session"));
        }

        #endregion

        #region Position Updates

        private void StartPositionUpdates()
        {
            if (_positionUpdateCoroutine != null)
            {
                StopCoroutine(_positionUpdateCoroutine);
            }
            _positionUpdateCoroutine = StartCoroutine(PositionUpdateLoop());
        }

        private void StopPositionUpdates()
        {
            if (_positionUpdateCoroutine != null)
            {
                StopCoroutine(_positionUpdateCoroutine);
                _positionUpdateCoroutine = null;
            }
        }

        private IEnumerator PositionUpdateLoop()
        {
            while (IsFlying)
            {
                yield return new WaitForSeconds(positionUpdateInterval);

                if (IsFlying)
                {
                    UpdatePosition();
                }
            }
        }

        private void UpdatePosition()
        {
            // In a real app, get GPS coordinates here
            // For now, simulate slight movement
            _currentLatitude += UnityEngine.Random.Range(-0.0001f, 0.0001f);
            _currentLongitude += UnityEngine.Random.Range(-0.0001f, 0.0001f);

            ApiClient.Instance.UpdateFlightPosition(CurrentFlight.Id, _currentLatitude, _currentLongitude, _currentAltitude,
                result =>
                {
                    if (!result.IsSuccess)
                    {
                        Debug.LogWarning($"Position update failed: {result.Error}");
                    }
                },
                error => Debug.LogWarning($"Position update error: {error}"));

            // Also send via SignalR for real-time broadcast
            SignalRClient.Instance?.UpdatePosition(CurrentFlight.Id, _currentLatitude, _currentLongitude, _currentAltitude);
        }

        public void SetPosition(double latitude, double longitude, double altitude = 100)
        {
            _currentLatitude = latitude;
            _currentLongitude = longitude;
            _currentAltitude = altitude;
        }

        #endregion

        #region Nearby Flights

        public void GetNearbyFlights(double radiusMeters = 1000)
        {
            if (!IsFlying) return;

            ApiClient.Instance.GetNearbyFlights(CurrentFlight.Id, radiusMeters,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        OnNearbyFlightsUpdated?.Invoke(result.Value);
                    }
                },
                error => Debug.LogWarning($"Failed to get nearby flights: {error}"));
        }

        #endregion

        #region Flight History

        public void GetFlightHistory(int count = 10, Action<List<FlightSessionDto>> callback = null)
        {
            if (!GameManager.Instance.IsLoggedIn) return;

            string playerId = GameManager.Instance.CurrentPlayer.Id;

            ApiClient.Instance.GetFlightHistory(playerId, count,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        callback?.Invoke(result.Value);
                    }
                    else
                    {
                        OnError?.Invoke(result.Error ?? "Failed to get flight history");
                    }
                },
                error => OnError?.Invoke(error));
        }

        #endregion

        #region SignalR Event Handlers

        private void HandleFlightStarted(FlightSessionDto flight)
        {
            Debug.Log($"SignalR: Flight started - {flight.Id}");
            // This is typically our own flight, already handled
        }

        private void HandleFlightEnded(string sessionId)
        {
            Debug.Log($"SignalR: Flight ended - {sessionId}");

            if (CurrentFlight?.Id == sessionId)
            {
                StopPositionUpdates();
                CurrentFlight = null;
                OnFlightEnded?.Invoke(sessionId);
                GameManager.Instance.ChangeState(GameState.Coop);
            }
        }

        private void HandlePositionUpdated(FlightPositionUpdate update)
        {
            // Don't process our own position updates
            if (CurrentFlight?.Id == update.SessionId) return;

            OnPositionUpdated?.Invoke(update);
        }

        private void HandleEncounterDetected(EncounterNotification notification)
        {
            Debug.Log($"SignalR: Encounter detected! Opponent: {notification.OpponentPlayer?.Username}");
            OnEncounterDetected?.Invoke(notification);
            GameManager.Instance.ChangeState(GameState.Encounter);
        }

        private void HandleEncounterResult(EncounterResultNotification result)
        {
            Debug.Log($"SignalR: Encounter result - {(result.YouWon ? "Victory!" : "Defeat")}");
            OnEncounterResolved?.Invoke(result);

            // Refresh player data for coins/XP changes
            GameManager.Instance.RefreshPlayerData();

            // Refresh birds in case any were lost
            BirdManager.Instance?.LoadPlayerBirds();
        }

        #endregion

        #region Encounter Operations

        public void GetActiveEncounters(Action<List<EncounterDto>> callback = null)
        {
            if (!GameManager.Instance.IsLoggedIn) return;

            string playerId = GameManager.Instance.CurrentPlayer.Id;

            ApiClient.Instance.GetActiveEncounters(playerId,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        callback?.Invoke(result.Value);
                    }
                },
                error => OnError?.Invoke(error));
        }

        public void GetEncounterStats(Action<EncounterStatsDto> callback = null)
        {
            if (!GameManager.Instance.IsLoggedIn) return;

            string playerId = GameManager.Instance.CurrentPlayer.Id;

            ApiClient.Instance.GetEncounterStats(playerId,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        callback?.Invoke(result.Value);
                    }
                },
                error => OnError?.Invoke(error));
        }

        #endregion
    }
}
