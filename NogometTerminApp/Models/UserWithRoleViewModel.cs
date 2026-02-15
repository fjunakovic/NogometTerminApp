using NogometTerminApp.Data;

namespace NogometTerminApp.Models
{
    public class UserWithRoleViewModel
    {
        public ApplicationUser User { get; set; }
        public bool IsAdmin { get; set; }
    }
}
