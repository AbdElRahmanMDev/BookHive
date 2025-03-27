using Microsoft.AspNetCore.Identity;

namespace BookHive.Web.Core.Models
{
    [Index(nameof(Email),IsUnique =true)]
    [Index(nameof(UserName),IsUnique =true)]
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        public string? CreatedById { get; set; }
        public string? LastUpdatedById { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime? LastUpdateOn { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

    }
}
