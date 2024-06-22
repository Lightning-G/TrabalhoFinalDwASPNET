using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrabalhoFinalDwASPNET.Data;
using TrabalhoFinalDwASPNET.Models;

namespace TrabalhoFinalDwASPNET
{
    public class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            context.Database.Migrate();
            
            // Verifica se há eventos na base de dados
            if (context.Users.Any())
            {
                return; // A base de dados já foi inicializado
            }

            // Criação de utilizadores iniciais
            var user1 = new IdentityUser { UserName = "user1@example.com", Email = "user1@example.com" };
            var user2 = new IdentityUser { UserName = "user2@example.com", Email = "user2@example.com" };
            var user3 = new IdentityUser { UserName = "user3@example.com", Email = "user3@example.com" };

            await userManager.CreateAsync(user1, "PassWord00!");
            await userManager.CreateAsync(user2, "PassWord00!");
            await userManager.CreateAsync(user3, "PassWord00!");

            await context.SaveChangesAsync();
        }
    }
}
