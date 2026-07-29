using System.Net.Http.Json;
using WicStock.Web.Models.Dtos;

namespace WicStock.Web.Services
{
    public class CatalogueService
    {
        private readonly HttpClient _http;

        public CatalogueService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CatalogueProduitDto>> ObtenirCatalogue()
        {
            return await _http.GetFromJsonAsync<List<CatalogueProduitDto>>("api/Produit/catalogue")
                   ?? new List<CatalogueProduitDto>();
        }

        public async Task<List<MaCommandeDto>> ObtenirMesCommandes()
        {
            return await _http.GetFromJsonAsync<List<MaCommandeDto>>("api/HistoriqueVente/mes-commandes")
                   ?? new List<MaCommandeDto>();
        }

        public async Task<List<CommandeManagerDto>> ObtenirToutesLesCommandes()
        {
            return await _http.GetFromJsonAsync<List<CommandeManagerDto>>("api/HistoriqueVente")
                   ?? new List<CommandeManagerDto>();
        }

        public async Task<bool> PasserCommande(CommandeCreateDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/HistoriqueVente", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AnnulerCommande(int commandeId)
        {
            var response = await _http.DeleteAsync($"api/HistoriqueVente/annuler/{commandeId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AccepterCommande(int commandeId)
        {
            var response = await _http.PutAsync($"api/HistoriqueVente/{commandeId}/accepter", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RefuserCommande(int commandeId)
        {
            var response = await _http.PutAsync($"api/HistoriqueVente/{commandeId}/refuser", null);
            return response.IsSuccessStatusCode;
        }
    }
}
