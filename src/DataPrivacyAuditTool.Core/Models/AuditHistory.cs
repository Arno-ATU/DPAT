using System.ComponentModel.DataAnnotations;

namespace DataPrivacyAuditTool.Core.Models
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
    }
}
