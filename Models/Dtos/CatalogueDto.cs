namespace WicStock.Web.Models.Dtos
{
    public class CatalogueProduitDto
    {
        public int Id { get; set; }
        public string Reference { get; set; } = "";
        public string Nom { get; set; } = "";
        public string? TypeTissu { get; set; }
        public string? Categorie { get; set; }
        public decimal PrixUnitaire { get; set; } = 0;
        public string? ImageUrl { get; set; }
    }

    public class CommandeCreateDto
    {
        public int ProduitId { get; set; }
        public int QuantiteVendue { get; set; }
        public decimal PrixUnitaire { get; set; }
    }

    public class MaCommandeDto
    {
        public int Id { get; set; }
        public DateTime DateVente { get; set; }
        public int QuantiteVendue { get; set; }
        public decimal PrixUnitaire { get; set; }
        public string StatutCommande { get; set; } = "ACCEPTEE";
        public int ProduitId { get; set; }
        public string? ProduitNom { get; set; }
        public string? ProduitReference { get; set; }
        public string? ProduitCategorie { get; set; }
        public string? ProduitImageUrl { get; set; }
        public decimal TotalCommande { get; set; }
    }

    public class CommandeManagerDto
    {
        public int Id { get; set; }
        public DateTime DateVente { get; set; }
        public int QuantiteVendue { get; set; }
        public decimal PrixUnitaire { get; set; }
        public string StatutCommande { get; set; } = "ACCEPTEE";
        public int ProduitId { get; set; }
        public string? ProduitNom { get; set; }
        public string? ProduitReference { get; set; }
        public int? UtilisateurId { get; set; }
        public string? ClientNom { get; set; }
        public string? ClientEmail { get; set; }
        public decimal TotalCommande => QuantiteVendue * PrixUnitaire;
    }
}
