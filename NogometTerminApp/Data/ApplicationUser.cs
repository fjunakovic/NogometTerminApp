using Microsoft.AspNetCore.Identity;

namespace NogometTerminApp.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
    }
}
