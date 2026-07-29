// Controllers/ProduitController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WicStock_.Models;

namespace WicStock_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Toutes les actions nécessitent un token JWT valide
    public class ProduitController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProduitController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/produit (Vue interne)
        [HttpGet]
        [Authorize(Roles = "RESPONSABLE_STOCK_PRODUCTION,ADMIN")]
        public async Task<ActionResult<IEnumerable<Produit>>> GetProduits()
        {
            return await _context.Produits
                .Include(p => p.Stock)
                .ToListAsync();
        }

        // GET: api/produit/catalogue (Catalogue simplifie pour CLIENT)
        [HttpGet("catalogue")]
        [Authorize(Roles = "CLIENT,ADMIN")]
        public async Task<ActionResult<IEnumerable<object>>> GetCatalogueClient()
        {
            var catalogue = await _context.Produits
                .Select(p => new
                {
                    p.Id,
                    p.Reference,
                    p.Nom,
                    p.TypeTissu,
                    p.Categorie,
                    p.PrixUnitaire,
                    p.ImageUrl
                })
                .ToListAsync();

            return Ok(catalogue);
        }

        // GET: api/produit/5
        [HttpGet("{id}")]
        [Authorize(Roles = "RESPONSABLE_STOCK_PRODUCTION,ADMIN")]
        public async Task<ActionResult<Produit>> GetProduit(int id)
        {
            var produit = await _context.Produits
                .Include(p => p.Stock)
                .Include(p => p.Alertes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produit == null)
                return NotFound();

            return produit;
        }

        // POST: api/produit
        [HttpPost]
        [Authorize(Roles = "RESPONSABLE_STOCK_PRODUCTION,ADMIN")]
        public async Task<ActionResult<Produit>> CreerProduit(Produit produit)
        {
            if (!string.IsNullOrEmpty(produit.ImageBase64))
            {
                produit.ImageUrl = SaveUploadedImage(produit.ImageBase64);
            }

            if (produit.Stock == null)
            {
                produit.Stock = new Stock
                {
                    QuantiteActuelle = 0,
                    SeuilAlerte = 10,
                    Emplacement = "Magasin principal",
                    DateMiseAJour = DateTime.Now
                };
            }
            else
            {
                produit.Stock.DateMiseAJour = DateTime.Now;
            }

            _context.Produits.Add(produit);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduit), new { id = produit.Id }, produit);
        }

        // PUT: api/produit/5
        [HttpPut("{id}")]
        [Authorize(Roles = "RESPONSABLE_STOCK_PRODUCTION,ADMIN")]
        public async Task<IActionResult> ModifierProduit(int id, Produit produit)
        {
            if (id != produit.Id)
                return BadRequest();

            if (!string.IsNullOrEmpty(produit.ImageBase64))
            {
                // Supprimer l'ancienne image si elle existe
                if (!string.IsNullOrEmpty(produit.ImageUrl))
                {
                    var pathRelatif = produit.ImageUrl.Replace("/", Path.DirectorySeparatorChar.ToString()).TrimStart(Path.DirectorySeparatorChar);
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pathRelatif);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try { System.IO.File.Delete(oldFilePath); } catch {}
                    }
                }
                produit.ImageUrl = SaveUploadedImage(produit.ImageBase64);
            }

            // Mise à jour des propriétés du produit
            var existingProduit = await _context.Produits
                .Include(p => p.Stock)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduit == null)
                return NotFound();

            existingProduit.Reference = produit.Reference;
            existingProduit.Nom = produit.Nom;
            existingProduit.TypeTissu = produit.TypeTissu;
            existingProduit.Categorie = produit.Categorie;
            existingProduit.CycleDeVie = produit.CycleDeVie;
            existingProduit.PrixUnitaire = produit.PrixUnitaire;
            if (!string.IsNullOrEmpty(produit.ImageUrl))
            {
                existingProduit.ImageUrl = produit.ImageUrl;
            }

            if (produit.Stock != null)
            {
                if (existingProduit.Stock != null)
                {
                    existingProduit.Stock.QuantiteActuelle = produit.Stock.QuantiteActuelle;
                    if (produit.Stock.SeuilAlerte > 0) existingProduit.Stock.SeuilAlerte = produit.Stock.SeuilAlerte;
                    if (!string.IsNullOrEmpty(produit.Stock.Emplacement)) existingProduit.Stock.Emplacement = produit.Stock.Emplacement;
                    existingProduit.Stock.DateMiseAJour = DateTime.Now;
                }
                else
                {
                    existingProduit.Stock = new Stock
                    {
                        ProduitId = id,
                        QuantiteActuelle = produit.Stock.QuantiteActuelle,
                        SeuilAlerte = produit.Stock.SeuilAlerte > 0 ? produit.Stock.SeuilAlerte : 10,
                        Emplacement = !string.IsNullOrEmpty(produit.Stock.Emplacement) ? produit.Stock.Emplacement : "Magasin principal",
                        DateMiseAJour = DateTime.Now
                    };
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Produits.Any(p => p.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        private string? SaveUploadedImage(string? base64Data)
        {
            if (string.IsNullOrEmpty(base64Data))
                return null;

            try
            {
                string extension = ".png";
                string base64Content = base64Data;

                if (base64Data.Contains(","))
                {
                    var parts = base64Data.Split(',');
                    base64Content = parts[1];
                    
                    var mimePart = parts[0];
                    if (mimePart.Contains("image/jpeg"))
                        extension = ".jpg";
                    else if (mimePart.Contains("image/png"))
                        extension = ".png";
                    else if (mimePart.Contains("image/gif"))
                        extension = ".gif";
                    else if (mimePart.Contains("image/webp"))
                        extension = ".webp";
                }

                byte[] imageBytes = Convert.FromBase64String(base64Content);

                string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                string fileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(uploadDir, fileName);

                System.IO.File.WriteAllBytes(filePath, imageBytes);

                return $"/uploads/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IMAGE UPLOAD ERROR] : {ex.Message}");
                return null;
            }
        }

        // DELETE: api/produit/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,RESPONSABLE_STOCK_PRODUCTION")] // Restriction par rôle
        public async Task<IActionResult> SupprimerProduit(int id)
        {
            var produit = await _context.Produits.FindAsync(id);
            if (produit == null)
                return NotFound();

            _context.Produits.Remove(produit);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}