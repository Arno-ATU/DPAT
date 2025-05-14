using System.ComponentModel.DataAnnotations;

namespace DataPrivacyAuditTool.Models
{
    public class AuditHistory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        public DateTime AuditDate { get; set; }

        public double OverallScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional: Store which files were analyzed
        public bool SettingsFileAnalyzed { get; set; }
        public bool AddressesFileAnalyzed { get; set; }
    }
}
