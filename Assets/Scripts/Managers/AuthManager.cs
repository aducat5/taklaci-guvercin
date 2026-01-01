using System;
using UnityEngine;
using TaklaciGuvercin.Api;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.Managers
{
    /// <summary>
    /// Handles player authentication (login, register, logout).
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        private static AuthManager _instance;
        public static AuthManager Instance => _instance;

        // Events
        public event Action<AuthResponse> OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action<AuthResponse> OnRegisterSuccess;
        public event Action<string> OnRegisterFailed;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        public void Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                OnLoginFailed?.Invoke("Email and password are required");
                return;
            }

            Debug.Log($"Attempting login for {email}...");

            ApiClient.Instance.Login(email, password,
                response =>
                {
                    if (response.Success && response.Player != null)
                    {
                        Debug.Log($"Login successful: {response.Player.Username}");

                        // Save token and set current player
                        GameManager.Instance.SaveAuthToken(response.Token);
                        GameManager.Instance.SetCurrentPlayer(response.Player);

                        OnLoginSuccess?.Invoke(response);
                    }
                    else
                    {
                        string error = response.Error ?? "Login failed";
                        Debug.LogWarning($"Login failed: {error}");
                        OnLoginFailed?.Invoke(error);
                    }
                },
                error =>
                {
                    Debug.LogError($"Login error: {error}");
                    OnLoginFailed?.Invoke(error);
                });
        }

        public void Register(string username, string email, string password)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(username))
            {
                OnRegisterFailed?.Invoke("Username is required");
                return;
            }
            if (string.IsNullOrEmpty(email))
            {
                OnRegisterFailed?.Invoke("Email is required");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                OnRegisterFailed?.Invoke("Password is required");
                return;
            }
            if (password.Length < 6)
            {
                OnRegisterFailed?.Invoke("Password must be at least 6 characters");
                return;
            }

            Debug.Log($"Attempting registration for {email}...");

            ApiClient.Instance.Register(username, email, password,
                response =>
                {
                    if (response.Success && response.Player != null)
                    {
                        Debug.Log($"Registration successful: {response.Player.Username}");

                        // Save token and set current player
                        GameManager.Instance.SaveAuthToken(response.Token);
                        GameManager.Instance.SetCurrentPlayer(response.Player);

                        OnRegisterSuccess?.Invoke(response);
                    }
                    else
                    {
                        string error = response.Error ?? "Registration failed";
                        Debug.LogWarning($"Registration failed: {error}");
                        OnRegisterFailed?.Invoke(error);
                    }
                },
                error =>
                {
                    Debug.LogError($"Registration error: {error}");
                    OnRegisterFailed?.Invoke(error);
                });
        }

        public void Logout()
        {
            GameManager.Instance.Logout();
        }

        /// <summary>
        /// Quick login using test player data (for development)
        /// </summary>
        public void DevLogin(int playerIndex = 1)
        {
            string email = $"player{playerIndex}@test.com";
            string password = "hashedpassword123";
            Login(email, password);
        }
    }
}
