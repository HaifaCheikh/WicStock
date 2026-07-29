using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WicStock_.Models
{
    public class Produit
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Reference { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Nom { get; set; } = string.Empty;

        [MaxLength(100)]
        public string TypeTissu { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Categorie { get; set; } = string.Empty;

        [MaxLength(100)]
        public string CycleDeVie { get; set; } = string.Empty;

        public decimal PrixUnitaire { get; set; } = 0;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public string? ImageUrl { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? ImageBase64 { get; set; }

        // Navigation
        public Stock? Stock { get; set; }
        public List<HistoriqueVente> HistoriqueVentes { get; set; } = new();
        public List<HistoriqueProduction> HistoriqueProductions { get; set; } = new();
        public List<Alerte> Alertes { get; set; } = new();
        public List<PrevisionEtatProduit> Previsions { get; set; } = new();

        public int CalculerAncienneteJours()
        {
            return (DateTime.Now - DateCreation).Days;
        }
    }
}