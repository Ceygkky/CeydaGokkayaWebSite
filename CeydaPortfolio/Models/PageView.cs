using System.ComponentModel.DataAnnotations;

namespace CeydaPortfolio.Models
{
    public class PageView
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string PageName { get; set; } = string.Empty;

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}