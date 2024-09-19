using MathEquationWebApp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace MathEquationWebApp.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Math operation is required.")]
        public string Operation { get; set; }

        private readonly ILogger<IndexModel> _logger;
        private readonly IOptions<CalculateEquationApi> _calculateEquationApi;

        public string? result = null;
        public string? validationMessage = null;
        public string? errorMessage = null;

        public IndexModel(ILogger<IndexModel> logger, IOptions<CalculateEquationApi> calculateEquationApi)
        {
            _logger = logger;
            _calculateEquationApi = calculateEquationApi;
        }

        public async Task<IActionResult> OnPost(string operation)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    HttpClient client = new HttpClient();

                    // Call API
                    string apiURL = _calculateEquationApi.Value.apiUrl + "Calculation/CalculateMathOperation";
                    string apiKey = _calculateEquationApi.Value.apiKey;

                    client.BaseAddress = new Uri(apiURL);
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                    var body = new StringContent(JsonSerializer.Serialize(operation), Encoding.UTF8, "application/json");
                    _logger.LogInformation($"Start Call to CalculateEquation API | Request: {operation}");
                    var response = await client.PostAsync(apiURL, body);
                    _logger.LogInformation($"End Call to CalculateEquation API | Response : {(int)response.StatusCode} {response.StatusCode} - {response.Content.ReadAsStringAsync().Result}");

                    // API Reponse handling
                    if (response.IsSuccessStatusCode)
                    {
                        result = response.Content.ReadAsStringAsync().Result;
                        ModelState.Clear();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        validationMessage = $"{response.Content.ReadAsStringAsync().Result}";
                    }
                    else
                    {
                        errorMessage = $"{response.Content.ReadAsStringAsync().Result}";
                    }
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected Exception: {ex.Message}");
                errorMessage = $"{ex.Message}";
                return Page();
            }
        }
    }
}
