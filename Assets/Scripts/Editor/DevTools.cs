#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TaklaciGuvercin.Managers;

namespace TaklaciGuvercin.Editor
{
    /// <summary>
    /// Developer tools menu for quick testing.
    /// </summary>
    public class DevTools : EditorWindow
    {
        [MenuItem("Taklaci Guvercin/Dev Login Player 1 %&1")]
        public static void DevLoginPlayer1()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Game must be playing to use Dev Login");
                return;
            }
            AuthManager.Instance?.DevLogin(1);
        }

        [MenuItem("Taklaci Guvercin/Dev Login Player 2 %&2")]
        public static void DevLoginPlayer2()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Game must be playing to use Dev Login");
                return;
            }
            AuthManager.Instance?.DevLogin(2);
        }

        [MenuItem("Taklaci Guvercin/Dev Login Player 3 %&3")]
        public static void DevLoginPlayer3()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Game must be playing to use Dev Login");
                return;
            }
            AuthManager.Instance?.DevLogin(3);
        }

        [MenuItem("Taklaci Guvercin/Open Backend Swagger")]
        public static void OpenSwagger()
        {
            Application.OpenURL("http://localhost:5000/swagger");
        }

        [MenuItem("Taklaci Guvercin/Start Backend Server")]
        public static void StartBackend()
        {
            var projectPath = Application.dataPath.Replace("/Assets", "");
            var backendPath = $"{projectPath}/Backend/TaklaciGuvercin.Api";

            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c cd \"{backendPath}\" && dotnet run";
            process.StartInfo.UseShellExecute = true;
            process.Start();

            Debug.Log($"Starting backend server at {backendPath}...");
            EditorUtility.DisplayDialog("Backend Server",
                "Backend server is starting...\n\nCheck the command window for status.\nSwagger UI: http://localhost:5000/swagger",
                "OK");
        }

        [MenuItem("Taklaci Guvercin/Quick Test Flight")]
        public static void QuickTestFlight()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Game must be playing");
                return;
            }

            if (!GameManager.Instance?.IsLoggedIn == true)
            {
                Debug.LogWarning("Must be logged in first");
                return;
            }

            var birds = BirdManager.Instance?.PlayerBirds;
            if (birds == null || birds.Count == 0)
            {
                Debug.LogWarning("No birds loaded. Wait for birds to load first.");
                return;
            }

            // Select first 3 ready birds
            int count = 0;
            foreach (var bird in birds)
            {
                if (BirdManager.Instance.CanSelectBird(bird))
                {
                    BirdManager.Instance.SelectBird(bird);
                    count++;
                    if (count >= 3) break;
                }
            }

            if (count > 0)
            {
                FlightManager.Instance?.StartFlight(BirdManager.Instance.GetSelectedBirdIds(), 5);
                Debug.Log($"Started test flight with {count} birds");
            }
            else
            {
                Debug.LogWarning("No birds available for flight");
            }
        }

        [MenuItem("Taklaci Guvercin/Documentation/Open GitHub")]
        public static void OpenGitHub()
        {
            Application.OpenURL("https://github.com/aducat5/taklaci-guvercin");
        }
    }
}
#endif
