using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NogometTerminApp.Data;
using NogometTerminApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NogometTerminApp.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public AdminApiController(
            UserManager<ApplicationUser> userManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // ====== DTO KLASE ======

        public class ChangePasswordDto
        {
            public string NewPassword { get; set; }
        }

        public class CreateTermDto
        {
            public System.DateTime TermDateTime { get; set; }
            public string Location { get; set; }
            public int MaxPlayers { get; set; }
        }

        public class UpdateResultDto
        {
            public string Result { get; set; }
        }

        // ====== KORISNICI I ROLE ======

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.FirstName
                })
                .ToListAsync();

            return Ok(users);
        }

        // POST: api/admin/users/{id}/toggle-admin
        [HttpPost("users/{id}/toggle-admin")]
        public async Task<IActionResult> ToggleAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                return Ok(new { message = "Maknuta admin rola." });
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return Ok(new { message = "Dodana admin rola." });
            }
        }

        // DELETE: api/admin/users/{id}
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        // POST: api/admin/users/{id}/password
        // body: { "newPassword": "Lozinka123!" }
        [HttpPost("users/{id}/password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest("Nova lozinka je obavezna.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Lozinka promijenjena." });
        }

        // ====== TERMINI ======

        // GET: api/admin/terms
        [HttpGet("terms")]
        public async Task<IActionResult> GetTerms()
        {
            var terms = await _context.Terms
                .OrderByDescending(t => t.TermDateTime)
                .Select(t => new
                {
                    t.Id,
                    t.TermDateTime,
                    t.Location,
                    t.MaxPlayers,
                    t.IsPostponed,
                    t.Result
                })
                .ToListAsync();

            return Ok(terms);
        }

        // POST: api/admin/terms
        // body: { "termDateTime":"2026-02-16T20:00:00", "location":"Teren", "maxPlayers":14 }
        [HttpPost("terms")]
        public async Task<IActionResult> CreateTerm([FromBody] CreateTermDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Location))
                return BadRequest("Nedostaju podaci za termin.");

            var term = new Term
            {
                TermDateTime = dto.TermDateTime,
                Location = dto.Location,
                MaxPlayers = dto.MaxPlayers,
                IsPostponed = false
            };

            _context.Terms.Add(term);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTerms), new { id = term.Id }, new
            {
                term.Id,
                term.TermDateTime,
                term.Location,
                term.MaxPlayers,
                term.IsPostponed,
                term.Result
            });
        }

        // DELETE: api/admin/terms/{id}
        [HttpDelete("terms/{id}")]
        public async Task<IActionResult> DeleteTerm(int id)
        {
            var term = await _context.Terms
                .Include(t => t.Registrations)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (term == null) return NotFound();

            _context.TermRegistrations.RemoveRange(term.Registrations);
            _context.Terms.Remove(term);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/admin/terms/{id}/result
        // body: { "result":"5:3" }
        [HttpPut("terms/{id}/result")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] UpdateResultDto dto)
        {
            if (dto == null) return BadRequest("Rezultat je obavezan.");

            var term = await _context.Terms.FindAsync(id);
            if (term == null) return NotFound();

            term.Result = dto.Result;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rezultat ažuriran." });
        }
    }
}
