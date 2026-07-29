using System.Net.Http.Json;
using WicStock.Web.Models.Dtos;

namespace WicStock.Web.Services
{
    public class UtilisateurService
    {
        private readonly HttpClient _http;

        public UtilisateurService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<UtilisateurDto>> ObtenirTous()
        {
            return await _http.GetFromJsonAsync<List<UtilisateurDto>>("api/Utilisateur")
                   ?? new List<UtilisateurDto>();
        }

        public async Task<bool> ModifierRole(int id, string nouveauRole)
        {
            var reponse = await _http.PutAsJsonAsync($"api/Utilisateur/{id}/role", new ChangerRoleDto { NouveauRole = nouveauRole });
            return reponse.IsSuccessStatusCode;
        }

        public async Task<bool> Supprimer(int id)
        {
            var reponse = await _http.DeleteAsync($"api/Utilisateur/{id}");
            return reponse.IsSuccessStatusCode;
        }
    }
}
