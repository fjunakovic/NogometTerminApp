namespace NogometTerminApp.Data
{
    public class TermRegistration
    {
        public int Id { get; set; }

        public int TermId { get; set; }
        public Term Term { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public DateTime RegisteredAt { get; set; }
    }
}
