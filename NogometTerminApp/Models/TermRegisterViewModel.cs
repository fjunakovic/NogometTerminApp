using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NogometTerminApp.Data;
using System.Collections.Generic;
using System.Linq;

namespace NogometTerminApp.Models
{
    public class TermRegisterViewModel
    {
        public int TermId { get; set; }
        public int CurrentCount { get; set; }
        public int MaxPlayers { get; set; }
        
        [ValidateNever]
        public List<TermRegistrationInfo> Registrations { get; set; }

        public IEnumerable<TermRegistrationInfo> WhiteTeam =>
            Registrations?.Where(r => r.Team == "Bijeli") ?? new List<TermRegistrationInfo>();

        public IEnumerable<TermRegistrationInfo> DarkTeam =>
            Registrations?.Where(r => r.Team == "Tamni") ?? new List<TermRegistrationInfo>();

        public IEnumerable<TermRegistrationInfo> NoTeam =>
            Registrations?.Where(r => string.IsNullOrEmpty(r.Team)) ?? new List<TermRegistrationInfo>();

        public bool IsCurrentUserRegistered { get; set; }
    }
}