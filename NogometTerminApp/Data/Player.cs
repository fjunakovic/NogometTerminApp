namespace NogometTerminApp.Data
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<TermRegistration> TermRegistrations { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
