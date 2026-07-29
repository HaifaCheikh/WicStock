using Microsoft.EntityFrameworkCore;
using WicStock_.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produit> Produits { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<MouvementStock> MouvementsStock { get; set; }
    public DbSet<HistoriqueVente> HistoriqueVentes { get; set; }
    public DbSet<HistoriqueProduction> HistoriqueProductions { get; set; }
    public DbSet<Alerte> Alertes { get; set; }
    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<PrevisionEtatProduit> PrevisionsEtatProduit { get; set; }
    public DbSet<ActionRecommandee> ActionsRecommandees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Produit <-> Stock : relation 1-1 stricte
        modelBuilder.Entity<Produit>()
            .HasOne(p => p.Stock)
            .WithOne(s => s.Produit)
            .HasForeignKey<Stock>(s => s.ProduitId);

        // PrevisionEtatProduit <-> ActionRecommandee : relation 1 - 0..1
        modelBuilder.Entity<PrevisionEtatProduit>()
            .HasOne(p => p.ActionRecommandee)
            .WithOne(a => a.PrevisionEtatProduit)
            .HasForeignKey<ActionRecommandee>(a => a.PrevisionEtatProduitId);

        // Stocker les enums en texte plutôt qu'en nombre (plus lisible en base)
        modelBuilder.Entity<MouvementStock>()
            .Property(m => m.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Alerte>()
            .Property(a => a.TypeRisque)
            .HasConversion<string>();

        modelBuilder.Entity<Alerte>()
            .Property(a => a.Statut)
            .HasConversion<string>();

        modelBuilder.Entity<Utilisateur>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<PrevisionEtatProduit>()
            .Property(p => p.TypeRisquePredit)
            .HasConversion<string>();

        modelBuilder.Entity<ActionRecommandee>()
            .Property(a => a.TypeAction)
            .HasConversion<string>();

        // Éviter les suppressions en cascade multiples (SQL Server les refuse par défaut)
        modelBuilder.Entity<Alerte>()
            .HasOne(a => a.Utilisateur)
            .WithMany(u => u.AlertesTraitees)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ActionRecommandee>()
            .HasOne(a => a.Utilisateur)
            .WithMany(u => u.ActionsValidees)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<HistoriqueVente>()
            .HasOne(h => h.Utilisateur)
            .WithMany()
            .HasForeignKey(h => h.UtilisateurId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}