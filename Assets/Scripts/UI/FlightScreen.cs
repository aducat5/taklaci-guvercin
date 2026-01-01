using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TaklaciGuvercin.Managers;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.UI
{
    /// <summary>
    /// Flight screen showing active flight info, map, and encounter alerts.
    /// </summary>
    public class FlightScreen : MonoBehaviour
    {
        [Header("Flight Info")]
        [SerializeField] private TMP_Text flightTimerText;
        [SerializeField] private TMP_Text birdCountText;
        [SerializeField] private TMP_Text encounterCountText;
        [SerializeField] private Button endFlightButton;

        [Header("Position")]
        [SerializeField] private TMP_Text latitudeText;
        [SerializeField] private TMP_Text longitudeText;
        [SerializeField] private TMP_Text altitudeText;

        [Header("Birds Panel")]
        [SerializeField] private Transform birdsContainer;
        [SerializeField] private GameObject flightBirdCardPrefab;

        [Header("Nearby Flights")]
        [SerializeField] private TMP_Text nearbyCountText;
        [SerializeField] private Transform nearbyContainer;
        [SerializeField] private GameObject nearbyFlightPrefab;

        [Header("Power Display")]
        [SerializeField] private TMP_Text totalPowerText;

        private float _flightStartTime;
        private float _flightDuration;
        private bool _isActive;

        private void Start()
        {
            endFlightButton?.onClick.AddListener(OnEndFlightClicked);

            // Subscribe to flight events
            if (FlightManager.Instance != null)
            {
                FlightManager.Instance.OnFlightStarted += HandleFlightStarted;
                FlightManager.Instance.OnFlightEnded += HandleFlightEnded;
                FlightManager.Instance.OnPositionUpdated += HandlePositionUpdated;
                FlightManager.Instance.OnNearbyFlightsUpdated += HandleNearbyFlightsUpdated;
                FlightManager.Instance.OnEncounterDetected += HandleEncounterDetected;
            }

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (FlightManager.Instance != null)
            {
                FlightManager.Instance.OnFlightStarted -= HandleFlightStarted;
                FlightManager.Instance.OnFlightEnded -= HandleFlightEnded;
                FlightManager.Instance.OnPositionUpdated -= HandlePositionUpdated;
                FlightManager.Instance.OnNearbyFlightsUpdated -= HandleNearbyFlightsUpdated;
                FlightManager.Instance.OnEncounterDetected -= HandleEncounterDetected;
            }
        }

        private void Update()
        {
            if (_isActive)
            {
                UpdateTimer();
            }
        }

        private void HandleFlightStarted(FlightSessionDto flight)
        {
            gameObject.SetActive(true);
            _isActive = true;

            // Parse start time and calculate duration
            if (System.DateTime.TryParse(flight.StartedAt, out var startTime) &&
                System.DateTime.TryParse(flight.EndsAt, out var endTime))
            {
                _flightStartTime = Time.time;
                _flightDuration = (float)(endTime - startTime).TotalSeconds;
            }
            else
            {
                _flightStartTime = Time.time;
                _flightDuration = 30 * 60; // Default 30 min
            }

            // Update bird info
            if (birdCountText != null)
            {
                birdCountText.text = $"Birds: {flight.Birds.Count}";
            }

            if (encounterCountText != null)
            {
                encounterCountText.text = $"Encounters: {flight.EncountersCount}";
            }

            // Calculate and display total power
            int totalPower = 0;
            foreach (var bird in flight.Birds)
            {
                totalPower += bird.TotalPower;
            }
            if (totalPowerText != null)
            {
                totalPowerText.text = $"Power: {totalPower}";
            }

            // Display bird cards
            DisplayFlightBirds(flight.Birds);

            // Update position display
            UpdatePositionDisplay(flight.Latitude, flight.Longitude, 100);
        }

        private void HandleFlightEnded(string sessionId)
        {
            _isActive = false;
            gameObject.SetActive(false);
        }

        private void UpdateTimer()
        {
            float elapsed = Time.time - _flightStartTime;
            float remaining = _flightDuration - elapsed;

            if (remaining <= 0)
            {
                if (flightTimerText != null)
                {
                    flightTimerText.text = "Ending...";
                }
                return;
            }

            int minutes = Mathf.FloorToInt(remaining / 60);
            int seconds = Mathf.FloorToInt(remaining % 60);

            if (flightTimerText != null)
            {
                flightTimerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        private void UpdatePositionDisplay(double lat, double lon, double alt)
        {
            if (latitudeText != null) latitudeText.text = $"Lat: {lat:F4}";
            if (longitudeText != null) longitudeText.text = $"Lon: {lon:F4}";
            if (altitudeText != null) altitudeText.text = $"Alt: {alt:F0}m";
        }

        private void DisplayFlightBirds(List<BirdSummaryDto> birds)
        {
            if (birdsContainer == null || flightBirdCardPrefab == null) return;

            // Clear existing
            foreach (Transform child in birdsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create bird cards
            foreach (var bird in birds)
            {
                var cardObj = Instantiate(flightBirdCardPrefab, birdsContainer);
                var nameText = cardObj.GetComponentInChildren<TMP_Text>();
                if (nameText != null)
                {
                    nameText.text = $"{bird.Name}\nPwr: {bird.TotalPower}";
                }
            }
        }

        private void HandlePositionUpdated(FlightPositionUpdate update)
        {
            // This is for other players' positions
            // Could update a mini-map here
        }

        private void HandleNearbyFlightsUpdated(List<FlightSessionDto> nearbyFlights)
        {
            if (nearbyCountText != null)
            {
                nearbyCountText.text = $"Nearby: {nearbyFlights.Count}";
            }

            if (nearbyContainer == null || nearbyFlightPrefab == null) return;

            // Clear existing
            foreach (Transform child in nearbyContainer)
            {
                Destroy(child.gameObject);
            }

            // Create nearby flight indicators
            foreach (var flight in nearbyFlights)
            {
                var indicator = Instantiate(nearbyFlightPrefab, nearbyContainer);
                var text = indicator.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    int power = 0;
                    foreach (var bird in flight.Birds)
                    {
                        power += bird.TotalPower;
                    }
                    text.text = $"Player {flight.PlayerId.Substring(0, 8)}...\nPower: {power}";
                }
            }
        }

        private void HandleEncounterDetected(EncounterNotification notification)
        {
            // Encounter screen will handle this
            Debug.Log($"Encounter detected! Opponent: {notification.OpponentPlayer?.Username}");
        }

        private void OnEndFlightClicked()
        {
            FlightManager.Instance?.EndFlight();
        }

        public void RefreshNearbyFlights()
        {
            FlightManager.Instance?.GetNearbyFlights();
        }
    }
}
