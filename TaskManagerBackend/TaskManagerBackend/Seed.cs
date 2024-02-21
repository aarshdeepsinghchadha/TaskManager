using Domain;
using Microsoft.AspNetCore.Identity;

namespace TaskManagerBackend
{
    public static class Seed
    {
        public static async Task Initialize(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
           
            string[] roleNames = { "Admin" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            string adminPassword = "Admin333!";

            // Create a default Admin
            var admin = new AppUser
            {
                UserName = "admin3",
                Email = "admin3@example.com",
                FirstName = "Admin3",
                LastName = "User3",
                EmailConfirmed = true ,
                //ConfirmPassword = adminPassword

            };

          
            var adminUser = await userManager.FindByEmailAsync(admin.Email);
            if (adminUser == null)
            {
                var createAdmin = await userManager.CreateAsync(admin, adminPassword);
              
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin ,"Admin");
                    
                }
            }
        }
    }
}



