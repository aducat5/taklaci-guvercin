using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TaklaciGuvercin.Managers;
using TaklaciGuvercin.Shared.DTOs;
using TaklaciGuvercin.Shared.Enums;

namespace TaklaciGuvercin.UI
{
    /// <summary>
    /// Main coop screen showing player's birds and flight controls.
    /// </summary>
    public class CoopScreen : MonoBehaviour
    {
        [Header("Player Info")]
        [SerializeField] private TMP_Text usernameText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Slider experienceSlider;
        [SerializeField] private Button logoutButton;

        [Header("Bird Grid")]
        [SerializeField] private Transform birdGridContainer;
        [SerializeField] private GameObject birdCardPrefab;

        [Header("Selection Panel")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private TMP_Text selectionCountText;
        [SerializeField] private TMP_Text totalPowerText;
        [SerializeField] private Button clearSelectionButton;
        [SerializeField] private Button startFlightButton;

        [Header("Flight Duration")]
        [SerializeField] private Slider durationSlider;
        [SerializeField] private TMP_Text durationText;

        [Header("Filter Buttons")]
        [SerializeField] private Button filterAllButton;
        [SerializeField] private Button filterReadyButton;
        [SerializeField] private TMP_Dropdown sortDropdown;

        [Header("Stats Panel")]
        [SerializeField] private TMP_Text totalBirdsText;
        [SerializeField] private TMP_Text winsText;
        [SerializeField] private TMP_Text lossesText;

        private List<BirdCardUI> _birdCards = new List<BirdCardUI>();
        private int _selectedDuration = 15;

        private void Start()
        {
            // Setup listeners
            logoutButton?.onClick.AddListener(OnLogoutClicked);
            clearSelectionButton?.onClick.AddListener(OnClearSelectionClicked);
            startFlightButton?.onClick.AddListener(OnStartFlightClicked);
            filterAllButton?.onClick.AddListener(() => FilterBirds(null));
            filterReadyButton?.onClick.AddListener(() => FilterBirds(BirdState.InCoop));

            durationSlider?.onValueChanged.AddListener(OnDurationChanged);
            sortDropdown?.onValueChanged.AddListener(OnSortChanged);

            // Subscribe to events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerLoggedIn += HandlePlayerLoggedIn;
                GameManager.Instance.OnPlayerUpdated += UpdatePlayerInfo;
                GameManager.Instance.OnPlayerLoggedOut += HandlePlayerLoggedOut;
            }

            if (BirdManager.Instance != null)
            {
                BirdManager.Instance.OnBirdsLoaded += HandleBirdsLoaded;
                BirdManager.Instance.OnBirdSelected += HandleBirdSelected;
                BirdManager.Instance.OnBirdDeselected += HandleBirdDeselected;
                BirdManager.Instance.OnSelectionCleared += HandleSelectionCleared;
            }

            // Initialize UI
            UpdateSelectionPanel();

            // Load initial data if already logged in
            if (GameManager.Instance?.IsLoggedIn == true)
            {
                HandlePlayerLoggedIn(GameManager.Instance.CurrentPlayer);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerLoggedIn -= HandlePlayerLoggedIn;
                GameManager.Instance.OnPlayerUpdated -= UpdatePlayerInfo;
                GameManager.Instance.OnPlayerLoggedOut -= HandlePlayerLoggedOut;
            }

            if (BirdManager.Instance != null)
            {
                BirdManager.Instance.OnBirdsLoaded -= HandleBirdsLoaded;
                BirdManager.Instance.OnBirdSelected -= HandleBirdSelected;
                BirdManager.Instance.OnBirdDeselected -= HandleBirdDeselected;
                BirdManager.Instance.OnSelectionCleared -= HandleSelectionCleared;
            }
        }

        private void HandlePlayerLoggedIn(PlayerDto player)
        {
            gameObject.SetActive(true);
            UpdatePlayerInfo(player);
            BirdManager.Instance?.LoadPlayerBirds();
            FlightManager.Instance?.CheckActiveFlightSession();
        }

        private void HandlePlayerLoggedOut()
        {
            gameObject.SetActive(false);
            ClearBirdGrid();
        }

        private void UpdatePlayerInfo(PlayerDto player)
        {
            if (usernameText != null) usernameText.text = player.Username;
            if (coinsText != null) coinsText.text = player.Coins.ToString("N0");
            if (levelText != null) levelText.text = $"Lv. {player.Level}";

            if (experienceSlider != null)
            {
                int expForNext = player.Level * 100;
                experienceSlider.maxValue = expForNext;
                experienceSlider.value = player.Experience % expForNext;
            }

            if (totalBirdsText != null) totalBirdsText.text = player.Stats.TotalBirdsOwned.ToString();
            if (winsText != null) winsText.text = player.Stats.TotalEncountersWon.ToString();
            if (lossesText != null) lossesText.text = player.Stats.TotalEncountersLost.ToString();
        }

        private void HandleBirdsLoaded(List<BirdDto> birds)
        {
            ClearBirdGrid();
            CreateBirdCards(birds);
        }

        private void ClearBirdGrid()
        {
            foreach (var card in _birdCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            _birdCards.Clear();
        }

        private void CreateBirdCards(List<BirdDto> birds)
        {
            if (birdGridContainer == null || birdCardPrefab == null) return;

            foreach (var bird in birds)
            {
                var cardObj = Instantiate(birdCardPrefab, birdGridContainer);
                var card = cardObj.GetComponent<BirdCardUI>();

                if (card != null)
                {
                    card.Setup(bird);
                    card.OnCardClicked += HandleBirdCardClicked;
                    _birdCards.Add(card);
                }
            }
        }

        private void HandleBirdCardClicked(BirdDto bird)
        {
            BirdManager.Instance?.ToggleBirdSelection(bird);
        }

        private void HandleBirdSelected(BirdDto bird)
        {
            UpdateBirdCardSelection(bird.Id, true);
            UpdateSelectionPanel();
        }

        private void HandleBirdDeselected(BirdDto bird)
        {
            UpdateBirdCardSelection(bird.Id, false);
            UpdateSelectionPanel();
        }

        private void HandleSelectionCleared()
        {
            foreach (var card in _birdCards)
            {
                card?.SetSelected(false);
            }
            UpdateSelectionPanel();
        }

        private void UpdateBirdCardSelection(string birdId, bool selected)
        {
            foreach (var card in _birdCards)
            {
                if (card?.BirdId == birdId)
                {
                    card.SetSelected(selected);
                    break;
                }
            }
        }

        private void UpdateSelectionPanel()
        {
            var selectedBirds = BirdManager.Instance?.SelectedBirds;
            int count = selectedBirds?.Count ?? 0;
            int max = BirdManager.Instance?.MaxFlightBirds ?? 5;

            if (selectionCountText != null)
            {
                selectionCountText.text = $"{count}/{max}";
            }

            if (totalPowerText != null)
            {
                int totalPower = 0;
                if (selectedBirds != null)
                {
                    foreach (var bird in selectedBirds)
                    {
                        totalPower += bird.Stats.TotalPower;
                    }
                }
                totalPowerText.text = $"Power: {totalPower}";
            }

            bool hasSelection = count > 0;
            selectionPanel?.SetActive(hasSelection);
            if (startFlightButton != null) startFlightButton.interactable = hasSelection;
        }

        private void FilterBirds(BirdState? state)
        {
            foreach (var card in _birdCards)
            {
                if (card == null) continue;

                if (state == null)
                {
                    card.gameObject.SetActive(true);
                }
                else
                {
                    bool match = card.BirdState == state.Value;
                    card.gameObject.SetActive(match);
                }
            }
        }

        private void OnDurationChanged(float value)
        {
            _selectedDuration = Mathf.RoundToInt(value);
            if (durationText != null)
            {
                durationText.text = $"{_selectedDuration} min";
            }
        }

        private void OnSortChanged(int index)
        {
            // 0: Default, 1: Power (High), 2: Power (Low), 3: Rarity
            // Sorting would require re-ordering the cards
            var birds = BirdManager.Instance?.PlayerBirds;
            if (birds == null) return;

            List<BirdDto> sorted = new List<BirdDto>(birds);

            switch (index)
            {
                case 1: // Power High
                    sorted.Sort((a, b) => b.Stats.TotalPower.CompareTo(a.Stats.TotalPower));
                    break;
                case 2: // Power Low
                    sorted.Sort((a, b) => a.Stats.TotalPower.CompareTo(b.Stats.TotalPower));
                    break;
                case 3: // Rarity
                    sorted.Sort((a, b) => b.Rarity.CompareTo(a.Rarity));
                    break;
            }

            ClearBirdGrid();
            CreateBirdCards(sorted);
        }

        private void OnLogoutClicked()
        {
            AuthManager.Instance?.Logout();
        }

        private void OnClearSelectionClicked()
        {
            BirdManager.Instance?.ClearSelection();
        }

        private void OnStartFlightClicked()
        {
            var birdIds = BirdManager.Instance?.GetSelectedBirdIds();
            if (birdIds == null || birdIds.Count == 0)
            {
                Debug.LogWarning("No birds selected for flight");
                return;
            }

            FlightManager.Instance?.StartFlight(birdIds, _selectedDuration);
            gameObject.SetActive(false);
        }
    }
}
