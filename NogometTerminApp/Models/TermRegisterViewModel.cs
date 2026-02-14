using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NogometTerminApp.Data;

namespace NogometTerminApp.Models
{
    public class TermRegisterViewModel
    {
        public int TermId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public int CurrentCount { get; set; }
        public int MaxPlayers { get; set; }
        
        [ValidateNever]
        public List<TermRegistrationInfo> Registrations { get; set; }
    }
}