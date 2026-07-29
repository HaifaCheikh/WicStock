using System.Net.Http.Json;
using WicStock.Web.Models.Dtos;

namespace WicStock.Web.Services
{
    public class ProduitService
    {
        private readonly HttpClient _http;

        public ProduitService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ProduitDto>> ObtenirTous()
        {
            return await _http.GetFromJsonAsync<List<ProduitDto>>("api/Produit")
                   ?? new List<ProduitDto>();
        }

        public async Task<ProduitDto?> ObtenirParId(int id)
        {
            return await _http.GetFromJsonAsync<ProduitDto>($"api/Produit/{id}");
        }

        public async Task<bool> Creer(ProduitDto produit)
        {
            var reponse = await _http.PostAsJsonAsync("api/Produit", produit);
            return reponse.IsSuccessStatusCode;
        }

        public async Task<bool> Modifier(int id, ProduitDto produit)
        {
            var reponse = await _http.PutAsJsonAsync($"api/Produit/{id}", produit);
            return reponse.IsSuccessStatusCode;
        }

        public async Task<bool> Supprimer(int id)
        {
            var reponse = await _http.DeleteAsync($"api/Produit/{id}");
            return reponse.IsSuccessStatusCode;
        }
    }
}