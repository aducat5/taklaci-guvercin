using System;
using UnityEngine;
using TaklaciGuvercin.Api;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.Managers
{
    /// <summary>
    /// Main game manager that coordinates all other managers and holds current game state.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;

        [Header("References")]
        [SerializeField] private ApiClient apiClient;
        [SerializeField] private SignalRClient signalRClient;

        // Current player state
        public PlayerDto CurrentPlayer { get; private set; }
        public bool IsLoggedIn => CurrentPlayer != null;

        // Events
        public event Action<PlayerDto> OnPlayerLoggedIn;
        public event Action OnPlayerLoggedOut;
        public event Action<PlayerDto> OnPlayerUpdated;
        public event Action<int> OnCoinsChanged;
        public event Action<int> OnExperienceChanged;

        // Game state
        public GameState State { get; private set; } = GameState.Login;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure API and SignalR clients exist
            if (apiClient == null)
            {
                apiClient = GetComponentInChildren<ApiClient>();
            }
            if (signalRClient == null)
            {
                signalRClient = GetComponentInChildren<SignalRClient>();
            }
        }

        private void Start()
        {
            // Try to restore session from PlayerPrefs
            TryRestoreSession();
        }

        private void TryRestoreSession()
        {
            string savedPlayerId = PlayerPrefs.GetString("PlayerId", "");
            string savedToken = PlayerPrefs.GetString("AuthToken", "");

            if (!string.IsNullOrEmpty(savedPlayerId) && !string.IsNullOrEmpty(savedToken))
            {
                // Restore token and fetch player data
                ApiClient.Instance.SetAuthToken(savedToken);
                ApiClient.Instance.GetPlayer(savedPlayerId,
                    result =>
                    {
                        if (result.IsSuccess && result.Value != null)
                        {
                            SetCurrentPlayer(result.Value);
                            Debug.Log($"Session restored for {CurrentPlayer.Username}");
                        }
                        else
                        {
                            ClearSavedSession();
                        }
                    },
                    error =>
                    {
                        Debug.LogWarning($"Failed to restore session: {error}");
                        ClearSavedSession();
                    });
            }
        }

        public void SetCurrentPlayer(PlayerDto player)
        {
            CurrentPlayer = player;
            State = GameState.Coop;

            // Save session
            PlayerPrefs.SetString("PlayerId", player.Id);
            PlayerPrefs.Save();

            // Connect to SignalR
            SignalRClient.Instance?.Connect();
            SignalRClient.Instance?.JoinPlayerGroup(player.Id);

            OnPlayerLoggedIn?.Invoke(player);
        }

        public void UpdatePlayerData(PlayerDto player)
        {
            if (CurrentPlayer == null) return;

            int oldCoins = CurrentPlayer.Coins;
            int oldExp = CurrentPlayer.Experience;

            CurrentPlayer = player;

            if (oldCoins != player.Coins)
            {
                OnCoinsChanged?.Invoke(player.Coins);
            }
            if (oldExp != player.Experience)
            {
                OnExperienceChanged?.Invoke(player.Experience);
            }

            OnPlayerUpdated?.Invoke(player);
        }

        public void Logout()
        {
            // Disconnect from SignalR
            if (CurrentPlayer != null)
            {
                SignalRClient.Instance?.LeavePlayerGroup(CurrentPlayer.Id);
            }
            SignalRClient.Instance?.Disconnect();

            // Clear state
            CurrentPlayer = null;
            State = GameState.Login;
            ApiClient.Instance?.ClearAuthToken();
            ClearSavedSession();

            OnPlayerLoggedOut?.Invoke();
        }

        private void ClearSavedSession()
        {
            PlayerPrefs.DeleteKey("PlayerId");
            PlayerPrefs.DeleteKey("AuthToken");
            PlayerPrefs.Save();
        }

        public void SaveAuthToken(string token)
        {
            PlayerPrefs.SetString("AuthToken", token);
            PlayerPrefs.Save();
            ApiClient.Instance?.SetAuthToken(token);
        }

        public void RefreshPlayerData()
        {
            if (CurrentPlayer == null) return;

            ApiClient.Instance.GetPlayer(CurrentPlayer.Id,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        UpdatePlayerData(result.Value);
                    }
                },
                error => Debug.LogError($"Failed to refresh player data: {error}"));
        }

        public void ChangeState(GameState newState)
        {
            State = newState;
            Debug.Log($"Game state changed to: {newState}");
        }
    }

    public enum GameState
    {
        Login,
        Coop,
        BirdDetails,
        Flight,
        Encounter,
        Breeding,
        Shop
    }
}
