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
                IsPast = t.TermDateTime < now,
                IsPostponed = t.IsPostponed
            }).ToList();

            return View(model);
        }
        public async Task<IActionResult> Manage()
        {
            var now = DateTime.Now;

            var terms = await _context.Terms
                .Include(t => t.Registrations)
                .OrderByDescending(t => t.TermDateTime)
                .ToListAsync();

            var model = terms.Select(t => new TermStatisticsViewModel
            {
                TermId = t.Id,
                TermDateTime = t.TermDateTime,
                Location = t.Location,
                MaxPlayers = t.MaxPlayers,
                RegisteredCount = t.Registrations.Count,
                IsPast = t.TermDateTime < now,
                Result = t.Result,
                IsPostponed = t.IsPostponed
            });

            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var term = await _context.Terms
                .Include(t => t.Registrations)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (term == null)
            {
                return NotFound();
            }

            var vm = new TermStatisticsViewModel
            {
                TermId = term.Id,
                TermDateTime = term.TermDateTime,
                Location = term.Location,
                MaxPlayers = term.MaxPlayers,
                RegisteredCount = term.Registrations.Count,
                IsPast = term.TermDateTime < DateTime.Now,
                Result = term.Result,
                IsInEditMode = true,
                IsPostponed = term.IsPostponed
            };

            return View(vm);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var term = await _context.Terms
                .Include(t => t.Registrations)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (term == null)
            {
                return NotFound();
            }

            var vm = new TermStatisticsViewModel
            {
                TermId = term.Id,
                TermDateTime = term.TermDateTime,
                Location = term.Location,
                RegisteredCount = term.Registrations.Count,
                Result = term.Result
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TermStatisticsViewModel vm)
        {
            // privremeno ignoriramo ModelState za ovu simple formu

            var term = await _context.Terms.FindAsync(vm.TermId);
            if (term == null)
            {
                return NotFound();
            }

            term.Result = vm.Result;
            term.IsPostponed = vm.IsPostponed;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int termId)
        {
            var term = await _context.Terms
                .Include(t => t.Registrations)
                .FirstOrDefaultAsync(t => t.Id == termId);

            if (term == null)
            {
                return NotFound();
            }

            _context.TermRegistrations.RemoveRange(term.Registrations);
            _context.Terms.Remove(term);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Postpone(int id)
        {
            var term = await _context.Terms.FindAsync(id);
            if (term == null)
            {
                return NotFound();
            }

            term.IsPostponed = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnPostpone(int id)
        {
            var term = await _context.Terms.FindAsync(id);
            if (term == null)
            {
                return NotFound();
            }

            term.IsPostponed = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }

    }
}