namespace WicStock_.Models
{
    public class Enums
    {
        public enum TypeMouvement
        {
            ENTREE,
            SORTIE,
            RETOUR,
            AJUSTEMENT
        }

        public enum TypeRisque
        {
            SURSTOCK,
            OBSOLESCENCE,
            RUPTURE
        }

        public enum StatutAlerte
        {
            NON_TRAITEE,
            EN_COURS,
            TRAITEE
        }

        public enum RoleUtilisateur
        {
            ADMIN,
            RESPONSABLE_STOCK_PRODUCTION,
            CLIENT
        }

        public enum TypeAction
        {
            PROMOTION_CIBLEE,
            REDISTRIBUTION,
            RECYCLAGE_ANTICIPE,
            AUCUNE_ACTION
        }
    }
}
