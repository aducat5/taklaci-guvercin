using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TaklaciGuvercin.Managers;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.UI
{
    /// <summary>
    /// Login and registration screen controller.
    /// </summary>
    public class LoginScreen : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject registerPanel;

        [Header("Login Fields")]
        [SerializeField] private TMP_InputField loginEmailInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button goToRegisterButton;

        [Header("Register Fields")]
        [SerializeField] private TMP_InputField registerUsernameInput;
        [SerializeField] private TMP_InputField registerEmailInput;
        [SerializeField] private TMP_InputField registerPasswordInput;
        [SerializeField] private TMP_InputField registerConfirmPasswordInput;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button goToLoginButton;

        [Header("Feedback")]
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("Dev Mode")]
        [SerializeField] private Button devLoginButton;

        private void Start()
        {
            // Setup button listeners
            loginButton?.onClick.AddListener(OnLoginClicked);
            registerButton?.onClick.AddListener(OnRegisterClicked);
            goToRegisterButton?.onClick.AddListener(ShowRegisterPanel);
            goToLoginButton?.onClick.AddListener(ShowLoginPanel);
            devLoginButton?.onClick.AddListener(OnDevLoginClicked);

            // Subscribe to auth events
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess += HandleLoginSuccess;
                AuthManager.Instance.OnLoginFailed += HandleAuthFailed;
                AuthManager.Instance.OnRegisterSuccess += HandleRegisterSuccess;
                AuthManager.Instance.OnRegisterFailed += HandleAuthFailed;
            }

            // Default to login panel
            ShowLoginPanel();
            HideLoading();
            ClearError();
        }

        private void OnDestroy()
        {
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess -= HandleLoginSuccess;
                AuthManager.Instance.OnLoginFailed -= HandleAuthFailed;
                AuthManager.Instance.OnRegisterSuccess -= HandleRegisterSuccess;
                AuthManager.Instance.OnRegisterFailed -= HandleAuthFailed;
            }
        }

        public void ShowLoginPanel()
        {
            loginPanel?.SetActive(true);
            registerPanel?.SetActive(false);
            ClearError();
        }

        public void ShowRegisterPanel()
        {
            loginPanel?.SetActive(false);
            registerPanel?.SetActive(true);
            ClearError();
        }

        private void OnLoginClicked()
        {
            string email = loginEmailInput?.text?.Trim();
            string password = loginPasswordInput?.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter email and password");
                return;
            }

            ShowLoading();
            ClearError();
            AuthManager.Instance?.Login(email, password);
        }

        private void OnRegisterClicked()
        {
            string username = registerUsernameInput?.text?.Trim();
            string email = registerEmailInput?.text?.Trim();
            string password = registerPasswordInput?.text;
            string confirmPassword = registerConfirmPasswordInput?.text;

            if (string.IsNullOrEmpty(username))
            {
                ShowError("Please enter a username");
                return;
            }
            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter an email");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter a password");
                return;
            }
            if (password != confirmPassword)
            {
                ShowError("Passwords do not match");
                return;
            }

            ShowLoading();
            ClearError();
            AuthManager.Instance?.Register(username, email, password);
        }

        private void OnDevLoginClicked()
        {
            ShowLoading();
            ClearError();
            AuthManager.Instance?.DevLogin(1);
        }

        private void HandleLoginSuccess(AuthResponse response)
        {
            HideLoading();
            Debug.Log($"Login successful: {response.Player?.Username}");
            gameObject.SetActive(false);
        }

        private void HandleRegisterSuccess(AuthResponse response)
        {
            HideLoading();
            Debug.Log($"Registration successful: {response.Player?.Username}");
            gameObject.SetActive(false);
        }

        private void HandleAuthFailed(string error)
        {
            HideLoading();
            ShowError(error);
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
        }

        private void ClearError()
        {
            if (errorText != null)
            {
                errorText.text = "";
                errorText.gameObject.SetActive(false);
            }
        }

        private void ShowLoading()
        {
            loadingIndicator?.SetActive(true);
            SetButtonsInteractable(false);
        }

        private void HideLoading()
        {
            loadingIndicator?.SetActive(false);
            SetButtonsInteractable(true);
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (loginButton != null) loginButton.interactable = interactable;
            if (registerButton != null) registerButton.interactable = interactable;
            if (devLoginButton != null) devLoginButton.interactable = interactable;
        }
    }
}
