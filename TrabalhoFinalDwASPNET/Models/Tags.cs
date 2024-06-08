namespace TrabalhoFinalDwASPNET.Models
{
    public class Tags
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Navigation property
        public ICollection<EventTag> EventTags { get; set; }
    }

    public class EventTag
    {
        public int EventId { get; set; }
        public Events Event { get; set; }

        public int TagId { get; set; }
        public Tags Tag { get; set; }
    }
}
