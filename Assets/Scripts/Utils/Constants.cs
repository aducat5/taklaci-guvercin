namespace TaklaciGuvercin.Utils
{
    /// <summary>
    /// Game-wide constants and configuration values.
    /// </summary>
    public static class Constants
    {
        // API Configuration
        public const string DEFAULT_API_URL = "http://localhost:5000/api";
        public const string DEFAULT_SIGNALR_URL = "ws://localhost:5000/hubs/airspace";

        // Flight Configuration
        public const int MIN_FLIGHT_DURATION_MINUTES = 5;
        public const int MAX_FLIGHT_DURATION_MINUTES = 60;
        public const int DEFAULT_FLIGHT_DURATION_MINUTES = 15;
        public const int MAX_BIRDS_PER_FLIGHT = 5;
        public const float POSITION_UPDATE_INTERVAL = 5f;

        // Encounter Configuration
        public const int ENCOUNTER_TIMEOUT_SECONDS = 30;
        public const float ENCOUNTER_RANGE_METERS = 500f;

        // Bird Configuration
        public const int MIN_STAMINA_FOR_FLIGHT = 20;
        public const int STAMINA_COST_PER_FLIGHT = 20;

        // UI Configuration
        public const float UI_ANIMATION_DURATION = 0.3f;
        public const float NOTIFICATION_DISPLAY_TIME = 3f;

        // PlayerPrefs Keys
        public const string PREF_PLAYER_ID = "PlayerId";
        public const string PREF_AUTH_TOKEN = "AuthToken";
        public const string PREF_SOUND_ENABLED = "SoundEnabled";
        public const string PREF_MUSIC_ENABLED = "MusicEnabled";
    }
}
