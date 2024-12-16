using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CommunityHub.Client.Pages
{
#nullable disable
    public class ProfileModel : PageModel
    {
        private readonly HttpClient _httpClient;
        public ProfileModel(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("CommunityHubAPI");
        }

        [BindProperty]
        public ProfileDto Profile { get; set; } = new ProfileDto();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    Response.Redirect("/Login");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/users/profile");

                if (response.IsSuccessStatusCode)
                {
                    Profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
                }
                else
                {
                    ErrorMessage = "Failed to load profile information.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    Response.Redirect("/Login");
                    return Page();
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PutAsJsonAsync("api/users/profile", Profile);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Profile updated successfully.";
                }
                else
                {
                    ErrorMessage = "Failed to update profile.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return Page();
        }
    }

    public class ProfileDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }
    }
}
