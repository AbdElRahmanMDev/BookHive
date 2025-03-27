﻿using Microsoft.AspNetCore.Identity;

namespace BookHive.Web.consts
{
    public static class DefaultUsers
    {

        public static async Task SeedAdminUserAync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser admin = new ApplicationUser()
            {
                UserName="admin",
                Email="admin@bookify.com",
                FullName="Admin",
                EmailConfirmed=true,
            };

            var user=await userManager.FindByEmailAsync(admin.Email);
            if(user == null)
            {
                await userManager.CreateAsync(admin,"P@ssword123");
                await userManager.AddToRoleAsync(admin,AppRoles.Admin);
            }
        }
    }
}
