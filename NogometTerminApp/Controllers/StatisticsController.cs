using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NogometTerminApp.Data;
using NogometTerminApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NogometTerminApp.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Statistics/Index
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var terms = await _context.Terms
                .Include(t => t.Registrations)
                .ThenInclude(r => r.Player)
                .OrderByDescending(t => t.TermDateTime)
                .ToListAsync();

            var model = terms.Select(t => new TermStatisticsViewModel
            {
                TermId = t.Id,
                TermDateTime = t.TermDateTime,
                Location = t.Location,
                MaxPlayers = t.MaxPlayers,
                RegisteredCount = t.Registrations.Count,
                IsPast = t.TermDateTime < now
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTerms(int[] termIds)
        {
            if (termIds == null || termIds.Length == 0)
            {
                return RedirectToAction("Index");
            }

            // obriši prijave za sve označene termine
            var registrations = await _context.TermRegistrations
                .Where(r => termIds.Contains(r.TermId))
                .ToListAsync();

            if (registrations.Any())
            {
                _context.TermRegistrations.RemoveRange(registrations);
            }

            // obriši same termine
            var terms = await _context.Terms
                .Where(t => termIds.Contains(t.Id))
                .ToListAsync();

            if (terms.Any())
            {
                _context.Terms.RemoveRange(terms);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
