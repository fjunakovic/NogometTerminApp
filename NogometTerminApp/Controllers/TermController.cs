using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NogometTerminApp.Data;
using NogometTerminApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NogometTerminApp.Controllers
{
    public class TermController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TermController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<TermRegisterViewModel> BuildTermViewModel(Term term)
        {
            var registrations = term.Registrations?
                .OrderBy(r => r.RegisteredAt)
                .Select(r => new TermRegistrationInfo
                {
                    RegistrationId = r.Id,
                    PlayerName = r.Player.Name,
                    Team = r.Team
                })
                .ToList() ?? new List<TermRegistrationInfo>();

            var vm = new TermRegisterViewModel
            {
                TermId = term.Id,
                CurrentCount = registrations.Count,
                MaxPlayers = term.MaxPlayers,
                Registrations = registrations
            };

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var player = await _context.Players
                        .FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (player != null)
                    {
                        vm.IsCurrentUserRegistered = term.Registrations
                            .Any(r => r.PlayerId == player.Id);
                    }
                }
            }

            return vm;
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

            var vm = await BuildTermViewModel(term);
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
        [Authorize]
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

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Moraš biti prijavljen da bi se prijavio na termin.");
            }

            if (!ModelState.IsValid)
            {
                var vm = await BuildTermViewModel(term);
                return View("Index", vm);
            }

            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (player == null)
            {
                player = new Player
                {
                    Name = user.FirstName,
                    UserId = user.Id
                };
                _context.Players.Add(player);
                await _context.SaveChangesAsync();
            }

            bool alreadyRegistered = term.Registrations.Any(r => r.PlayerId == player.Id);
            if (!alreadyRegistered)
            {
                var registration = new TermRegistration
                {
                    TermId = term.Id,
                    PlayerId = player.Id,
                    RegisteredAt = DateTime.Now
                };

                _context.TermRegistrations.Add(registration);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Unregister(TermRegisterViewModel model)
        {
            var term = await _context.Terms
                .Include(t => t.Registrations)
                .FirstOrDefaultAsync(t => t.Id == model.TermId);

            if (term == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var player = await _context.Players.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (player == null)
                return RedirectToAction("Index");

            var registration = await _context.TermRegistrations
                .FirstOrDefaultAsync(r => r.TermId == term.Id && r.PlayerId == player.Id);

            if (registration != null)
            {
                _context.TermRegistrations.Remove(registration);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
            var now = DateTime.Now;
            int todayDow = (int)now.DayOfWeek;
            int targetDow = (int)DayOfWeek.Saturday;

            int daysUntilSaturday = targetDow - todayDow;
            if (daysUntilSaturday < 0)
            {
                daysUntilSaturday += 7;
            }

            var saturday = now.Date.AddDays(daysUntilSaturday);
            var thisSaturday20 = saturday.AddHours(20);

            if (now > thisSaturday20)
            {
                thisSaturday20 = thisSaturday20.AddDays(7);
            }

            return thisSaturday20;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeTeam(int registrationId, string team)
        {
            var registration = await _context.TermRegistrations
                .FirstOrDefaultAsync(r => r.Id == registrationId);

            if (registration == null)
            {
                return NotFound();
            }

            if (team != "Bijeli" && team != "Tamni" && !string.IsNullOrEmpty(team))
            {
                return BadRequest();
            }

            registration.Team = string.IsNullOrEmpty(team) ? null : team;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
