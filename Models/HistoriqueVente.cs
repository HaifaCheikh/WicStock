namespace WicStock_.Models
{
    public class HistoriqueVente
    {
        public int Id { get; set; }

        public DateTime DateVente { get; set; }
        public int QuantiteVendue { get; set; }
        public decimal PrixUnitaire { get; set; }

        public string StatutCommande { get; set; } = "ACCEPTEE"; // ACCEPTEE, EN_ATTENTE, REFUSEE

        public int ProduitId { get; set; }
        public Produit? Produit { get; set; }

        // Lien vers le client qui a passé la commande
        public int? UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }
    }
}
