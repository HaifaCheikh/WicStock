using System.Text.Json.Serialization;

namespace WicStock.Web.Models.Dtos
{
    public class QuestionIARequest
    {
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = string.Empty;

        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("utilisateur_id")]
        public int? UtilisateurId { get; set; }
    }

    public class ReponseIA
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("reponse")]
        public string Reponse { get; set; } = string.Empty;

        [JsonPropertyName("sql_genere")]
        public string? SqlGenere { get; set; }

        [JsonPropertyName("resultats")]
        public List<Dictionary<string, object>>? Resultats { get; set; }
    }
}
