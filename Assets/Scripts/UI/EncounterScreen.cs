using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TaklaciGuvercin.Managers;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.UI
{
    /// <summary>
    /// Encounter popup showing battle info and results.
    /// </summary>
    public class EncounterScreen : MonoBehaviour
    {
        [Header("Encounter Info")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private GameObject encounterPanel;
        [SerializeField] private GameObject resultPanel;

        [Header("Player Side")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text playerPowerText;
        [SerializeField] private Transform playerBirdsContainer;
        [SerializeField] private GameObject birdIconPrefab;

        [Header("Opponent Side")]
        [SerializeField] private TMP_Text opponentNameText;
        [SerializeField] private TMP_Text opponentPowerText;
        [SerializeField] private Transform opponentBirdsContainer;

        [Header("Win Chance")]
        [SerializeField] private Slider winChanceSlider;
        [SerializeField] private TMP_Text winChanceText;

        [Header("Result")]
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text coinsChangeText;
        [SerializeField] private TMP_Text expGainedText;
        [SerializeField] private TMP_Text birdsLostText;
        [SerializeField] private TMP_Text birdsGainedText;
        [SerializeField] private Button continueButton;

        [Header("Animations")]
        [SerializeField] private Animator encounterAnimator;

        private EncounterNotification _currentEncounter;
        private float _encounterStartTime;
        private bool _isWaitingForResult;

        private void Start()
        {
            continueButton?.onClick.AddListener(OnContinueClicked);

            // Subscribe to encounter events
            if (FlightManager.Instance != null)
            {
                FlightManager.Instance.OnEncounterDetected += HandleEncounterDetected;
                FlightManager.Instance.OnEncounterResolved += HandleEncounterResult;
            }

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (FlightManager.Instance != null)
            {
                FlightManager.Instance.OnEncounterDetected -= HandleEncounterDetected;
                FlightManager.Instance.OnEncounterResolved -= HandleEncounterResult;
            }
        }

        private void Update()
        {
            if (_isWaitingForResult && _currentEncounter != null)
            {
                UpdateTimer();
            }
        }

        private void HandleEncounterDetected(EncounterNotification notification)
        {
            _currentEncounter = notification;
            _encounterStartTime = Time.time;
            _isWaitingForResult = true;

            gameObject.SetActive(true);
            encounterPanel?.SetActive(true);
            resultPanel?.SetActive(false);

            // Set title
            if (titleText != null)
            {
                titleText.text = "Encounter!";
            }

            // Player info
            var player = GameManager.Instance?.CurrentPlayer;
            if (playerNameText != null)
            {
                playerNameText.text = player?.Username ?? "You";
            }
            if (playerPowerText != null)
            {
                playerPowerText.text = notification.YourTotalPower.ToString();
            }

            // Opponent info
            if (opponentNameText != null)
            {
                opponentNameText.text = notification.OpponentPlayer?.Username ?? "Opponent";
            }
            if (opponentPowerText != null)
            {
                opponentPowerText.text = notification.OpponentTotalPower.ToString();
            }

            // Win chance calculation
            float totalPower = notification.YourTotalPower + notification.OpponentTotalPower;
            float winChance = totalPower > 0 ? notification.YourTotalPower / totalPower : 0.5f;

            if (winChanceSlider != null)
            {
                winChanceSlider.value = winChance;
            }
            if (winChanceText != null)
            {
                winChanceText.text = $"{winChance * 100:F0}%";
            }

            // Display birds
            DisplayBirds(playerBirdsContainer, notification.OpponentBirds); // Using opponent birds for display
            DisplayBirds(opponentBirdsContainer, notification.OpponentBirds);

            // Play animation
            encounterAnimator?.SetTrigger("Start");

            Debug.Log($"Encounter started! Your power: {notification.YourTotalPower}, Opponent: {notification.OpponentTotalPower}");
        }

        private void HandleEncounterResult(EncounterResultNotification result)
        {
            _isWaitingForResult = false;

            encounterPanel?.SetActive(false);
            resultPanel?.SetActive(true);

            // Result text
            if (resultText != null)
            {
                if (result.YouWon)
                {
                    resultText.text = "Victory!";
                    resultText.color = Color.green;
                }
                else
                {
                    resultText.text = "Defeat";
                    resultText.color = Color.red;
                }
            }

            // Coins
            if (coinsChangeText != null)
            {
                string prefix = result.CoinsChange >= 0 ? "+" : "";
                coinsChangeText.text = $"{prefix}{result.CoinsChange} Coins";
                coinsChangeText.color = result.CoinsChange >= 0 ? Color.yellow : Color.red;
            }

            // Experience
            if (expGainedText != null)
            {
                expGainedText.text = $"+{result.ExperienceGained} XP";
            }

            // Birds lost
            if (birdsLostText != null)
            {
                if (result.BirdsLost.Count > 0)
                {
                    birdsLostText.text = $"Lost: {result.BirdsLost.Count} bird(s)";
                    birdsLostText.color = Color.red;
                    birdsLostText.gameObject.SetActive(true);
                }
                else
                {
                    birdsLostText.gameObject.SetActive(false);
                }
            }

            // Birds gained
            if (birdsGainedText != null)
            {
                if (result.BirdsGained.Count > 0)
                {
                    birdsGainedText.text = $"Looted: {result.BirdsGained.Count} bird(s)";
                    birdsGainedText.color = Color.green;
                    birdsGainedText.gameObject.SetActive(true);
                }
                else
                {
                    birdsGainedText.gameObject.SetActive(false);
                }
            }

            // Play result animation
            encounterAnimator?.SetTrigger(result.YouWon ? "Victory" : "Defeat");
        }

        private void UpdateTimer()
        {
            float elapsed = Time.time - _encounterStartTime;
            float remaining = _currentEncounter.TimeToRespondSeconds - elapsed;

            if (remaining <= 0)
            {
                if (timerText != null)
                {
                    timerText.text = "Resolving...";
                }
                return;
            }

            if (timerText != null)
            {
                timerText.text = $"{remaining:F0}s";
            }
        }

        private void DisplayBirds(Transform container, List<BirdSummaryDto> birds)
        {
            if (container == null || birdIconPrefab == null) return;

            // Clear existing
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            // Create bird icons
            foreach (var bird in birds)
            {
                var icon = Instantiate(birdIconPrefab, container);
                var text = icon.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = $"{bird.Name}\n{bird.TotalPower}";
                }

                // Set color based on rarity
                var image = icon.GetComponent<Image>();
                if (image != null)
                {
                    image.color = GetRarityColor(bird.Rarity);
                }
            }
        }

        private Color GetRarityColor(Shared.Enums.BirdRarity rarity)
        {
            return rarity switch
            {
                Shared.Enums.BirdRarity.Common => new Color(0.6f, 0.6f, 0.6f),
                Shared.Enums.BirdRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                Shared.Enums.BirdRarity.Rare => new Color(0.2f, 0.4f, 0.9f),
                Shared.Enums.BirdRarity.Epic => new Color(0.6f, 0.2f, 0.8f),
                Shared.Enums.BirdRarity.Legendary => new Color(0.9f, 0.7f, 0.1f),
                Shared.Enums.BirdRarity.Mythical => new Color(0.9f, 0.2f, 0.2f),
                _ => Color.white
            };
        }

        private void OnContinueClicked()
        {
            _currentEncounter = null;
            gameObject.SetActive(false);

            // Return to flight or coop
            if (FlightManager.Instance?.IsFlying == true)
            {
                GameManager.Instance?.ChangeState(GameState.Flight);
            }
            else
            {
                GameManager.Instance?.ChangeState(GameState.Coop);
            }
        }
    }
}
