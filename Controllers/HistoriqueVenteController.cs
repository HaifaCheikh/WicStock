using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WicStock_.Models;

namespace WicStock_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HistoriqueVenteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HistoriqueVenteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/historiquevente (Vue globale pour Responsable Stock & Production)
        [HttpGet]
        [Authorize(Roles = "RESPONSABLE_STOCK_PRODUCTION")]
        public async Task<ActionResult<IEnumerable<object>>> GetHistoriqueVentes()
        {
            var ventes = await _context.HistoriqueVentes
                .Include(h => h.Produit)
                .Include(h => h.Utilisateur)
                .OrderByDescending(h => h.DateVente)
                .ToListAsync();

            var result = ventes.Select(h => new
            {
                h.Id,
                h.DateVente,
                h.QuantiteVendue,
                h.PrixUnitaire,
                h.StatutCommande,
                h.ProduitId,
                ProduitNom = h.Produit?.Nom,
                ProduitReference = h.Produit?.Reference,
                h.UtilisateurId,
                ClientNom = h.Utilisateur != null ? $"{h.Utilisateur.Prenom} {h.Utilisateur.Nom}" : "Client anonyme",
                ClientEmail = h.Utilisateur?.Email
            });

            return Ok(result);
        }

        // GET: api/historiquevente/mes-commandes (Historique du CLIENT connecté)
        [HttpGet("mes-commandes")]
        [Authorize(Roles = "CLIENT,ADMIN")]
        public async Task<ActionResult<IEnumerable<object>>> GetMesCommandes()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var commandes = await _context.HistoriqueVentes
                .Include(h => h.Produit)
                .Where(h => h.UtilisateurId == userId)
                .OrderByDescending(h => h.DateVente)
                .ToListAsync();

            var result = commandes.Select(h => new
            {
                h.Id,
                h.DateVente,
                h.QuantiteVendue,
                h.PrixUnitaire,
                h.StatutCommande,
                h.ProduitId,
                ProduitNom = h.Produit?.Nom,
                ProduitReference = h.Produit?.Reference,
                ProduitCategorie = h.Produit?.Categorie,
                ProduitImageUrl = h.Produit?.ImageUrl,
                TotalCommande = h.QuantiteVendue * h.PrixUnitaire
            });

            return Ok(result);
        }

        // GET: api/historiquevente/produit/3
        [HttpGet("produit/{produitId}")]
        [Authorize(Roles = "RESPONSABLE_STOCK_PRODUCTION,ADMIN")]
        public async Task<ActionResult<IEnumerable<HistoriqueVente>>> GetParProduit(int produitId)
        {
            return await _context.HistoriqueVentes
                .Where(h => h.ProduitId == produitId)
                .OrderByDescending(h => h.DateVente)
                .ToListAsync();
        }

        // POST: api/historiquevente (Passer une commande client)
        [HttpPost]
        [Authorize(Roles = "CLIENT,ADMIN")]
        public async Task<ActionResult<object>> CreerVente(CommandeDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? utilisateurId = int.TryParse(userIdClaim, out int uid) ? uid : null;

            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProduitId == dto.ProduitId);

            string statut = "EN_ATTENTE";

            // Si la quantité en stock est suffisante, la commande passe directement ACCEPTEE
            if (stock != null && stock.QuantiteActuelle >= dto.QuantiteVendue)
            {
                statut = "ACCEPTEE";
                stock.QuantiteActuelle -= dto.QuantiteVendue;
                stock.DateMiseAJour = DateTime.Now;
            }

            var vente = new HistoriqueVente
            {
                ProduitId = dto.ProduitId,
                QuantiteVendue = dto.QuantiteVendue,
                PrixUnitaire = dto.PrixUnitaire,
                StatutCommande = statut,
                DateVente = DateTime.Now,
                UtilisateurId = utilisateurId
            };

            _context.HistoriqueVentes.Add(vente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMesCommandes), new { }, new
            {
                vente.Id,
                vente.DateVente,
                vente.QuantiteVendue,
                vente.PrixUnitaire,
                vente.StatutCommande,
                vente.ProduitId,
                vente.UtilisateurId
            });
        }

        // PUT: api/historiquevente/5/accepter (Manager accepte la commande)
        [HttpPut("{id}/accepter")]
        [Authorize(Roles = "RESPONSABLE_STOCK_PRODUCTION")]
        public async Task<IActionResult> AccepterCommande(int id)
        {
            var vente = await _context.HistoriqueVentes
                .Include(h => h.Produit)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (vente == null)
                return NotFound("Commande introuvable.");

            if (vente.StatutCommande == "ACCEPTEE")
                return BadRequest("La commande est déjà acceptée.");

            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProduitId == vente.ProduitId);
            if (stock != null)
            {
                stock.QuantiteActuelle = Math.Max(0, stock.QuantiteActuelle - vente.QuantiteVendue);
                stock.DateMiseAJour = DateTime.Now;
            }

            vente.StatutCommande = "ACCEPTEE";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Commande acceptée avec succès.", statut = vente.StatutCommande });
        }

        // PUT: api/historiquevente/5/refuser (Manager refuse la commande)
        [HttpPut("{id}/refuser")]
        [Authorize(Roles = "RESPONSABLE_STOCK_PRODUCTION")]
        public async Task<IActionResult> RefuserCommande(int id)
        {
            var vente = await _context.HistoriqueVentes
                .Include(h => h.Produit)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (vente == null)
                return NotFound("Commande introuvable.");

            if (vente.StatutCommande == "REFUSEE")
                return BadRequest("La commande est déjà refusée.");

            // Si la commande était acceptée précédemment, restaurer le stock
            if (vente.StatutCommande == "ACCEPTEE")
            {
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProduitId == vente.ProduitId);
                if (stock != null)
                {
                    stock.QuantiteActuelle += vente.QuantiteVendue;
                    stock.DateMiseAJour = DateTime.Now;
                }
            }

            // Supprimer la commande de la base de données au refus
            _context.HistoriqueVentes.Remove(vente);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Commande refusée et supprimée.", statut = "REFUSEE" });
        }

        // POST: api/historiquevente/import
        [HttpPost("import")]
        [Authorize(Roles = "RESPONSABLE_STOCK_PRODUCTION,ADMIN")]
        public async Task<ActionResult> ImporterVentes(List<HistoriqueVente> ventes)
        {
            _context.HistoriqueVentes.AddRange(ventes);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"{ventes.Count} ventes importées." });
        }

        // DELETE: api/historiquevente/annuler/5 (Annulation d'une commande par un CLIENT)
        [HttpDelete("annuler/{id}")]
        [Authorize(Roles = "CLIENT,ADMIN")]
        public async Task<IActionResult> AnnulerCommande(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var vente = await _context.HistoriqueVentes.FindAsync(id);
            if (vente == null)
                return NotFound("Commande introuvable.");

            if (vente.UtilisateurId != userId && !User.IsInRole("ADMIN"))
                return Forbid();

            // Si la commande était acceptée, remettre la quantité en stock
            if (vente.StatutCommande == "ACCEPTEE")
            {
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProduitId == vente.ProduitId);
                if (stock != null)
                {
                    stock.QuantiteActuelle += vente.QuantiteVendue;
                    stock.DateMiseAJour = DateTime.Now;
                }
            }

            _context.HistoriqueVentes.Remove(vente);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Commande annulée avec succès." });
        }

        // DELETE: api/historiquevente/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> SupprimerVente(int id)
        {
            var vente = await _context.HistoriqueVentes.FindAsync(id);
            if (vente == null)
                return NotFound();

            _context.HistoriqueVentes.Remove(vente);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class CommandeDto
    {
        public int ProduitId { get; set; }
        public int QuantiteVendue { get; set; }
        public decimal PrixUnitaire { get; set; }
    }
}