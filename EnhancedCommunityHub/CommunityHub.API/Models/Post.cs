namespace CommunityHub.API.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; } // Lost & Found, Announcements
        public int CreatedBy { get; set; }// user who created it
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
