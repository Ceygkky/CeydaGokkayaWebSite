using System.ComponentModel.DataAnnotations;
using CeydaPortfolio.Data;
using CeydaPortfolio.Models;
using CeydaPortfolio.Services; // YENÝ
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CeydaPortfolio.Pages
{
    public class ContactModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ContactModel> _logger;
        private readonly EmailNotifier _notifier; // YENÝ

        public ContactModel(ApplicationDbContext db, ILogger<ContactModel> logger, EmailNotifier notifier) // YENÝ
        {
            _db = db;
            _logger = logger;
            _notifier = notifier; // YENÝ
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Lütfen adýnýzý yazýn.")]
            [MaxLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Lütfen e-posta adresinizi yazýn.")]
            [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi yazýn.")]
            [MaxLength(150, ErrorMessage = "E-posta en fazla 150 karakter olabilir.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Lütfen mesajýnýzý yazýn.")]
            [MaxLength(2000, ErrorMessage = "Mesaj en fazla 2000 karakter olabilir.")]
            public string Message { get; set; } = string.Empty;

            // Bot tuzaðý: insanlar bu gizli alaný görmez, botlar doldurur
            public string? Website { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Bot tuzaðý doluysa kaydetme, ama bota belli etme
            if (!string.IsNullOrWhiteSpace(Input.Website))
            {
                SuccessMessage = "Mesajýnýz alýndý, teþekkürler!";
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            ContactMessage contactMessage; // YENÝ

            try
            {
                var email = Input.Email.Trim();
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);

                // Ayný e-postadan saatte en fazla 3 mesaj
                var recentCount = await _db.ContactMessages
                    .CountAsync(m => m.Email == email && m.CreatedAt >= oneHourAgo);

                if (recentCount >= 3)
                {
                    ErrorMessage = "Kýsa sürede çok fazla mesaj gönderdiniz. Lütfen biraz sonra tekrar deneyin.";
                    return Page();
                }

                contactMessage = new ContactMessage // YENÝ
                {
                    Name = Input.Name.Trim(),
                    Email = email,
                    Message = Input.Message.Trim()
                };

                _db.ContactMessages.Add(contactMessage); // YENÝ
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ýletiþim mesajý kaydedilemedi.");
                ErrorMessage = "Mesajýnýz þu an gönderilemedi. Lütfen aþaðýdaki e-posta adresiyle ulaþýn.";
                return Page();
            }

            await _notifier.SendContactNotificationAsync(contactMessage); // YENÝ

            SuccessMessage = "Mesajýnýz alýndý, teþekkürler!";
            return RedirectToPage();
        }
    }
}