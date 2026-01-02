#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace TaklaciGuvercin.Editor
{
    /// <summary>
    /// Editor script to set up the complete game scene with UI.
    /// Run from menu: Taklaci Guvercin > Setup Game Scene
    /// </summary>
    public class GameSceneSetup : EditorWindow
    {
        [MenuItem("Taklaci Guvercin/Setup Game Scene")]
        public static void SetupScene()
        {
            if (!EditorUtility.DisplayDialog("Setup Game Scene",
                "This will create all game managers and UI in the current scene. Continue?",
                "Yes", "Cancel"))
            {
                return;
            }

            CreateGameManagers();
            CreateEventSystem();
            CreateMainCanvas();
            CreatePrefabs();

            Debug.Log("Game scene setup complete! Don't forget to save the scene.");
            EditorUtility.DisplayDialog("Setup Complete",
                "Game scene has been set up successfully!\n\n" +
                "Next steps:\n" +
                "1. Save the scene (Ctrl+S)\n" +
                "2. Run the backend server\n" +
                "3. Press Play to test",
                "OK");
        }

        private static void CreateGameManagers()
        {
            // Check if already exists
            if (Object.FindFirstObjectByType<Bootstrap>() != null)
            {
                Debug.Log("Game managers already exist, skipping...");
                return;
            }

            // Create Bootstrap (parent for all managers)
            var bootstrapGO = new GameObject("GameBootstrap");
            bootstrapGO.AddComponent<Bootstrap>();

            // Create API Client
            var apiClientGO = new GameObject("ApiClient");
            apiClientGO.transform.SetParent(bootstrapGO.transform);
            apiClientGO.AddComponent<Api.ApiClient>();

            // Create SignalR Client
            var signalRGO = new GameObject("SignalRClient");
            signalRGO.transform.SetParent(bootstrapGO.transform);
            signalRGO.AddComponent<Api.SignalRClient>();

            // Create Game Manager with all managers
            var gameManagerGO = new GameObject("GameManager");
            gameManagerGO.transform.SetParent(bootstrapGO.transform);
            gameManagerGO.AddComponent<Managers.GameManager>();
            gameManagerGO.AddComponent<Managers.AuthManager>();
            gameManagerGO.AddComponent<Managers.BirdManager>();
            gameManagerGO.AddComponent<Managers.FlightManager>();

            Debug.Log("Created game managers");
        }

        private static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        private static void CreateMainCanvas()
        {
            // Create main canvas
            var canvasGO = new GameObject("MainCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Create UI Manager
            var uiManagerGO = new GameObject("UIManager");
            uiManagerGO.transform.SetParent(canvasGO.transform);
            var uiManager = uiManagerGO.AddComponent<UI.UIManager>();

            // Create screens
            var loginScreen = CreateLoginScreen(canvasGO.transform);
            var coopScreen = CreateCoopScreen(canvasGO.transform);
            var flightScreen = CreateFlightScreen(canvasGO.transform);
            var encounterScreen = CreateEncounterScreen(canvasGO.transform);

            // Wire up UIManager references via SerializedObject
            var so = new SerializedObject(uiManager);
            so.FindProperty("loginScreen").objectReferenceValue = loginScreen;
            so.FindProperty("coopScreen").objectReferenceValue = coopScreen;
            so.FindProperty("flightScreen").objectReferenceValue = flightScreen;
            so.FindProperty("encounterScreen").objectReferenceValue = encounterScreen;
            so.ApplyModifiedProperties();

            Debug.Log("Created main canvas with all screens");
        }

        private static UI.LoginScreen CreateLoginScreen(Transform parent)
        {
            var screenGO = new GameObject("LoginScreen");
            screenGO.transform.SetParent(parent);

            // Add Image first (creates RectTransform), then stretch
            var bg = screenGO.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f);
            SetFullStretch(screenGO);

            var screen = screenGO.AddComponent<UI.LoginScreen>();

            // Title
            var titleGO = CreateText(screenGO.transform, "Title", "Taklaci Guvercin", 48);
            SetAnchors(titleGO, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f));
            titleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 80);

            // Login Panel
            var loginPanel = CreatePanel(screenGO.transform, "LoginPanel", new Color(0.15f, 0.15f, 0.2f));
            SetAnchors(loginPanel, new Vector2(0.3f, 0.3f), new Vector2(0.7f, 0.7f));

            var loginTitle = CreateText(loginPanel.transform, "LoginTitle", "Login", 32);
            SetAnchors(loginTitle, new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f));

            var emailInput = CreateInputField(loginPanel.transform, "EmailInput", "Email");
            SetAnchors(emailInput, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.7f));

            var passwordInput = CreateInputField(loginPanel.transform, "PasswordInput", "Password", true);
            SetAnchors(passwordInput, new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.55f));

            var loginBtn = CreateButton(loginPanel.transform, "LoginButton", "Login", new Color(0.2f, 0.6f, 0.3f));
            SetAnchors(loginBtn, new Vector2(0.1f, 0.25f), new Vector2(0.45f, 0.35f));

            var registerBtn = CreateButton(loginPanel.transform, "GoToRegisterButton", "Register", new Color(0.3f, 0.3f, 0.5f));
            SetAnchors(registerBtn, new Vector2(0.55f, 0.25f), new Vector2(0.9f, 0.35f));

            var devLoginBtn = CreateButton(loginPanel.transform, "DevLoginButton", "Dev Login", new Color(0.5f, 0.3f, 0.2f));
            SetAnchors(devLoginBtn, new Vector2(0.3f, 0.1f), new Vector2(0.7f, 0.18f));

            var errorText = CreateText(loginPanel.transform, "ErrorText", "", 16);
            SetAnchors(errorText, new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f));
            errorText.GetComponent<TMP_Text>().color = Color.red;

            // Register Panel (hidden by default)
            var registerPanel = CreatePanel(screenGO.transform, "RegisterPanel", new Color(0.15f, 0.15f, 0.2f));
            SetAnchors(registerPanel, new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.8f));
            registerPanel.SetActive(false);

            var regTitle = CreateText(registerPanel.transform, "RegisterTitle", "Register", 32);
            SetAnchors(regTitle, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f));

            var usernameInput = CreateInputField(registerPanel.transform, "UsernameInput", "Username");
            SetAnchors(usernameInput, new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.78f));

            var regEmailInput = CreateInputField(registerPanel.transform, "RegEmailInput", "Email");
            SetAnchors(regEmailInput, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.63f));

            var regPasswordInput = CreateInputField(registerPanel.transform, "RegPasswordInput", "Password", true);
            SetAnchors(regPasswordInput, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.48f));

            var confirmPasswordInput = CreateInputField(registerPanel.transform, "ConfirmPasswordInput", "Confirm Password", true);
            SetAnchors(confirmPasswordInput, new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.33f));

            var regSubmitBtn = CreateButton(registerPanel.transform, "RegisterSubmitButton", "Create Account", new Color(0.2f, 0.6f, 0.3f));
            SetAnchors(regSubmitBtn, new Vector2(0.1f, 0.1f), new Vector2(0.45f, 0.18f));

            var backToLoginBtn = CreateButton(registerPanel.transform, "BackToLoginButton", "Back", new Color(0.3f, 0.3f, 0.5f));
            SetAnchors(backToLoginBtn, new Vector2(0.55f, 0.1f), new Vector2(0.9f, 0.18f));

            // Wire up references
            var so = new SerializedObject(screen);
            so.FindProperty("loginPanel").objectReferenceValue = loginPanel;
            so.FindProperty("registerPanel").objectReferenceValue = registerPanel;
            so.FindProperty("loginEmailInput").objectReferenceValue = emailInput.GetComponent<TMP_InputField>();
            so.FindProperty("loginPasswordInput").objectReferenceValue = passwordInput.GetComponent<TMP_InputField>();
            so.FindProperty("loginButton").objectReferenceValue = loginBtn.GetComponent<Button>();
            so.FindProperty("goToRegisterButton").objectReferenceValue = registerBtn.GetComponent<Button>();
            so.FindProperty("devLoginButton").objectReferenceValue = devLoginBtn.GetComponent<Button>();
            so.FindProperty("registerUsernameInput").objectReferenceValue = usernameInput.GetComponent<TMP_InputField>();
            so.FindProperty("registerEmailInput").objectReferenceValue = regEmailInput.GetComponent<TMP_InputField>();
            so.FindProperty("registerPasswordInput").objectReferenceValue = regPasswordInput.GetComponent<TMP_InputField>();
            so.FindProperty("registerConfirmPasswordInput").objectReferenceValue = confirmPasswordInput.GetComponent<TMP_InputField>();
            so.FindProperty("registerButton").objectReferenceValue = regSubmitBtn.GetComponent<Button>();
            so.FindProperty("goToLoginButton").objectReferenceValue = backToLoginBtn.GetComponent<Button>();
            so.FindProperty("errorText").objectReferenceValue = errorText.GetComponent<TMP_Text>();
            so.ApplyModifiedProperties();

            return screen;
        }

        private static UI.CoopScreen CreateCoopScreen(Transform parent)
        {
            var screenGO = new GameObject("CoopScreen");
            screenGO.transform.SetParent(parent);

            // Add Image first (creates RectTransform), then stretch
            var bg = screenGO.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.14f, 0.18f);
            SetFullStretch(screenGO);
            screenGO.SetActive(false);

            var screen = screenGO.AddComponent<UI.CoopScreen>();

            // Header Panel
            var headerPanel = CreatePanel(screenGO.transform, "HeaderPanel", new Color(0.08f, 0.1f, 0.14f));
            SetAnchors(headerPanel, new Vector2(0, 0.9f), new Vector2(1, 1));

            var usernameText = CreateText(headerPanel.transform, "UsernameText", "Player", 24);
            SetAnchors(usernameText, new Vector2(0.02f, 0.5f), new Vector2(0.2f, 0.5f));
            usernameText.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Left;

            var coinsText = CreateText(headerPanel.transform, "CoinsText", "0", 24);
            SetAnchors(coinsText, new Vector2(0.4f, 0.5f), new Vector2(0.5f, 0.5f));

            var levelText = CreateText(headerPanel.transform, "LevelText", "Lv. 1", 24);
            SetAnchors(levelText, new Vector2(0.6f, 0.5f), new Vector2(0.7f, 0.5f));

            var logoutBtn = CreateButton(headerPanel.transform, "LogoutButton", "Logout", new Color(0.5f, 0.2f, 0.2f));
            SetAnchors(logoutBtn, new Vector2(0.85f, 0.2f), new Vector2(0.98f, 0.8f));

            // Bird Grid Area with scroll
            var gridAreaGO = new GameObject("BirdGridArea");
            gridAreaGO.transform.SetParent(screenGO.transform);
            var gridAreaImage = gridAreaGO.AddComponent<Image>();
            gridAreaImage.color = new Color(0.1f, 0.1f, 0.12f);
            SetAnchors(gridAreaGO, new Vector2(0.02f, 0.15f), new Vector2(0.98f, 0.88f));

            var scrollRect = gridAreaGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(gridAreaGO.transform);
            viewport.AddComponent<Image>().color = Color.clear;
            SetFullStretch(viewport);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            var content = new GameObject("BirdGridContainer");
            content.transform.SetParent(viewport.transform);
            var contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 800);
            scrollRect.content = contentRT;

            var gridLayout = content.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(180, 220);
            gridLayout.spacing = new Vector2(15, 15);
            gridLayout.padding = new RectOffset(20, 20, 20, 20);
            gridLayout.childAlignment = TextAnchor.UpperCenter;

            var contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Selection Panel (bottom)
            var selectionPanel = CreatePanel(screenGO.transform, "SelectionPanel", new Color(0.15f, 0.2f, 0.25f));
            SetAnchors(selectionPanel, new Vector2(0, 0), new Vector2(1, 0.13f));

            var selectionCountText = CreateText(selectionPanel.transform, "SelectionCountText", "0/5", 28);
            SetAnchors(selectionCountText, new Vector2(0.05f, 0.5f), new Vector2(0.15f, 0.5f));

            var totalPowerText = CreateText(selectionPanel.transform, "TotalPowerText", "Power: 0", 24);
            SetAnchors(totalPowerText, new Vector2(0.2f, 0.5f), new Vector2(0.35f, 0.5f));

            var clearBtn = CreateButton(selectionPanel.transform, "ClearSelectionButton", "Clear", new Color(0.4f, 0.3f, 0.3f));
            SetAnchors(clearBtn, new Vector2(0.5f, 0.2f), new Vector2(0.65f, 0.8f));

            var startFlightBtn = CreateButton(selectionPanel.transform, "StartFlightButton", "Start Flight", new Color(0.2f, 0.5f, 0.7f));
            SetAnchors(startFlightBtn, new Vector2(0.7f, 0.2f), new Vector2(0.95f, 0.8f));

            // Wire up references
            var so = new SerializedObject(screen);
            so.FindProperty("usernameText").objectReferenceValue = usernameText.GetComponent<TMP_Text>();
            so.FindProperty("coinsText").objectReferenceValue = coinsText.GetComponent<TMP_Text>();
            so.FindProperty("levelText").objectReferenceValue = levelText.GetComponent<TMP_Text>();
            so.FindProperty("logoutButton").objectReferenceValue = logoutBtn.GetComponent<Button>();
            so.FindProperty("birdGridContainer").objectReferenceValue = content.transform;
            so.FindProperty("selectionPanel").objectReferenceValue = selectionPanel;
            so.FindProperty("selectionCountText").objectReferenceValue = selectionCountText.GetComponent<TMP_Text>();
            so.FindProperty("totalPowerText").objectReferenceValue = totalPowerText.GetComponent<TMP_Text>();
            so.FindProperty("clearSelectionButton").objectReferenceValue = clearBtn.GetComponent<Button>();
            so.FindProperty("startFlightButton").objectReferenceValue = startFlightBtn.GetComponent<Button>();
            so.ApplyModifiedProperties();

            return screen;
        }

        private static UI.FlightScreen CreateFlightScreen(Transform parent)
        {
            var screenGO = new GameObject("FlightScreen");
            screenGO.transform.SetParent(parent);

            // Add Image first (creates RectTransform), then stretch
            var bg = screenGO.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.5f, 0.7f);
            SetFullStretch(screenGO);
            screenGO.SetActive(false);

            var screen = screenGO.AddComponent<UI.FlightScreen>();

            // Timer Panel (top center)
            var timerPanel = CreatePanel(screenGO.transform, "TimerPanel", new Color(0, 0, 0, 0.5f));
            SetAnchors(timerPanel, new Vector2(0.35f, 0.88f), new Vector2(0.65f, 0.98f));

            var timerText = CreateText(timerPanel.transform, "FlightTimerText", "30:00", 48);
            SetAnchors(timerText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            // Info Panel (top left)
            var infoPanel = CreatePanel(screenGO.transform, "InfoPanel", new Color(0, 0, 0, 0.4f));
            SetAnchors(infoPanel, new Vector2(0.02f, 0.8f), new Vector2(0.25f, 0.95f));

            var birdCountText = CreateText(infoPanel.transform, "BirdCountText", "Birds: 0", 18);
            SetAnchors(birdCountText, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f));

            var encounterCountText = CreateText(infoPanel.transform, "EncounterCountText", "Encounters: 0", 18);
            SetAnchors(encounterCountText, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f));

            // Birds Panel (left side)
            var birdsPanel = CreatePanel(screenGO.transform, "BirdsPanel", new Color(0, 0, 0, 0.3f));
            SetAnchors(birdsPanel, new Vector2(0.02f, 0.3f), new Vector2(0.2f, 0.75f));

            var birdsContainer = new GameObject("BirdsContainer");
            birdsContainer.transform.SetParent(birdsPanel.transform);
            birdsContainer.AddComponent<RectTransform>();
            var birdsLayout = birdsContainer.AddComponent<VerticalLayoutGroup>();
            SetFullStretch(birdsContainer);
            birdsLayout.spacing = 5;
            birdsLayout.padding = new RectOffset(10, 10, 10, 10);

            // Position Info (bottom left)
            var posPanel = CreatePanel(screenGO.transform, "PositionPanel", new Color(0, 0, 0, 0.4f));
            SetAnchors(posPanel, new Vector2(0.02f, 0.02f), new Vector2(0.25f, 0.12f));

            var latText = CreateText(posPanel.transform, "LatitudeText", "Lat: 0.0000", 14);
            SetAnchors(latText, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f));

            var lonText = CreateText(posPanel.transform, "LongitudeText", "Lon: 0.0000", 14);
            SetAnchors(lonText, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f));

            // End Flight Button (bottom center)
            var endFlightBtn = CreateButton(screenGO.transform, "EndFlightButton", "End Flight", new Color(0.6f, 0.2f, 0.2f));
            SetAnchors(endFlightBtn, new Vector2(0.35f, 0.02f), new Vector2(0.65f, 0.1f));

            // Wire up references
            var so = new SerializedObject(screen);
            so.FindProperty("flightTimerText").objectReferenceValue = timerText.GetComponent<TMP_Text>();
            so.FindProperty("birdCountText").objectReferenceValue = birdCountText.GetComponent<TMP_Text>();
            so.FindProperty("encounterCountText").objectReferenceValue = encounterCountText.GetComponent<TMP_Text>();
            so.FindProperty("endFlightButton").objectReferenceValue = endFlightBtn.GetComponent<Button>();
            so.FindProperty("latitudeText").objectReferenceValue = latText.GetComponent<TMP_Text>();
            so.FindProperty("longitudeText").objectReferenceValue = lonText.GetComponent<TMP_Text>();
            so.FindProperty("birdsContainer").objectReferenceValue = birdsContainer.transform;
            so.ApplyModifiedProperties();

            return screen;
        }

        private static UI.EncounterScreen CreateEncounterScreen(Transform parent)
        {
            var screenGO = new GameObject("EncounterScreen");
            screenGO.transform.SetParent(parent);

            // Add Image first (creates RectTransform), then stretch
            var overlay = screenGO.AddComponent<Image>();
            overlay.color = new Color(0, 0, 0, 0.7f);
            SetFullStretch(screenGO);
            screenGO.SetActive(false);

            var screen = screenGO.AddComponent<UI.EncounterScreen>();

            // Encounter Panel
            var encounterPanel = CreatePanel(screenGO.transform, "EncounterPanel", new Color(0.15f, 0.15f, 0.2f));
            SetAnchors(encounterPanel, new Vector2(0.15f, 0.2f), new Vector2(0.85f, 0.8f));

            var titleText = CreateText(encounterPanel.transform, "TitleText", "Encounter!", 36);
            SetAnchors(titleText, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f));

            var timerText = CreateText(encounterPanel.transform, "TimerText", "30s", 28);
            SetAnchors(timerText, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f));
            timerText.GetComponent<TMP_Text>().color = Color.yellow;

            // Player Side
            var playerPanel = CreatePanel(encounterPanel.transform, "PlayerPanel", new Color(0.1f, 0.3f, 0.2f));
            SetAnchors(playerPanel, new Vector2(0.05f, 0.25f), new Vector2(0.45f, 0.75f));

            var playerNameText = CreateText(playerPanel.transform, "PlayerNameText", "You", 24);
            SetAnchors(playerNameText, new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f));

            var playerPowerText = CreateText(playerPanel.transform, "PlayerPowerText", "0", 32);
            SetAnchors(playerPowerText, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f));

            var playerBirdsContainer = new GameObject("PlayerBirdsContainer");
            playerBirdsContainer.transform.SetParent(playerPanel.transform);
            playerBirdsContainer.AddComponent<RectTransform>();
            SetAnchors(playerBirdsContainer, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.6f));

            // VS Text
            var vsText = CreateText(encounterPanel.transform, "VsText", "VS", 48);
            SetAnchors(vsText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            vsText.GetComponent<TMP_Text>().color = Color.red;

            // Opponent Side
            var opponentPanel = CreatePanel(encounterPanel.transform, "OpponentPanel", new Color(0.3f, 0.1f, 0.1f));
            SetAnchors(opponentPanel, new Vector2(0.55f, 0.25f), new Vector2(0.95f, 0.75f));

            var opponentNameText = CreateText(opponentPanel.transform, "OpponentNameText", "Opponent", 24);
            SetAnchors(opponentNameText, new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f));

            var opponentPowerText = CreateText(opponentPanel.transform, "OpponentPowerText", "0", 32);
            SetAnchors(opponentPowerText, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f));

            var opponentBirdsContainer = new GameObject("OpponentBirdsContainer");
            opponentBirdsContainer.transform.SetParent(opponentPanel.transform);
            opponentBirdsContainer.AddComponent<RectTransform>();
            SetAnchors(opponentBirdsContainer, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.6f));

            // Win Chance Slider
            var winChanceSlider = CreateSlider(encounterPanel.transform, "WinChanceSlider");
            SetAnchors(winChanceSlider, new Vector2(0.1f, 0.12f), new Vector2(0.7f, 0.18f));

            var winChanceText = CreateText(encounterPanel.transform, "WinChanceText", "50%", 20);
            SetAnchors(winChanceText, new Vector2(0.8f, 0.15f), new Vector2(0.95f, 0.15f));

            // Result Panel (hidden initially)
            var resultPanel = CreatePanel(screenGO.transform, "ResultPanel", new Color(0.15f, 0.15f, 0.2f));
            SetAnchors(resultPanel, new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.75f));
            resultPanel.SetActive(false);

            var resultText = CreateText(resultPanel.transform, "ResultText", "Victory!", 48);
            SetAnchors(resultText, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f));

            var coinsChangeText = CreateText(resultPanel.transform, "CoinsChangeText", "+100 Coins", 24);
            SetAnchors(coinsChangeText, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f));

            var expGainedText = CreateText(resultPanel.transform, "ExpGainedText", "+50 XP", 24);
            SetAnchors(expGainedText, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f));

            var birdsLostText = CreateText(resultPanel.transform, "BirdsLostText", "", 20);
            SetAnchors(birdsLostText, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f));

            var birdsGainedText = CreateText(resultPanel.transform, "BirdsGainedText", "", 20);
            SetAnchors(birdsGainedText, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f));

            var continueBtn = CreateButton(resultPanel.transform, "ContinueButton", "Continue", new Color(0.2f, 0.5f, 0.3f));
            SetAnchors(continueBtn, new Vector2(0.3f, 0.1f), new Vector2(0.7f, 0.2f));

            // Wire up references
            var so = new SerializedObject(screen);
            so.FindProperty("titleText").objectReferenceValue = titleText.GetComponent<TMP_Text>();
            so.FindProperty("timerText").objectReferenceValue = timerText.GetComponent<TMP_Text>();
            so.FindProperty("encounterPanel").objectReferenceValue = encounterPanel;
            so.FindProperty("resultPanel").objectReferenceValue = resultPanel;
            so.FindProperty("playerNameText").objectReferenceValue = playerNameText.GetComponent<TMP_Text>();
            so.FindProperty("playerPowerText").objectReferenceValue = playerPowerText.GetComponent<TMP_Text>();
            so.FindProperty("playerBirdsContainer").objectReferenceValue = playerBirdsContainer.transform;
            so.FindProperty("opponentNameText").objectReferenceValue = opponentNameText.GetComponent<TMP_Text>();
            so.FindProperty("opponentPowerText").objectReferenceValue = opponentPowerText.GetComponent<TMP_Text>();
            so.FindProperty("opponentBirdsContainer").objectReferenceValue = opponentBirdsContainer.transform;
            so.FindProperty("winChanceSlider").objectReferenceValue = winChanceSlider.GetComponent<Slider>();
            so.FindProperty("winChanceText").objectReferenceValue = winChanceText.GetComponent<TMP_Text>();
            so.FindProperty("resultText").objectReferenceValue = resultText.GetComponent<TMP_Text>();
            so.FindProperty("coinsChangeText").objectReferenceValue = coinsChangeText.GetComponent<TMP_Text>();
            so.FindProperty("expGainedText").objectReferenceValue = expGainedText.GetComponent<TMP_Text>();
            so.FindProperty("birdsLostText").objectReferenceValue = birdsLostText.GetComponent<TMP_Text>();
            so.FindProperty("birdsGainedText").objectReferenceValue = birdsGainedText.GetComponent<TMP_Text>();
            so.FindProperty("continueButton").objectReferenceValue = continueBtn.GetComponent<Button>();
            so.ApplyModifiedProperties();

            return screen;
        }

        private static void CreatePrefabs()
        {
            string prefabPath = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabPath))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            CreateBirdCardPrefab(prefabPath);
            CreateFlightBirdCardPrefab(prefabPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Assign prefab to CoopScreen
            var coopScreen = Object.FindFirstObjectByType<UI.CoopScreen>();
            if (coopScreen != null)
            {
                var birdCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/BirdCard.prefab");
                var so = new SerializedObject(coopScreen);
                so.FindProperty("birdCardPrefab").objectReferenceValue = birdCardPrefab;
                so.ApplyModifiedProperties();
            }

            Debug.Log("Created prefabs");
        }

        private static void CreateBirdCardPrefab(string path)
        {
            var cardGO = new GameObject("BirdCard");

            var rt = cardGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(180, 220);

            // Background
            var bg = cardGO.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.22f, 0.28f);

            // Add button
            var button = cardGO.AddComponent<Button>();
            button.targetGraphic = bg;

            // Add BirdCardUI
            var cardUI = cardGO.AddComponent<UI.BirdCardUI>();

            // Rarity border
            var borderGO = new GameObject("RarityBorder");
            borderGO.transform.SetParent(cardGO.transform);
            SetFullStretch(borderGO);
            var border = borderGO.AddComponent<Image>();
            border.color = Color.gray;
            var borderOutline = borderGO.AddComponent<Outline>();
            borderOutline.effectColor = Color.gray;
            borderOutline.effectDistance = new Vector2(3, -3);

            // Bird image area
            var birdImageGO = new GameObject("BirdImage");
            birdImageGO.transform.SetParent(cardGO.transform);
            SetAnchors(birdImageGO, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.9f));
            var birdImage = birdImageGO.AddComponent<Image>();
            birdImage.color = new Color(0.5f, 0.5f, 0.6f);

            // Name text
            var nameGO = CreateText(cardGO.transform, "NameText", "Bird Name", 16);
            SetAnchors(nameGO, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f));

            // Power text
            var powerGO = CreateText(cardGO.transform, "PowerText", "100", 20);
            SetAnchors(powerGO, new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f));

            // Health bar
            var healthBar = CreateSlider(cardGO.transform, "HealthBar");
            SetAnchors(healthBar, new Vector2(0.1f, 0.08f), new Vector2(0.48f, 0.12f));
            healthBar.GetComponent<Slider>().fillRect.GetComponent<Image>().color = Color.green;

            // Stamina bar
            var staminaBar = CreateSlider(cardGO.transform, "StaminaBar");
            SetAnchors(staminaBar, new Vector2(0.52f, 0.08f), new Vector2(0.9f, 0.12f));
            staminaBar.GetComponent<Slider>().fillRect.GetComponent<Image>().color = Color.yellow;

            // Selected indicator
            var selectedGO = new GameObject("SelectedIndicator");
            selectedGO.transform.SetParent(cardGO.transform);
            SetFullStretch(selectedGO);
            var selectedImage = selectedGO.AddComponent<Image>();
            selectedImage.color = new Color(0.3f, 0.8f, 0.3f, 0.3f);
            selectedGO.SetActive(false);

            // Element icon
            var elementGO = new GameObject("ElementIcon");
            elementGO.transform.SetParent(cardGO.transform);
            SetAnchors(elementGO, new Vector2(0.8f, 0.85f), new Vector2(0.95f, 0.95f));
            var elementImage = elementGO.AddComponent<Image>();
            elementImage.color = Color.white;

            // Wire up BirdCardUI
            var so = new SerializedObject(cardUI);
            so.FindProperty("backgroundImage").objectReferenceValue = bg;
            so.FindProperty("birdImage").objectReferenceValue = birdImage;
            so.FindProperty("nameText").objectReferenceValue = nameGO.GetComponent<TMP_Text>();
            so.FindProperty("powerText").objectReferenceValue = powerGO.GetComponent<TMP_Text>();
            so.FindProperty("rarityBorder").objectReferenceValue = border;
            so.FindProperty("healthBar").objectReferenceValue = healthBar.GetComponent<Slider>();
            so.FindProperty("staminaBar").objectReferenceValue = staminaBar.GetComponent<Slider>();
            so.FindProperty("selectedIndicator").objectReferenceValue = selectedGO;
            so.FindProperty("elementIcon").objectReferenceValue = elementImage;
            so.ApplyModifiedProperties();

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(cardGO, $"{path}/BirdCard.prefab");
            Object.DestroyImmediate(cardGO);
        }

        private static void CreateFlightBirdCardPrefab(string path)
        {
            var cardGO = new GameObject("FlightBirdCard");

            var rt = cardGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 60);

            var bg = cardGO.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f);

            var textGO = CreateText(cardGO.transform, "InfoText", "Bird\nPwr: 0", 14);
            SetAnchors(textGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            PrefabUtility.SaveAsPrefabAsset(cardGO, $"{path}/FlightBirdCard.prefab");
            Object.DestroyImmediate(cardGO);
        }

        #region Helper Methods

        private static void SetFullStretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetAnchors(GameObject go, Vector2 min, Vector2 max)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = new Vector2(0.5f, 0.5f);

            // For stretching anchors (min != max), use offsets to fill the anchor area
            // For point anchors (min == max), preserve sizeDelta for explicit sizing
            if (min != max)
            {
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                // Center on the anchor point
                rt.anchoredPosition = Vector2.zero;
            }
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var panelGO = new GameObject(name);
            panelGO.transform.SetParent(parent);
            var image = panelGO.AddComponent<Image>();
            image.color = color;
            return panelGO;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize)
        {
            var textGO = new GameObject(name);
            textGO.transform.SetParent(parent);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            // Set default size for point anchor positioning
            var rt = textGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, fontSize + 10);

            return textGO;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Color color)
        {
            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent);

            var image = buttonGO.AddComponent<Image>();
            image.color = color;

            // Set default size for point anchor positioning
            var rt = buttonGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 40);

            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = image;

            var textGO = CreateText(buttonGO.transform, "Text", text, 18);
            SetFullStretch(textGO);

            return buttonGO;
        }

        private static GameObject CreateInputField(Transform parent, string name, string placeholder, bool isPassword = false)
        {
            var inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent);

            var image = inputGO.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.12f);

            // Set default size for point anchor positioning
            var rt = inputGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 40);

            // Text Area
            var textAreaGO = new GameObject("Text Area");
            textAreaGO.transform.SetParent(inputGO.transform);
            SetFullStretch(textAreaGO);
            var textAreaRT = textAreaGO.GetComponent<RectTransform>();
            textAreaRT.offsetMin = new Vector2(10, 5);
            textAreaRT.offsetMax = new Vector2(-10, -5);
            textAreaGO.AddComponent<RectMask2D>();

            // Placeholder
            var placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(textAreaGO.transform);
            SetFullStretch(placeholderGO);
            var placeholderTMP = placeholderGO.AddComponent<TextMeshProUGUI>();
            placeholderTMP.text = placeholder;
            placeholderTMP.fontSize = 18;
            placeholderTMP.color = new Color(0.5f, 0.5f, 0.5f);
            placeholderTMP.alignment = TextAlignmentOptions.Left;

            // Text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(textAreaGO.transform);
            SetFullStretch(textGO);
            var textTMP = textGO.AddComponent<TextMeshProUGUI>();
            textTMP.fontSize = 18;
            textTMP.color = Color.white;
            textTMP.alignment = TextAlignmentOptions.Left;

            var input = inputGO.AddComponent<TMP_InputField>();
            input.textViewport = textAreaGO.GetComponent<RectTransform>();
            input.textComponent = textTMP;
            input.placeholder = placeholderTMP;

            if (isPassword)
            {
                input.contentType = TMP_InputField.ContentType.Password;
            }

            return inputGO;
        }

        private static GameObject CreateSlider(Transform parent, string name)
        {
            var sliderGO = new GameObject(name);
            sliderGO.transform.SetParent(parent);

            // Add Image to create RectTransform, then set default size
            var sliderBg = sliderGO.AddComponent<Image>();
            sliderBg.color = Color.clear; // Invisible container
            var rt = sliderGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 20);

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(sliderGO.transform);
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);
            SetFullStretch(bgGO);

            var fillAreaGO = new GameObject("Fill Area");
            fillAreaGO.transform.SetParent(sliderGO.transform);
            SetFullStretch(fillAreaGO);
            var fillAreaRT = fillAreaGO.GetComponent<RectTransform>();
            fillAreaRT.offsetMin = new Vector2(0, 0);
            fillAreaRT.offsetMax = new Vector2(0, 0);

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillAreaGO.transform);
            SetFullStretch(fillGO);
            var fillImage = fillGO.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.6f, 0.3f);

            var slider = sliderGO.AddComponent<Slider>();
            slider.fillRect = fillGO.GetComponent<RectTransform>();
            slider.targetGraphic = bgImage;
            slider.interactable = false;

            return sliderGO;
        }

        #endregion
    }
}
#endif
