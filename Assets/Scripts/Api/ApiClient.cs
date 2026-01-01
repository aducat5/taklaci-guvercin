using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TaklaciGuvercin.Shared.Common;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.Api
{
    public class ApiClient : MonoBehaviour
    {
        private static ApiClient _instance;
        public static ApiClient Instance => _instance;

        [SerializeField] private string baseUrl = "http://localhost:5000/api";

        private string _authToken;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetAuthToken(string token)
        {
            _authToken = token;
        }

        public void ClearAuthToken()
        {
            _authToken = null;
        }

        #region Auth Endpoints

        public void Login(string email, string password, Action<AuthResponse> onSuccess, Action<string> onError)
        {
            var request = new LoginRequest { Email = email, Password = password };
            StartCoroutine(PostRequest<LoginRequest, AuthResponse>("auth/login", request, onSuccess, onError));
        }

        public void Register(string username, string email, string password, Action<AuthResponse> onSuccess, Action<string> onError)
        {
            var request = new RegisterRequest { Username = username, Email = email, Password = password };
            StartCoroutine(PostRequest<RegisterRequest, AuthResponse>("auth/register", request, onSuccess, onError));
        }

        #endregion

        #region Player Endpoints

        public void GetPlayer(string playerId, Action<Result<PlayerDto>> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequest<Result<PlayerDto>>($"players/{playerId}", onSuccess, onError));
        }

        public void GetPlayerBirds(string playerId, Action<Result<List<BirdDto>>> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequest<Result<List<BirdDto>>>($"birds/player/{playerId}", onSuccess, onError));
        }

        #endregion

        #region Bird Endpoints

        public void GetBird(string birdId, Action<Result<BirdDto>> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequest<Result<BirdDto>>($"birds/{birdId}", onSuccess, onError));
        }

        public void UpdateBirdName(string birdId, string newName, Action<Result<BirdDto>> onSuccess, Action<string> onError)
        {
            StartCoroutine(PutRequest<object, Result<BirdDto>>($"birds/{birdId}/name?name={UnityWebRequest.EscapeURL(newName)}", null, onSuccess, onError));
        }

        public void HealBird(string birdId, Action<Result<BirdDto>> onSuccess, Action<string> onError)
        {
            StartCoroutine(PostRequest<object, Result<BirdDto>>($"birds/{birdId}/heal", null, onSuccess, onError));
        }

        public void RestBird(string birdId, Action<Result<BirdDto>> onSuccess, Action<string> onError)
        {
            StartCoroutine(PostRequest<object, Result<BirdDto>>($"birds/{birdId}/rest", null, onSuccess, onError));
        }

        #endregion

        #region Flight Endpoints

        public void GetActiveFlightSession(string playerId, Action<Result<FlightSessionDto>> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequest<Result<FlightSessionDto>>($"flights/player/{playerId}", onSuccess, onError));
        }

        public void StartFlight(List<string> birdIds, double latitude, double longitude, int durationMinutes,
            Action<Result<FlightSessionDto>> onSuccess, Action<string> onError)
        {
            var request = new StartFlightRequest
            {
                BirdIds = birdIds,
                Latitude = latitude,
                Longitude = longitude,
                DurationMinutes = durationMinutes
            };
            StartCoroutine(PostRequest<StartFlightRequest, Result<FlightSessionDto>>("flights/start", request, onSuccess, onError));
        }

        public void EndFlight(string sessionId, Action<Result<object>> onSuccess, Action<string> onError)
        {
            StartCoroutine(PostRequest<object, Result<object>>($"flights/{sessionId}/end", null, onSuccess, onError));
        }

        public void UpdateFlightPosition(string sessionId, double latitude, double longitude, double altitude,
            Action<Result<object>> onSuccess, Action<string> onError)
        {
            var update = new FlightPositionUpdate
            {
                SessionId = sessionId,
                Latitude = latitude,
                Longitude = longitude,
                Altitude = altitude
            };
            StartCoroutine(PutRequest<FlightPositionUpdate, Result<object>>($"flights/{sessionId}/position", update, onSuccess, onError));
        }

        public void GetActiveFlights(Action<Result<List<FlightSessionDto>>> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequest<Result<List<FlightSessionDto>>>("flights/active", onSuccess, onError));
        }

        public void GetFlightHistory(string playerId, int count, Action<Result<List<FlightSessionDto>>> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequest<Result<List<FlightSessionDto>>>($"flights/player/{playerId}/history?count={count}", onSuccess, onError));
        }

        public void GetNearbyFlights(string sessionId, double radiusMeters, Action<Result<List<FlightSessionDto>>> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequest<Result<List<FlightSessionDto>>>($"flights/{sessionId}/nearby?radiusMeters={radiusMeters}", onSuccess, onError));
        }

        #endregion

        #region Encounter Endpoints

        public void GetEncounter(string encounterId, Action<Result<EncounterDto>> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequest<Result<EncounterDto>>($"encounters/{encounterId}", onSuccess, onError));
        }

        public void GetActiveEncounters(string playerId, Action<Result<List<EncounterDto>>> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequest<Result<List<EncounterDto>>>($"encounters/player/{playerId}/active", onSuccess, onError));
        }

        public void GetEncounterHistory(string playerId, int count, Action<Result<List<EncounterDto>>> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequest<Result<List<EncounterDto>>>($"encounters/player/{playerId}/history?count={count}", onSuccess, onError));
        }

        public void GetEncounterStats(string playerId, Action<Result<EncounterStatsDto>> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequest<Result<EncounterStatsDto>>($"encounters/player/{playerId}/stats", onSuccess, onError));
        }

        public void ResolveEncounter(string encounterId, Action<Result<EncounterDto>> onSuccess, Action<string> onError)
        {
            StartCoroutine(PostRequest<object, Result<EncounterDto>>($"encounters/{encounterId}/resolve", null, onSuccess, onError));
        }

        public void CancelEncounter(string encounterId, Action<Result<EncounterDto>> onSuccess, Action<string> onError)
        {
            StartCoroutine(PostRequest<object, Result<EncounterDto>>($"encounters/{encounterId}/cancel", null, onSuccess, onError));
        }

        public void PreviewEncounter(string session1Id, string session2Id, Action<Result<EncounterPreviewDto>> onSuccess, Action<string> onError)
        {
            var request = new EncounterPreviewRequest { Session1Id = session1Id, Session2Id = session2Id };
            StartCoroutine(PostRequest<EncounterPreviewRequest, Result<EncounterPreviewDto>>("encounters/preview", request, onSuccess, onError));
        }

        #endregion

        #region HTTP Methods

        private IEnumerator GetRequest<TResponse>(string endpoint, Action<TResponse> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/{endpoint}";
            using var request = UnityWebRequest.Get(url);
            SetHeaders(request);

            yield return request.SendWebRequest();

            HandleResponse(request, onSuccess, onError);
        }

        private IEnumerator PostRequest<TRequest, TResponse>(string endpoint, TRequest body, Action<TResponse> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/{endpoint}";
            string jsonBody = body != null ? JsonUtility.ToJson(body) : "{}";

            using var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            SetHeaders(request);

            yield return request.SendWebRequest();

            HandleResponse(request, onSuccess, onError);
        }

        private IEnumerator PutRequest<TRequest, TResponse>(string endpoint, TRequest body, Action<TResponse> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/{endpoint}";
            string jsonBody = body != null ? JsonUtility.ToJson(body) : "{}";

            using var request = new UnityWebRequest(url, "PUT");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            SetHeaders(request);

            yield return request.SendWebRequest();

            HandleResponse(request, onSuccess, onError);
        }

        private IEnumerator DeleteRequest<TResponse>(string endpoint, Action<TResponse> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/{endpoint}";
            using var request = UnityWebRequest.Delete(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            SetHeaders(request);

            yield return request.SendWebRequest();

            HandleResponse(request, onSuccess, onError);
        }

        private void SetHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            if (!string.IsNullOrEmpty(_authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            }
        }

        private void HandleResponse<TResponse>(UnityWebRequest request, Action<TResponse> onSuccess, Action<string> onError)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(response);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse response: {ex.Message}\nResponse: {request.downloadHandler.text}");
                    onError?.Invoke($"Failed to parse response: {ex.Message}");
                }
            }
            else
            {
                string errorMessage = request.error;
                if (!string.IsNullOrEmpty(request.downloadHandler?.text))
                {
                    errorMessage += $": {request.downloadHandler.text}";
                }
                Debug.LogError($"API Error: {errorMessage}");
                onError?.Invoke(errorMessage);
            }
        }

        #endregion
    }
}
