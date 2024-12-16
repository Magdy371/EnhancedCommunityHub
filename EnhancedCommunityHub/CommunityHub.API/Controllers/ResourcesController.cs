using CommunityHub.API.Data;
using CommunityHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunityHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController(CommunityHubDbContext context) : ControllerBase
    {
        private readonly CommunityHubDbContext _context = context;

        // GET: api/resources
        [HttpGet]
        public async Task<IActionResult> GetResources()
        {
            var resources = await _context.Resources.ToListAsync();
            return Ok(resources);
        }

        // GET: api/resources/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResource(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return NotFound();
            return Ok(resource);
        }

        // POST: api/resources
        [HttpPost]
        public async Task<IActionResult> CreateResource(Resource resource)
        {
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetResource), new { id = resource.Id }, resource);
        }

        // PUT: api/resources/borrow/{id}
        [HttpPut("borrow/{id}")]
        public async Task<IActionResult> BorrowResource(int id, [FromQuery] int userId)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null || !resource.IsAvailable) return BadRequest("Resource is not available.");

            resource.IsAvailable = false;
            resource.BorrowedById = userId;
            resource.BorrowedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(resource);
        }

        // PUT: api/resources/return/{id}
        [HttpPut("return/{id}")]
        public async Task<IActionResult> ReturnResource(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null || resource.IsAvailable) return BadRequest("Resource is not borrowed.");

            resource.IsAvailable = true;
            resource.BorrowedById = null;
            resource.BorrowedDate = null;
            resource.ReturnDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(resource);
        }

        // DELETE: api/resources/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return NotFound();

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
