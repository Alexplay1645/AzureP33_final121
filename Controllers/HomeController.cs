using AzureP33.Data;
using AzureP33.Models;
using AzureP33.Models.Home;
using AzureP33.Models.Orm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace AzureP33.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private static LanguagesResponse? languagesResponse;

        public HomeController(AppDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<IActionResult> IndexAsync(HomeIndexFormModel? formModel)
        {
            Task<LanguagesResponse> respTask = GetLanguagesAsync();

            HomeIndexViewModel viewModel = new()
            {
                PageTitle = "Перекладач",
                FormModel = formModel?.Action == null ? null : formModel
            };

            if (formModel?.Action == "translate")
            {
                string query = $"from={formModel.LangFrom}&to={formModel.LangTo}";
                object[] body = new object[] { new { Text = formModel.OriginalText } };
                string requestBody = JsonSerializer.Serialize(body);

                string result = await RequestApi(query, requestBody, ApiMode.Translate);
                if (result.StartsWith("["))
                {
                    viewModel.Items = JsonSerializer.Deserialize<List<TranslatorResponseItem>>(result);
                }
                else
                {
                    viewModel.ErrorResponse = JsonSerializer.Deserialize<TranslatorErrorResponse>(result);
                }

                if (viewModel.Items != null)
                {
                    var historyItem = new TranslationHistory
                    {
                        CreatedAt = DateTime.UtcNow,
                        UserId = User.Identity?.Name,
                        OriginalText = formModel!.OriginalText,
                        OriginalLanguage = formModel.LangFrom,
                        TranslatedText = viewModel.Items[0].Translations[0].Text,
                        TranslatedLanguage = formModel.LangTo,
                        OriginalTransliteration = viewModel.FromTransliteration?.Text,
                        OriginalScript = viewModel.FromTransliteration?.Script,
                        TranslatedTransliteration = viewModel.ToTransliteration?.Text,
                        TranslatedScript = viewModel.ToTransliteration?.Script
                    };

                    _dbContext.TranslationHistories.Add(historyItem);
                    await _dbContext.SaveChangesAsync();
                }

            }

            viewModel.LanguagesResponse = await respTask;
            return View(viewModel);
        }

        private async Task<LanguagesResponse> GetLanguagesAsync()
        {
            if (languagesResponse == null)
            {
                using HttpClient client = new();
                string json = await client.GetStringAsync(
                    "https://api.cognitive.microsofttranslator.com/languages?api-version=3.0"
                );
                languagesResponse = JsonSerializer.Deserialize<LanguagesResponse>(json)
                    ?? throw new Exception("LanguagesResponse is null");
            }
            return languagesResponse;
        }

        private async Task<string> RequestApi(string query, string body, ApiMode apiMode)
        {
            var sec = _configuration.GetSection("Azure:Translator");
            string key = sec.GetValue<string>("Key")!;
            string endpoint = sec.GetValue<string>("Endpoint")!;
            string location = sec.GetValue<string>("Location")!;
            string apiVersion = sec.GetValue<string>("ApiVersion")!;
            string path = apiMode switch
            {
                ApiMode.Translate => sec.GetValue<string>("TranslatorPath")!,
                ApiMode.Transliterate => sec.GetValue<string>("TransliteratorPath")!,
                _ => throw new Exception("Unknown API mode")
            };

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}{path}?api-version={apiVersion}&{query}");
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", key);
            request.Headers.Add("Ocp-Apim-Subscription-Region", location);

            HttpResponseMessage response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        public IActionResult History()
        {
            var history = _dbContext.TranslationHistories
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            return View(history);
        }
    }

    public enum ApiMode
    {
        Translate,
        Transliterate
    }
}
