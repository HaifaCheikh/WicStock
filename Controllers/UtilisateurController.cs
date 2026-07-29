using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WicStock_.Models;
using static WicStock_.Models.Enums;

namespace WicStock_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class UtilisateurController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtilisateurController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/utilisateur
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUtilisateurs()
        {
            var users = await _context.Utilisateurs.ToListAsync();
            var result = users.Select(u => new
            {
                u.Id,
                u.Nom,
                u.Prenom,
                u.Email,
                u.Telephone,
                Role = u.Role.ToString()
            });

            return Ok(result);
        }

        // GET: api/utilisateur/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUtilisateur(int id)
        {
            var u = await _context.Utilisateurs.FindAsync(id);
            if (u == null)
                return NotFound();

            return Ok(new
            {
                u.Id,
                u.Nom,
                u.Prenom,
                u.Email,
                u.Telephone,
                Role = u.Role.ToString()
            });
        }

        public class ChangerRoleRequest
        {
            public string NouveauRole { get; set; } = string.Empty;
        }

        // PUT: api/utilisateur/5/role
        [HttpPut("{id}/role")]
        public async Task<IActionResult> ModifierRole(int id, [FromBody] ChangerRoleRequest req)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null)
                return NotFound("Utilisateur introuvable.");

            if (Enum.TryParse<Enums.RoleUtilisateur>(req.NouveauRole, out var roleEnum))
            {
                utilisateur.Role = roleEnum;
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Rôle mis à jour avec succès." });
            }

            return BadRequest("Rôle invalide.");
        }

        // DELETE: api/utilisateur/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> SupprimerUtilisateur(int id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null)
                return NotFound();

            _context.Utilisateurs.Remove(utilisateur);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}