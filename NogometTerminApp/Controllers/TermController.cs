using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NogometTerminApp.Data;
using NogometTerminApp.Models;

namespace NogometTerminApp.Controllers
{
    public class TermController : Controller
    {
        private readonly AppDbContext _context;

        public TermController(AppDbContext context)
        {
            _context = context;
        }

        // za sada jedan fiksni termin s Id = 1
        public async Task<IActionResult> Index()
        {
            var term = await _context.Terms
                .Include(t => t.Registrations)
                .ThenInclude(r => r.Player)
                .FirstOrDefaultAsync(t => t.Id == 1);

            if (term == null)
            {
                // ako ga nema, kreiramo ga prvi put
                term = new Term
                {
                    TermDateTime = DateTime.Today.AddHours(20),
                    Location = "Zagreb - dvorana",
                    MaxPlayers = 14
                };
                _context.Terms.Add(term);
                await _context.SaveChangesAsync();
            }

            var players = term.Registrations?
                .OrderBy(r => r.RegisteredAt)
                .Select(r => r.Player.Name)
                .ToList() ?? new List<string>();

            var vm = new TermRegisterViewModel
            {
                TermId = term.Id,
                CurrentCount = players.Count,
                MaxPlayers = term.MaxPlayers,
                PlayerNames = players
            };

            return View(vm);
        }

        [HttpPost]
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
                model.CurrentCount = currentCount;
                model.MaxPlayers = term.MaxPlayers;
                model.PlayerNames = term.Registrations
                    .OrderBy(r => r.RegisteredAt)
                    .Select(r => r.Player.Name)
                    .ToList();

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
    }
}
