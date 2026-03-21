using Learnova.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Learnova.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Create Roles
            string[] roleNames = { "Admin", "Instructor", "Learner" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create Admin User
            var adminEmail = "admin@learnova.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // Create Sample Instructor
            var instructorEmail = "instructor@learnova.com";
            var instructorUser = await userManager.FindByEmailAsync(instructorEmail);

            if (instructorUser == null)
            {
                var newInstructor = new ApplicationUser
                {
                    UserName = instructorEmail,
                    Email = instructorEmail,
                    FirstName = "John",
                    LastName = "Instructor",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newInstructor, "Instructor@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newInstructor, "Instructor");
                }
            }

            // Create Sample Learner
            var learnerEmail = "learner@learnova.com";
            var learnerUser = await userManager.FindByEmailAsync(learnerEmail);

            if (learnerUser == null)
            {
                var newLearner = new ApplicationUser
                {
                    UserName = learnerEmail,
                    Email = learnerEmail,
                    FirstName = "Jane",
                    LastName = "Learner",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newLearner, "Learner@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newLearner, "Learner");
                }
            }
        }
    }
}