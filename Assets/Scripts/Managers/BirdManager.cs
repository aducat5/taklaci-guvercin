using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TaklaciGuvercin.Api;
using TaklaciGuvercin.Shared.DTOs;
using TaklaciGuvercin.Shared.Enums;

namespace TaklaciGuvercin.Managers
{
    /// <summary>
    /// Manages bird data, selection, and operations.
    /// </summary>
    public class BirdManager : MonoBehaviour
    {
        private static BirdManager _instance;
        public static BirdManager Instance => _instance;

        // Bird cache
        private List<BirdDto> _playerBirds = new List<BirdDto>();
        public IReadOnlyList<BirdDto> PlayerBirds => _playerBirds;

        // Selected birds for flight
        private List<BirdDto> _selectedBirds = new List<BirdDto>();
        public IReadOnlyList<BirdDto> SelectedBirds => _selectedBirds;

        // Currently viewed bird
        public BirdDto CurrentBird { get; private set; }

        // Configuration
        [SerializeField] private int maxFlightBirds = 5;
        public int MaxFlightBirds => maxFlightBirds;

        // Events
        public event Action<List<BirdDto>> OnBirdsLoaded;
        public event Action<BirdDto> OnBirdUpdated;
        public event Action<BirdDto> OnBirdSelected;
        public event Action<BirdDto> OnBirdDeselected;
        public event Action OnSelectionCleared;
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

        public void LoadPlayerBirds()
        {
            if (!GameManager.Instance.IsLoggedIn)
            {
                OnError?.Invoke("Player not logged in");
                return;
            }

            string playerId = GameManager.Instance.CurrentPlayer.Id;
            Debug.Log($"Loading birds for player {playerId}...");

            ApiClient.Instance.GetPlayerBirds(playerId,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        _playerBirds = result.Value;
                        Debug.Log($"Loaded {_playerBirds.Count} birds");
                        OnBirdsLoaded?.Invoke(_playerBirds);
                    }
                    else
                    {
                        OnError?.Invoke(result.Error ?? "Failed to load birds");
                    }
                },
                error => OnError?.Invoke(error));
        }

        public void RefreshBird(string birdId)
        {
            ApiClient.Instance.GetBird(birdId,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        UpdateBirdInCache(result.Value);
                    }
                },
                error => Debug.LogError($"Failed to refresh bird: {error}"));
        }

        private void UpdateBirdInCache(BirdDto bird)
        {
            int index = _playerBirds.FindIndex(b => b.Id == bird.Id);
            if (index >= 0)
            {
                _playerBirds[index] = bird;
            }

            // Update current bird if it's the same
            if (CurrentBird?.Id == bird.Id)
            {
                CurrentBird = bird;
            }

            // Update in selection
            int selIndex = _selectedBirds.FindIndex(b => b.Id == bird.Id);
            if (selIndex >= 0)
            {
                _selectedBirds[selIndex] = bird;
            }

            OnBirdUpdated?.Invoke(bird);
        }

        #region Bird Selection

        public bool CanSelectBird(BirdDto bird)
        {
            if (_selectedBirds.Count >= maxFlightBirds) return false;
            if (_selectedBirds.Any(b => b.Id == bird.Id)) return false;
            if (bird.State != BirdState.InCoop) return false;
            if (bird.Health <= 0 || bird.Stamina < 20) return false;
            return true;
        }

        public void SelectBird(BirdDto bird)
        {
            if (!CanSelectBird(bird))
            {
                Debug.LogWarning($"Cannot select bird {bird.Name}");
                return;
            }

            _selectedBirds.Add(bird);
            Debug.Log($"Selected bird: {bird.Name} ({_selectedBirds.Count}/{maxFlightBirds})");
            OnBirdSelected?.Invoke(bird);
        }

        public void DeselectBird(BirdDto bird)
        {
            if (_selectedBirds.RemoveAll(b => b.Id == bird.Id) > 0)
            {
                Debug.Log($"Deselected bird: {bird.Name}");
                OnBirdDeselected?.Invoke(bird);
            }
        }

        public void ToggleBirdSelection(BirdDto bird)
        {
            if (_selectedBirds.Any(b => b.Id == bird.Id))
            {
                DeselectBird(bird);
            }
            else
            {
                SelectBird(bird);
            }
        }

        public bool IsBirdSelected(string birdId)
        {
            return _selectedBirds.Any(b => b.Id == birdId);
        }

        public void ClearSelection()
        {
            _selectedBirds.Clear();
            Debug.Log("Cleared bird selection");
            OnSelectionCleared?.Invoke();
        }

        public List<string> GetSelectedBirdIds()
        {
            return _selectedBirds.Select(b => b.Id).ToList();
        }

        #endregion

        #region Bird Operations

        public void SetCurrentBird(BirdDto bird)
        {
            CurrentBird = bird;
        }

        public void SetCurrentBird(string birdId)
        {
            CurrentBird = _playerBirds.FirstOrDefault(b => b.Id == birdId);
        }

        public void RenameBird(string birdId, string newName, Action<BirdDto> onSuccess = null)
        {
            ApiClient.Instance.UpdateBirdName(birdId, newName,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        UpdateBirdInCache(result.Value);
                        onSuccess?.Invoke(result.Value);
                    }
                    else
                    {
                        OnError?.Invoke(result.Error ?? "Failed to rename bird");
                    }
                },
                error => OnError?.Invoke(error));
        }

        public void HealBird(string birdId, Action<BirdDto> onSuccess = null)
        {
            ApiClient.Instance.HealBird(birdId,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        UpdateBirdInCache(result.Value);
                        onSuccess?.Invoke(result.Value);
                    }
                    else
                    {
                        OnError?.Invoke(result.Error ?? "Failed to heal bird");
                    }
                },
                error => OnError?.Invoke(error));
        }

        public void RestBird(string birdId, Action<BirdDto> onSuccess = null)
        {
            ApiClient.Instance.RestBird(birdId,
                result =>
                {
                    if (result.IsSuccess && result.Value != null)
                    {
                        UpdateBirdInCache(result.Value);
                        onSuccess?.Invoke(result.Value);
                    }
                    else
                    {
                        OnError?.Invoke(result.Error ?? "Failed to rest bird");
                    }
                },
                error => OnError?.Invoke(error));
        }

        #endregion

        #region Filtering & Sorting

        public List<BirdDto> GetBirdsByState(BirdState state)
        {
            return _playerBirds.Where(b => b.State == state).ToList();
        }

        public List<BirdDto> GetReadyBirds()
        {
            return _playerBirds.Where(b =>
                b.State == BirdState.InCoop &&
                b.Health > 0 &&
                b.Stamina >= 20).ToList();
        }

        public List<BirdDto> GetBirdsByRarity(BirdRarity rarity)
        {
            return _playerBirds.Where(b => b.Rarity == rarity).ToList();
        }

        public List<BirdDto> GetBirdsByElement(Element element)
        {
            return _playerBirds.Where(b => b.DNA.Element == element).ToList();
        }

        public List<BirdDto> GetSortedByPower(bool descending = true)
        {
            return descending
                ? _playerBirds.OrderByDescending(b => b.Stats.TotalPower).ToList()
                : _playerBirds.OrderBy(b => b.Stats.TotalPower).ToList();
        }

        #endregion
    }
}
