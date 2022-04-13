﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Mvc;

using OrganicFoodShop.Data;
using OrganicFoodShop.Data.Models;
using OrganicFoodShop.Models.Products;

namespace OrganicFoodShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ShopDbContext data;
        private readonly IMapper mapper;

        public ProductsController(ShopDbContext data, IMapper mapper)
        {
            this.data = data;
            this.mapper = mapper;
        }

        public IActionResult Add()
        {
            return this.View(new AddProductFormModel
            {
                Categories = this.GetProductCategories()
            });
        }

        [HttpPost]
        public IActionResult Add(AddProductFormModel product)
        {
            if (!this.data.Categories.Any(c => c.Id == product.CategoryId))
            {
                this.ModelState.AddModelError(nameof(product.CategoryId), "Category does not exist!");
            }

            if (!this.ModelState.IsValid)
            {
                product.Categories = this.GetProductCategories();

                return this.View(product);
            }

            var productData = mapper.Map<Product>(product);
                
            //    new Product{
            //    ImageURL = product.ImageURL,
            //    PriceBuy = product.PriceBuy,
            //    PriceSell = product.PriceSell,
            //    Barcode = product.Barcode,
            //    CategoryId = product.CategoryId,                 
            //    Description = product.Description,
            //    Manufacturer = product.Manufacturer,
            //    Quantity = product.Quantity,
            //    Name = product.Name
            //};

            this.data.Add(productData);
            this.data.SaveChanges();

            return this.RedirectToAction(nameof(All));
        }

        private IEnumerable<CategoryViewModel> GetProductCategories()
        {
            return this.data
                    .Categories
                    .Select(c => new CategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToList();
        }

        public IActionResult All(int category, string manufacturer, string searchTerm)
        {
            var productsQuery = this.data.Products.AsQueryable();

            if (category > 0)
            {
                productsQuery = productsQuery
                    .Where(p =>
                    p.Category.Id == category);
            }

            if (!string.IsNullOrWhiteSpace(manufacturer))
            {
                if (manufacturer != "Select a Manufacturer")
                {
                    productsQuery = productsQuery
                        .Where(p =>
                        p.Manufacturer == manufacturer);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                productsQuery = productsQuery
                    .Where(p =>
                    p.Barcode.ToLower().Contains(searchTerm.ToLower()) ||
                    p.Description.ToLower().Contains(searchTerm.ToLower()) ||
                    p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    p.Manufacturer.ToLower().Contains(searchTerm.ToLower()));
            }


            var products = productsQuery
                .OrderByDescending(p => p.Id)
                // .ProjectTo<ProductListingViewModel>(this.mapper.ConfigurationProvider)
                .Select(p => new ProductListingViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    PriceSell = p.PriceSell,
                    ImageURL = p.ImageURL
                })
                .ToList();

            var productManufacturers = this.data
                .Products
                .Select(p => p.Manufacturer)
                .Distinct()
                .ToList();

            var productCategories = this.data
                .Products
                .Select(p => new CategoryViewModel 
                {
                    Name = p.Category.Name,
                    Id = p.Category.Id
                })
                .Distinct()
                .ToList();

            return View(new AllProductsQueryModel
            {
                Products = products,
                Categories = productCategories,
                Manufacturers = productManufacturers,
                SearchTerm = searchTerm
            });
        }
    }
}
