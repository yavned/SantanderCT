namespace SantanderCT.Models
{
    public class HackerNewsStory
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Uri { get; set; }

        public string? PostedBy { get; set; }

        public DateTime Time { get; set; }

        public int Score { get; set; }

        public int CommentCount { get; set; }

        public string? Type { get; set; }

        public bool Dead { get; set; }

        public bool Deleted { get; set; }
    }
}
