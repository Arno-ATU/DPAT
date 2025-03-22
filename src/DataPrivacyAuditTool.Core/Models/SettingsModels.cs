
namespace DataPrivacyAuditTool.Core.Models
{
    public class SettingsData
    {
        public List<object> AppSettings { get; set; } = new List<object>();
        public List<object> AppLists { get; set; } = new List<object>();
        public List<SearchEngine> SearchEngines { get; set; } = new List<SearchEngine>();
        public List<App> Apps { get; set; } = new List<App>();
        public List<Preference> Preferences { get; set; } = new List<Preference>();
        public List<object> Themes { get; set; } = new List<object>();
        public List<PriorityPreference> PriorityPreferences { get; set; } = new List<PriorityPreference>();
        public List<object> WebApps { get; set; } = new List<object>();
    }

    public class SearchEngine
    {
        public string ShortName { get; set; }
        public string Url { get; set; }
        public string FaviconUrl { get; set; }
        public bool SafeForAutoreplace { get; set; }
        public string IsActive { get; set; }
        public string SyncGuid { get; set; }
        // Add other properties as needed
    }

    public class App
    {
        public string AppLaunchOrdinal { get; set; }
        public Extension Extension { get; set; }
        public string PageOrdinal { get; set; }
    }

    public class Extension
    {
        public bool IncognitoEnabled { get; set; }
        public bool RemoteInstall { get; set; }
        public int DisableReasons { get; set; }
        public string UpdateUrl { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public bool Enabled { get; set; }
    }

    public class Preference
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class PriorityPreference
    {
        public PreferenceItem Preference { get; set; }
    }

    public class PreferenceItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
