namespace NogometTerminApp.Models
{
    public class TermRegisterViewModel
    {
        public int TermId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public int CurrentCount { get; set; }
        public int MaxPlayers { get; set; }
        public List<string> PlayerNames { get; set; }
    }
}