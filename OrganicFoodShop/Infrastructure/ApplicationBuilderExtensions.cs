﻿using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OrganicFoodShop.Data;
using OrganicFoodShop.Data.Models;

using System.Linq;

namespace OrganicFoodShop.Infrastructure
{
    public static class ApplicationBuilderExtensions
    {

        public static IApplicationBuilder PrepareDatabase(this IApplicationBuilder app)
        {
            //create scope for dbcontext
            using var serviceScope = app.ApplicationServices.CreateScope();

            var db = serviceScope.ServiceProvider.GetRequiredService<ShopDbContext>();

            db.Database.Migrate();

            SeedCategories(db);

            return app;
        }

        private static void SeedCategories(ShopDbContext db)
        {
            if (db.Categories.Any())
            {
                return;
            }

            db.Categories.AddRange(new[]
            {
                new Category {Name = "Superfoods"},
                new Category {Name = "GrainsSeedsNutsFruits"},
                new Category {Name = "OilsSpreadsJams"},
                new Category {Name = "CoffeeTea"},
                new Category {Name = "ChocolatesDesserts"},
                new Category {Name = "NutritionalSupplements"},
                new Category {Name = "Drinks"},
                new Category {Name = "Cosmetics"},
                new Category {Name = "Other"}
            });

            //[Display(Name = "Grains, Seeds, Nuts, Fruits")] = 2,
            //[Display(Name = "Oils, Spreads, Jams")] s = 3,
            //[Display(Name = "Coffee, Tea")]  = 4,
            //[Display(Name = "Chocolates, Desserts")]  = 5,
            //[Display(Name = "Nutritional Supplements")]  = 6,

            db.SaveChanges();
        }
    }
}
