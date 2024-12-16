using CommunityHub.API.Data;
using CommunityHub.API.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace CommunityHub.API.Controllers
{
#nullable enable
    /*
     * ControllerBase: The UsersController class inherits from ControllerBase, 
     * which is a base class for controllers that handle API requests in ASP.NETCore. 
     * This provides the controller with a number of useful properties and methods 
     * for handling HTTP requests and responses.
     */
#nullable disable
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(CommunityHubDbContext context) : ControllerBase
    {
        private readonly CommunityHubDbContext _context = context;

        // GET: api/users
        #region GetallUserAndSingleUser
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        } 
        #endregion

        // POST: api/users

        #region CreateUser
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateUser(Models.RegisterRequest registerRequest)
        {
            // Check if the email already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerRequest.Email))
            {
                return BadRequest("A user with this email already exists.");
            }

            // Map RegisterRequest to User model
            var user = new User
            {
                Name = registerRequest.Name,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password), // Hash the password
                Role = "Resident", // Default role; modify if needed
                CreatedDate = DateTime.UtcNow
            };

            // Save the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Return the created user with a 201 status
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        #endregion

        #region Login
        // POST: api/users/auth/login
        //This section for the login
        [AllowAnonymous]
        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] Models.LoginRequest loginRequest)
        {
            try
            {
                Console.WriteLine($"Login attempt for email: {loginRequest.Email}");

                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                {
                    Console.WriteLine("User not found.");
                    return Unauthorized("Invalid email or password.");
                }

                var token = GenerateJwtToken(user);

                HttpContext.Session.SetString("AuthToken", token);
                HttpContext.Session.SetString("UserName", user.Name);

                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        #endregion

        // Helper method to generate JWT token
        #region Tokens
        private string GenerateJwtToken(User user)
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT_SECRET_KEY is not set in the environment variables.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Include the correct claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Include numeric user ID
            };

            // Log the claims for debugging
            Console.WriteLine("Claims in Token:");
            foreach (var claim in claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

            var token = new JwtSecurityToken(
                issuer: "https://localhost:7175",
                audience: "https://localhost:7175",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion

        #region DeleteAndUpdateUser
        //------------------------------
        //
        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User userUpdate)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Name = userUpdate.Name;
            user.Email = userUpdate.Email;
            user.Role = userUpdate.Role;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

             return NoContent();
        }
        #endregion

        #region profileSection
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Filter claims for the correct NameIdentifier value
            var userIdClaim = User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _))
                .Select(c => c.Value)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                Console.WriteLine($"Invalid or missing user ID claim: {userIdClaim}");
                return Unauthorized("Invalid or missing user ID in token.");
            }

            // Find the user by ID in the database
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                user.Name,
                user.Email,
                user.Role
            });
        }


        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest updateRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Name = updateRequest.Name;
            user.Email = updateRequest.Email;

            if (!string.IsNullOrEmpty(updateRequest.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateRequest.Password);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DTO for update profile
        public class UpdateProfileRequest
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string? Password { get; set; }
        }
        #endregion

    }
}
