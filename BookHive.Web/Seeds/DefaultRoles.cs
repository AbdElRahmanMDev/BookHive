using BookHive.Web.consts;
using Microsoft.AspNetCore.Identity;

namespace BookHive.Web.Seeds
{
    public static class DefaultRoles
    {

        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if(!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));
                await roleManager.CreateAsync(new IdentityRole(AppRoles.Reception));
                await roleManager.CreateAsync(new IdentityRole(AppRoles.Archieve));

            }
        }
    }
}
