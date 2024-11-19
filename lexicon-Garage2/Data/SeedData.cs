using lexicon_Garage2.Models;
using Microsoft.AspNetCore.Identity;

namespace lexicon_Garage2.Data
{
    public class SeedData
    {
        private static lexicon_Garage2Context context = default!;
        private static RoleManager<IdentityRole> roleManager = default!;
        private static UserManager<ApplicationUser> userManager = default!;

        public static async Task Init(lexicon_Garage2Context _context, IServiceProvider services)
        {
            context = _context;

            // Exit if roles already exist
            if (context.Roles.Any())
                return;

            roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Define roles and account details
            var roleNames = new[] { "Admin", "User" };
            var adminEmail = "admin@admin.com";
            var userEmail = "user@user.com";

            // Seed roles
            await AddRolesAsync(roleNames);

            // Seed accounts
            var admin = await AddAccountAsync(
                adminEmail,
                "Admin",
                "Adminsson",
                "800101-1234",
                "Abcd_1234"
            );
            var user = await AddAccountAsync(
                userEmail,
                "User",
                "Usersson",
                "990202-5678",
                "Abcd_1234"
            );

            // Assign roles
            if (admin != null)
                await AddUserToRoleAsync(admin, "Admin");

            if (user != null)
                await AddUserToRoleAsync(user, "User");
        }

        private static async Task AddUserToRoleAsync(ApplicationUser user, string roleName)
        {
            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var result = await userManager.AddToRoleAsync(user, roleName);
                if (!result.Succeeded)
                    throw new Exception(string.Join("\n", result.Errors));
            }
        }

        private static async Task AddRolesAsync(string[] roleNames)
        {
            foreach (var roleName in roleNames)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                    continue;

                var role = new IdentityRole { Name = roleName };
                var result = await roleManager.CreateAsync(role);

                if (!result.Succeeded)
                    throw new Exception(string.Join("\n", result.Errors));
            }
        }

        private static async Task<ApplicationUser> AddAccountAsync(
            string accountEmail,
            string fName,
            string lName,
            string ssn,
            string pw
        )
        {
            var found = await userManager.FindByEmailAsync(accountEmail);
            if (found != null)
                return null;

            var user = new ApplicationUser
            {
                UserName = accountEmail,
                Email = accountEmail,
                FirstName = fName,
                LastName = lName,
                Id = ssn, // Use SSN as a unique identifier (assuming this is desired)
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user, pw);
            if (!result.Succeeded)
                throw new Exception(string.Join("\n", result.Errors));

            return user;
        }
    }
}
