using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CommunityHub.Client.Pages
{
#nullable disable
    public class DashboardModel : PageModel
    {
        private readonly HttpClient _httpClient;
        public string UserName { get; set; } = "Guest";
        public string ErrorMessage { get; set; }
        public List<Post> Posts { get; set; } = new List<Post>();

        public DashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("CommunityHubAPI");
        }

        public async Task OnGetAsync()
        {
            UserName = HttpContext.Session.GetString(UserName) ?? "Guest";
            try
            {
                // Retrieve the JWT token from session
                string token = HttpContext.Session.GetString("AuthToken");

                if (string.IsNullOrEmpty(token))
                {
                    ErrorMessage = "You need to log in.";
                    Response.Redirect("/Login");
                    return;
                }

                // Set Authorization header with Bearer token
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var res = await _httpClient.GetAsync("api/dashboard");
                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Clear session and redirect to login if token is invalid
                    HttpContext.Session.Clear();
                    Response.Redirect("/Login");
                    return;
                }


                // Make an authenticated API request
                var response = await _httpClient.GetAsync("api/posts"); // Replace with your API route


                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the JSON content into a list of posts
                    Posts = await response.Content.ReadFromJsonAsync<List<Post>>();
                }
                else
                {
                    ErrorMessage = "Error fetching posts.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }
    }

    public class Post
    {
        public string Title { get; set; }
        public string Content { get; set; }
        
        public string Category { get; set; }
    }

}
