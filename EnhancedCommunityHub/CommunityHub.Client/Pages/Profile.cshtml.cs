using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace CommunityHub.Client.Pages
{
    public class ProfileModel(IHttpClientFactory clientFactory) : PageModel
    {
        private readonly HttpClient _httpClient = clientFactory.CreateClient("CommunityHubAPI");

        [BindProperty]
        public UserProfile Profile { get; set; } = new UserProfile();

        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            try
            {
                // Fetch user profile (replace with your user ID or session logic)
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId != null)
                {
                    Profile = await _httpClient.GetFromJsonAsync<UserProfile>($"api/users/{userId}");
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
                // Update user profile (replace with your user ID or session logic)
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId != null)
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}", Profile);

                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "Profile updated successfully!";
                        return Page();
                    }
                }
                ErrorMessage = "Failed to update profile.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }

        public class UserProfile
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; } // Leave empty if not updating
        }
    }
}
