using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Data
{
    public class DefaultIdentity
    {
        public async static Task CreateDefault(UserManager<IdentityUser> userManager,
                                   RoleManager<IdentityRole> roleManager)
        {
            await CreateRole(roleManager);
            await CreateUser(userManager);

        }
        private async static Task CreateUser(UserManager<IdentityUser> userManager)
        {
            if (await userManager.FindByEmailAsync("admin@admin.com") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com"
                };
                var res = await userManager.CreateAsync(user, "Pasword-1");
                if (res.Succeeded)
                {
                   await userManager.AddToRoleAsync(user, "Administrator");
                }

            }
            if (await userManager.FindByEmailAsync("customer1@customer.com") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "customer1@customer.com",
                    Email = "customer1@customer.com"
                };
                var res = await userManager.CreateAsync(user, "Pasword-1");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer");
                }

            }
            if (await userManager.FindByEmailAsync("customer2@customer.com") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "customer2@customer.com",
                    Email = "customer2@customer.com"
                };
                var res = await userManager.CreateAsync(user, "Pasword-1");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer");
                }

            }

        }
        private async static Task CreateRole(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                var createRole = new IdentityRole
                {
                    Name = "Administrator"
                };
                await roleManager.CreateAsync(createRole);

            }
            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                var createRole = new IdentityRole
                {
                    Name = "Customer"
                };
                await roleManager.CreateAsync(createRole);

            }

        }
    }
}
