using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TaklaciGuvercin.Shared.DTOs;
using TaklaciGuvercin.Shared.Enums;
using TaklaciGuvercin.Managers;

namespace TaklaciGuvercin.UI
{
    /// <summary>
    /// UI component for displaying a bird card in the coop grid.
    /// </summary>
    public class BirdCardUI : MonoBehaviour
    {
        [Header("Main Elements")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image birdImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text powerText;

        [Header("Status Indicators")]
        [SerializeField] private Image stateIcon;
        [SerializeField] private Image elementIcon;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private GameObject selectedIndicator;

        [Header("Bars")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider staminaBar;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color uncommonColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color rareColor = new Color(0.2f, 0.4f, 0.9f);
        [SerializeField] private Color epicColor = new Color(0.6f, 0.2f, 0.8f);
        [SerializeField] private Color legendaryColor = new Color(0.9f, 0.7f, 0.1f);
        [SerializeField] private Color mythicalColor = new Color(0.9f, 0.2f, 0.2f);

        [Header("Element Colors")]
        [SerializeField] private Color fireColor = new Color(0.9f, 0.3f, 0.1f);
        [SerializeField] private Color iceColor = new Color(0.3f, 0.7f, 0.9f);
        [SerializeField] private Color windColor = new Color(0.5f, 0.9f, 0.5f);
        [SerializeField] private Color emeraldColor = new Color(0.1f, 0.8f, 0.3f);

        private BirdDto _bird;
        private bool _isSelected;
        private Button _button;

        public string BirdId => _bird?.Id;
        public BirdState BirdState => _bird?.State ?? BirdState.InCoop;

        public event Action<BirdDto> OnCardClicked;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }
        }

        public void Setup(BirdDto bird)
        {
            _bird = bird;
            UpdateDisplay();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            selectedIndicator?.SetActive(selected);
        }

        private void UpdateDisplay()
        {
            if (_bird == null) return;

            // Name and power
            if (nameText != null) nameText.text = _bird.Name;
            if (powerText != null) powerText.text = _bird.Stats.TotalPower.ToString();

            // Health and stamina bars
            if (healthBar != null)
            {
                healthBar.maxValue = _bird.MaxHealth;
                healthBar.value = _bird.Health;
            }

            if (staminaBar != null)
            {
                staminaBar.maxValue = _bird.MaxStamina;
                staminaBar.value = _bird.Stamina;
            }

            // Rarity border
            if (rarityBorder != null)
            {
                rarityBorder.color = GetRarityColor(_bird.Rarity);
            }

            // Element indicator
            if (elementIcon != null && _bird.DNA.Element != Element.None)
            {
                elementIcon.color = GetElementColor(_bird.DNA.Element);
                elementIcon.gameObject.SetActive(true);
            }
            else if (elementIcon != null)
            {
                elementIcon.gameObject.SetActive(false);
            }

            // State visual
            UpdateStateVisual();

            // Selection state
            bool isCurrentlySelected = BirdManager.Instance?.IsBirdSelected(_bird.Id) ?? false;
            SetSelected(isCurrentlySelected);

            // Button interactability based on state
            if (_button != null)
            {
                bool canSelect = BirdManager.Instance?.CanSelectBird(_bird) ?? false;
                _button.interactable = canSelect || isCurrentlySelected;
            }
        }

        private void UpdateStateVisual()
        {
            if (_bird == null) return;

            // Dim the card if bird is not available
            float alpha = 1f;

            switch (_bird.State)
            {
                case BirdState.InCoop:
                    if (_bird.Health <= 0 || _bird.Stamina < 20)
                        alpha = 0.5f;
                    break;
                case BirdState.Flying:
                    alpha = 0.6f;
                    break;
                case BirdState.Sick:
                case BirdState.Resting:
                    alpha = 0.4f;
                    break;
            }

            if (backgroundImage != null)
            {
                var color = backgroundImage.color;
                color.a = alpha;
                backgroundImage.color = color;
            }
        }

        private Color GetRarityColor(BirdRarity rarity)
        {
            return rarity switch
            {
                BirdRarity.Common => commonColor,
                BirdRarity.Uncommon => uncommonColor,
                BirdRarity.Rare => rareColor,
                BirdRarity.Epic => epicColor,
                BirdRarity.Legendary => legendaryColor,
                BirdRarity.Mythical => mythicalColor,
                _ => commonColor
            };
        }

        private Color GetElementColor(Element element)
        {
            return element switch
            {
                Element.Fire => fireColor,
                Element.Ice => iceColor,
                Element.Wind => windColor,
                Element.Emerald => emeraldColor,
                _ => Color.white
            };
        }

        private void HandleClick()
        {
            if (_bird != null)
            {
                OnCardClicked?.Invoke(_bird);
            }
        }

        public void Refresh()
        {
            UpdateDisplay();
        }
    }
}
