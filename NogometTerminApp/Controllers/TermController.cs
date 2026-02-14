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
    public class TermController : Controller
    {
        private readonly AppDbContext _context;

        public TermController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var termDateTime = GetCurrentSaturday20();

            var term = await _context.Terms
                .Include(t => t.Registrations)
                .ThenInclude(r => r.Player)
                .FirstOrDefaultAsync(t => t.TermDateTime == termDateTime);

            if (term == null)
            {
                term = new Term
                {
                    TermDateTime = termDateTime,
                    Location = "Zagreb - ŠD Hotanj",
                    MaxPlayers = 14
                };

                _context.Terms.Add(term);
                await _context.SaveChangesAsync();
            }

            var registrations = term.Registrations?
                .OrderBy(r => r.RegisteredAt)
                .Select(r => new TermRegistrationInfo
                {
                    RegistrationId = r.Id,
                    PlayerName = r.Player.Name
                })
                .ToList() ?? new List<TermRegistrationInfo>();

            var vm = new TermRegisterViewModel
            {
                TermId = term.Id,
                CurrentCount = registrations.Count,
                MaxPlayers = term.MaxPlayers,
                Registrations = registrations
            };

            return View(vm);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var term = await _context.Terms.FindAsync(id);
            if (term == null)
            {
                return NotFound();
            }
            return View(term);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(TermRegisterViewModel model)
        {
            var term = await _context.Terms
                .Include(t => t.Registrations)
                .ThenInclude(r => r.Player)
                .FirstOrDefaultAsync(t => t.Id == model.TermId);

            if (term == null)
            {
                return NotFound();
            }

            var currentCount = term.Registrations.Count;

            if (currentCount >= term.MaxPlayers)
            {
                ModelState.AddModelError(string.Empty, "Termin je pun (maksimalno 14 igrača).");
            }

            if (!ModelState.IsValid)
            {
                var registrations = term.Registrations
                    .OrderBy(r => r.RegisteredAt)
                    .Select(r => new TermRegistrationInfo
                    {
                        RegistrationId = r.Id,
                        PlayerName = r.Player.Name
                    })
                    .ToList();

                model.CurrentCount = registrations.Count;
                model.MaxPlayers = term.MaxPlayers;
                model.Registrations = registrations;

                return View("Index", model);
            }

            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Email == model.Email);

            if (player == null)
            {
                player = new Player
                {
                    Name = model.Name,
                    Email = model.Email
                };
                _context.Players.Add(player);
                await _context.SaveChangesAsync();
            }

            var registration = new TermRegistration
            {
                TermId = term.Id,
                PlayerId = player.Id,
                RegisteredAt = DateTime.Now
            };

            _context.TermRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int registrationId)
        {
            var registration = await _context.TermRegistrations
                .FirstOrDefaultAsync(r => r.Id == registrationId);

            if (registration != null)
            {
                _context.TermRegistrations.Remove(registration);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        private DateTime GetCurrentSaturday20()
        {
            var today = DateTime.Today;
            int todayDow = (int)today.DayOfWeek;      
            int targetDow = (int)DayOfWeek.Saturday;

            int daysUntilSaturday = targetDow - todayDow;
            if (daysUntilSaturday < 0)
            {
                daysUntilSaturday += 7;
            }

            var saturday = today.AddDays(daysUntilSaturday);
            return saturday.AddHours(20);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Term term)
        {
            if (id != term.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(term);
            }

            _context.Update(term);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // ili "Index" u Statistics
        }
    }
}
