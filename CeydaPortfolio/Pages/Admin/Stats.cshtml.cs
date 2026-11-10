using CeydaPortfolio.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CeydaPortfolio.Pages.Admin
{
    public class StatsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public StatsModel(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public int TotalViews { get; set; }
        public int Last7Days { get; set; }
        public int Today { get; set; }
        public List<PageCount> TopPages { get; set; } = new();
        public List<DayCount> Daily { get; set; } = new();

        public record PageCount(string PageName, int Count);
        public record DayCount(DateTime Day, int Count);

        public async Task<IActionResult> OnGetAsync()
        {
            // Giriþ sistemi eklenene kadar bu sayfa sadece kendi bilgisayarýnda açýlýr
            if (!_env.IsDevelopment())
            {
                return NotFound();
            }

            var offset = TimeSpan.FromHours(3); // Türkiye saati (UTC+3)
            var todayLocal = (DateTime.UtcNow + offset).Date;

            TotalViews = await _db.PageViews.CountAsync();

            // Son 14 günün kayýtlarýný al
            var since = todayLocal.AddDays(-13) - offset;
            var recent = await _db.PageViews
                .Where(p => p.ViewedAt >= since)
                .Select(p => p.ViewedAt)
                .ToListAsync();

            var recentLocal = recent.Select(d => d + offset).ToList();

            Today = recentLocal.Count(d => d.Date == todayLocal);
            Last7Days = recentLocal.Count(d => d.Date >= todayLocal.AddDays(-6));

            Daily = Enumerable.Range(0, 14)
                .Select(i => todayLocal.AddDays(-13 + i))
                .Select(day => new DayCount(day, recentLocal.Count(d => d.Date == day)))
                .ToList();

            var top = await _db.PageViews
                .GroupBy(p => p.PageName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            TopPages = top.Select(x => new PageCount(x.Name, x.Count)).ToList();

            return Page();
        }
    }
}