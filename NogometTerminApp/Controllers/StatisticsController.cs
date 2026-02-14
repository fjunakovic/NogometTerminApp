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
                Result = t.Result,
                IsPast = t.TermDateTime < now
            }).ToList();

            return View(model);
        }
    }
}