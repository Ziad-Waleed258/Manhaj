using Manhaj.Models;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Services
{
    public static class DataSeeder
    {
        public static void Seed(ManhajDbContext context)
        {
            context.Database.EnsureCreated();

            // Seed Admin
            if (!context.Admins.Any())
            {
                var admin = new Admin
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@manhaj.com",
                    Password = PasswordHasher.HashPassword("Admin@123"),
                    Phone = "01000000000"
                };
                context.Admins.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
