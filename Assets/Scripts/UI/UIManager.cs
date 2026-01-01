using UnityEngine;
using TaklaciGuvercin.Managers;

namespace TaklaciGuvercin.UI
{
    /// <summary>
    /// Manages UI screen transitions and state.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance => _instance;

        [Header("Screens")]
        [SerializeField] private LoginScreen loginScreen;
        [SerializeField] private CoopScreen coopScreen;
        [SerializeField] private FlightScreen flightScreen;
        [SerializeField] private EncounterScreen encounterScreen;

        [Header("Popups")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private GameObject errorPopup;

        private GameState _currentState = GameState.Login;

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
            // Subscribe to game state changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerLoggedIn += _ => ShowState(GameState.Coop);
                GameManager.Instance.OnPlayerLoggedOut += () => ShowState(GameState.Login);
            }

            // Subscribe to flight events
            if (FlightManager.Instance != null)
            {
                FlightManager.Instance.OnFlightStarted += _ => ShowState(GameState.Flight);
                FlightManager.Instance.OnFlightEnded += _ => ShowState(GameState.Coop);
                FlightManager.Instance.OnEncounterDetected += _ => ShowState(GameState.Encounter);
            }

            // Initial state
            ShowState(GameManager.Instance?.IsLoggedIn == true ? GameState.Coop : GameState.Login);
        }

        public void ShowState(GameState state)
        {
            _currentState = state;

            // Hide all screens
            HideAllScreens();

            // Show appropriate screen
            switch (state)
            {
                case GameState.Login:
                    loginScreen?.gameObject.SetActive(true);
                    break;
                case GameState.Coop:
                case GameState.BirdDetails:
                case GameState.Breeding:
                case GameState.Shop:
                    coopScreen?.gameObject.SetActive(true);
                    break;
                case GameState.Flight:
                    flightScreen?.gameObject.SetActive(true);
                    break;
                case GameState.Encounter:
                    // Show encounter over flight screen
                    flightScreen?.gameObject.SetActive(true);
                    encounterScreen?.gameObject.SetActive(true);
                    break;
            }

            Debug.Log($"UI State changed to: {state}");
        }

        private void HideAllScreens()
        {
            loginScreen?.gameObject.SetActive(false);
            coopScreen?.gameObject.SetActive(false);
            flightScreen?.gameObject.SetActive(false);
            encounterScreen?.gameObject.SetActive(false);
        }

        public void ShowLoading(bool show = true)
        {
            loadingOverlay?.SetActive(show);
        }

        public void HideLoading()
        {
            loadingOverlay?.SetActive(false);
        }

        public void ShowError(string message)
        {
            // Could implement a proper error popup
            Debug.LogError($"UI Error: {message}");
            errorPopup?.SetActive(true);
        }

        public void HideError()
        {
            errorPopup?.SetActive(false);
        }

        public void GoBack()
        {
            switch (_currentState)
            {
                case GameState.BirdDetails:
                case GameState.Breeding:
                case GameState.Shop:
                    ShowState(GameState.Coop);
                    break;
                case GameState.Encounter:
                    if (FlightManager.Instance?.IsFlying == true)
                        ShowState(GameState.Flight);
                    else
                        ShowState(GameState.Coop);
                    break;
            }
        }
    }
}
