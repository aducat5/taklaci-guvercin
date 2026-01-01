using UnityEngine;
using TaklaciGuvercin.Api;
using TaklaciGuvercin.Managers;
using TaklaciGuvercin.UI;

namespace TaklaciGuvercin
{
    /// <summary>
    /// Bootstrap script that initializes all game systems.
    /// Attach this to a persistent GameObject in the main scene.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string apiBaseUrl = "http://localhost:5000/api";
        [SerializeField] private string signalRHubUrl = "ws://localhost:5000/hubs/airspace";

        [Header("Manager Prefabs (Optional)")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject apiClientPrefab;
        [SerializeField] private GameObject signalRClientPrefab;

        private void Awake()
        {
            // Only run once
            if (FindObjectsByType<Bootstrap>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            Debug.Log("Initializing Taklaci Guvercin game systems...");

            // Create API Client if not exists
            if (ApiClient.Instance == null)
            {
                if (apiClientPrefab != null)
                {
                    Instantiate(apiClientPrefab);
                }
                else
                {
                    var apiClientObj = new GameObject("ApiClient");
                    apiClientObj.AddComponent<ApiClient>();
                    DontDestroyOnLoad(apiClientObj);
                }
            }

            // Create SignalR Client if not exists
            if (SignalRClient.Instance == null)
            {
                if (signalRClientPrefab != null)
                {
                    Instantiate(signalRClientPrefab);
                }
                else
                {
                    var signalRObj = new GameObject("SignalRClient");
                    signalRObj.AddComponent<SignalRClient>();
                    DontDestroyOnLoad(signalRObj);
                }
            }

            // Create Game Manager if not exists
            if (GameManager.Instance == null)
            {
                if (gameManagerPrefab != null)
                {
                    Instantiate(gameManagerPrefab);
                }
                else
                {
                    var gameManagerObj = new GameObject("GameManager");
                    gameManagerObj.AddComponent<GameManager>();
                    gameManagerObj.AddComponent<AuthManager>();
                    gameManagerObj.AddComponent<BirdManager>();
                    gameManagerObj.AddComponent<FlightManager>();
                    DontDestroyOnLoad(gameManagerObj);
                }
            }

            Debug.Log("Game systems initialized successfully!");
        }

        /// <summary>
        /// Call this from a button or startup to connect to backend
        /// </summary>
        public void ConnectToServer()
        {
            Debug.Log("Connecting to server...");
            SignalRClient.Instance?.Connect();
        }

        /// <summary>
        /// Quick dev login for testing
        /// </summary>
        public void DevLogin(int playerIndex = 1)
        {
            AuthManager.Instance?.DevLogin(playerIndex);
        }

#if UNITY_EDITOR
        [ContextMenu("Dev Login Player 1")]
        private void DevLoginPlayer1() => DevLogin(1);

        [ContextMenu("Dev Login Player 2")]
        private void DevLoginPlayer2() => DevLogin(2);

        [ContextMenu("Dev Login Player 3")]
        private void DevLoginPlayer3() => DevLogin(3);
#endif
    }
}
