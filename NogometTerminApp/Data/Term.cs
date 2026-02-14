namespace NogometTerminApp.Data
{
    public class Term
    {
        public int Id { get; set; }
        public DateTime TermDateTime { get; set; }
        public string Location { get; set; }
        public int MaxPlayers { get; set; }

        public ICollection<TermRegistration> Registrations { get; set; }
    }
}
