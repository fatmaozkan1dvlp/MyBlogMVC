using Microsoft.AspNetCore.Identity;

namespace Basics.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}

