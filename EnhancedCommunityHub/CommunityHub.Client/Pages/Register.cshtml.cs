using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace CommunityHub.Client.Pages
{
    public class RegisterModel(IHttpClientFactory clientFactory) : PageModel
    {
        private readonly HttpClient _httpClient = clientFactory.CreateClient("CommunityHubAPI");

        [BindProperty]
        public RegisterRequest Register { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users", Register);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Registration successful! Please log in.";
                    ErrorMessage = "";
                    return RedirectToPage("/Login");
                }
                else
                {
                    ErrorMessage = "Registration failed. Please try again.";
                    SuccessMessage = "";
                    return Page();
                }
            }
            catch
            {
                ErrorMessage = "An error occurred during registration.";
                SuccessMessage = "";
                return Page();
            }
        }

        public class RegisterRequest
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

    }
}
