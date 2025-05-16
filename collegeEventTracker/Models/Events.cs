namespace collegeEventTracker2.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Venue { get; set; }
        public DateTime DateTime { get; set; }
        public int OrganizerId { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsTeamEvent { get; set; }
    }
}
