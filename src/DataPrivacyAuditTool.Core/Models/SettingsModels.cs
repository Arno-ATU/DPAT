using System.Text.Json.Serialization;

namespace DataPrivacyAuditTool.Core.Models
{
    public class SettingsData
    {
        [JsonPropertyName("App Settings")]
        public List<object> AppSettings { get; set; } = new List<object>();

        [JsonPropertyName("App Lists")]
        public List<object> AppLists { get; set; } = new List<object>();

        [JsonPropertyName("Search Engines")]
        public List<SearchEngine> SearchEngines { get; set; } = new List<SearchEngine>();

        [JsonPropertyName("Apps")]
        public List<App> Apps { get; set; } = new List<App>();

        [JsonPropertyName("Preferences")]
        public List<Preference> Preferences { get; set; } = new List<Preference>();

        [JsonPropertyName("Themes")]
        public List<object> Themes { get; set; } = new List<object>();

        [JsonPropertyName("Priority Preferences")]
        public List<PriorityPreference> PriorityPreferences { get; set; } = new List<PriorityPreference>();

        [JsonPropertyName("Web Apps")]
        public List<object> WebApps { get; set; } = new List<object>();
    }

    public class SearchEngine
    {
        [JsonPropertyName("short_name")]
        public string ShortName { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("favicon_url")]
        public string FaviconUrl { get; set; }

        [JsonPropertyName("suggestions_url")]
        public string SuggestionsUrl { get; set; }

        [JsonPropertyName("safe_for_autoreplace")]
        public bool SafeForAutoreplace { get; set; }

        [JsonPropertyName("is_active")]
        public string IsActive { get; set; }

        [JsonPropertyName("sync_guid")]
        public string SyncGuid { get; set; }
    }

    public class App
    {
        [JsonPropertyName("app_launch_ordinal")]
        public string AppLaunchOrdinal { get; set; }

        [JsonPropertyName("extension")]
        public Extension Extension { get; set; }

        [JsonPropertyName("page_ordinal")]
        public string PageOrdinal { get; set; }
    }

    public class Extension
    {
        [JsonPropertyName("incognito_enabled")]
        public bool IncognitoEnabled { get; set; }

        [JsonPropertyName("remote_install")]
        public bool RemoteInstall { get; set; }

        [JsonPropertyName("disable_reasons")]
        public int DisableReasons { get; set; }

        [JsonPropertyName("update_url")]
        public string UpdateUrl { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }

    public class Preference
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class PriorityPreference
    {
        [JsonPropertyName("preference")]
        public PreferenceItem Preference { get; set; }
    }

    public class PreferenceItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
