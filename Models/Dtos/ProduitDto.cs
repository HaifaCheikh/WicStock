using System;
using System.Collections.Generic;
using System.Linq;

namespace WicStock.Web.Models.Dtos
{
    public class PrevisionEtatProduitDto
    {
        public int Id { get; set; }
        public int ProduitId { get; set; }
        public string TypeRisquePredit { get; set; } = string.Empty;
        public float ScoreRisque { get; set; }
        public int QuantitePredite { get; set; }
        public int HorizonJours { get; set; }
        public DateTime DateCalcul { get; set; }
        public ActionRecommandeeDto? ActionRecommandee { get; set; }
    }

    public class ActionRecommandeeDto
    {
        public int Id { get; set; }
        public string TypeAction { get; set; } = string.Empty;
        public string? TexteGenere { get; set; }
        public DateTime DateGeneration { get; set; }
    }

    public class ProduitDto
    {
        public int Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string TypeTissu { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
        public string CycleDeVie { get; set; } = string.Empty;
        public decimal PrixUnitaire { get; set; } = 0;
        public DateTime DateCreation { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageBase64 { get; set; }

        public StockDto? Stock { get; set; } = new StockDto();
        public List<PrevisionEtatProduitDto> Previsions { get; set; } = new();

        public PrevisionEtatProduitDto? DernierePrevision => Previsions?.OrderByDescending(p => p.DateCalcul).FirstOrDefault();

        public string StatutIAType
        {
            get
            {
                int qte = Stock?.QuantiteActuelle ?? 0;
                int seuil = (Stock?.SeuilAlerte ?? 0) > 0 ? Stock!.SeuilAlerte : 10;
                string? prevRisque = DernierePrevision?.TypeRisquePredit?.ToUpperInvariant();

                if (qte == 0 || qte < seuil || prevRisque == "RUPTURE")
                    return "RUPTURE";
                if (prevRisque == "SURSTOCK" || qte >= 100)
                    return "SURSTOCK";
                if (prevRisque == "OBSOLESCENCE")
                    return "OBSOLESCENCE";

                return "OPTIMAL";
            }
        }

        public int StatutIAPriorite => StatutIAType switch
        {
            "RUPTURE" => 1,
            "SURSTOCK" => 2,
            "OBSOLESCENCE" => 3,
            _ => 4
        };

        public string StatutIABadgeText
        {
            get
            {
                if (StatutIAType == "RUPTURE")
                    return "🔴 Rupture";

                if (StatutIAType == "SURSTOCK")
                    return "🟠 Surstock";

                if (StatutIAType == "OBSOLESCENCE")
                    return "⚠️ Obsolète";

                return "🟢 Optimal";
            }
        }

        public string? StatutIAActionTexte
        {
            get
            {
                // Uniquement si le type de risque est Surstock ou Obsolescence
                if (StatutIAType == "SURSTOCK" || StatutIAType == "OBSOLESCENCE")
                {
                    // Uniquement s'il y a un texte généré par l'IA en base de données
                    if (!string.IsNullOrWhiteSpace(DernierePrevision?.ActionRecommandee?.TexteGenere))
                    {
                        return DernierePrevision.ActionRecommandee.TexteGenere;
                    }
                }
                
                // Sinon on ne retourne rien (pas de fausse recommandation)
                return null;
            }
        }
    }

    public class StockDto
    {
        public int Id { get; set; }
        public int QuantiteActuelle { get; set; }
        public int SeuilAlerte { get; set; } = 10;
        public string Emplacement { get; set; } = "Magasin principal";
    }
}