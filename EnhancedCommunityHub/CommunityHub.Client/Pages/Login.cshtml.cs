//using Microsoft.AspNetCore.Identity.Data;
using CommunityHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Text.Json;

namespace CommunityHub.Client.Pages
{
#nullable disable
    public class LoginModel(IHttpClientFactory clientFactory) : PageModel
    {
        private readonly HttpClient _httpClient = clientFactory.CreateClient("CommunityHubAPI");

        [BindProperty]
        public LoginRequest LoginRequest { get; set; } = new LoginRequest();

        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsLoading { get; set; } = false;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users/auth/login", LoginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
                    {
                        // Store the JWT token securely
                        HttpContext.Session.SetString("AuthToken", tokenResponse.Token);

                        return RedirectToPage("/Dashboard");
                    }
                    else
                    {
                        ErrorMessage = "Failed to retrieve authentication token.";
                        return Page();
                    }
                }
                else
                {
                    ErrorMessage = "Invalid login credentials.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
            finally
            {
                IsLoading = false; // Reset loading state
            }
        }

        public class TokenResponse
        {
            public string Token { get; set; }
        }
        
    }
}
