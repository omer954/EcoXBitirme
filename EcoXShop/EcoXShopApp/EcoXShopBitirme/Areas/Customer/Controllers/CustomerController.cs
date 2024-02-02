using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EcoXShopBitirme.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EcoXShopBitirme.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EcoXShopBitirme.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CustomerController : Controller
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ApplicationDbContext _db;

        public CustomerController(ILogger<CustomerController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            var products = _db.Products.Where(i => i.IsHome && i.IsStock).ToList();
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var count = _db.ShoppingCarts.Where(i => i.ApplicationUserId == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32(Diger.ssShoppingCart, count);
            }
            return View(products);
        }
        public IActionResult CategoryDetails(int? id)
        {
            var product = _db.Products.Where(i => i.CategoryId == id).ToList();
            ViewBag.KategoriId = id;
            return View(product);
        }
        public IActionResult Details(int id)
        {
            var product = _db.Products.FirstOrDefault(i => i.Id == id);
            ShoppingCart cart = new ShoppingCart()
            {
                Product = product,
                ProductId = product.Id
            };
            return View(cart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart Scart)
        {
            Scart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                Scart.ApplicationUserId = claim.Value;
                ShoppingCart cart = _db.ShoppingCarts.FirstOrDefault(
                    u => u.ApplicationUserId == Scart.ApplicationUserId && u.ProductId == Scart.ProductId);
                if (cart == null)
                {
                    _db.ShoppingCarts.Add(Scart);
                }
                else
                {
                    cart.Count += Scart.Count;
                }
                _db.SaveChanges();
                var count = _db.ShoppingCarts.Where(i => i.ApplicationUserId == Scart.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(Diger.ssShoppingCart, count);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var product = _db.Products.FirstOrDefault(i => i.Id == Scart.Id);
                ShoppingCart cart = new ShoppingCart()
                {
                    Product = product,
                    ProductId = product.Id
                };
            }

            return View(Scart);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        public IActionResult Search(string q)
        {
			var sustainabilityFilters = HttpContext.Request.Form["sustainabilityFilters"];

			// Anahtar kelime araması
			if (!String.IsNullOrEmpty(q))
			{
				var keywordResults = _db.Products.Where(i => i.Title.Contains(q) || i.Description.Contains(q)).ToList();
				// Eğer sürdürülebilirlik filtresi de varsa, bu sonuçları filtrele
				if (!string.IsNullOrEmpty(sustainabilityFilters))
				{
					keywordResults = FilterResultsBySustainability(keywordResults, sustainabilityFilters);
				}
				return View(keywordResults);
			}

			// Sürdürülebilirlik filtreleme
			if (!string.IsNullOrEmpty(sustainabilityFilters))
			{
				var sustainabilityResults = FilterResultsBySustainability(_db.Products.ToList(), sustainabilityFilters);
				return View(sustainabilityResults);
			}

			// Eğer hiçbir arama veya filtreleme yapılmadıysa, tüm ürünleri getir
			return RedirectToAction(nameof(Index));



			
		}

        private List<Product> FilterResultsBySustainability(List<Product> products, string sustainabilityFilters)
        {
            var filteredProducts = products.AsQueryable();

            if (sustainabilityFilters.Contains("recycledMaterials"))
            {
                filteredProducts = filteredProducts.Where(p => p.Sustainability.Contains("Geri dönüştürülmüş"));
            }

            if (sustainabilityFilters.Contains("bluesignCriteria"))
            {
                filteredProducts = filteredProducts.Where(p => p.Sustainability.Contains("bluesign kriterlerini karşılayan"));
            }

            if (sustainabilityFilters.Contains("climateNeutralCertified"))
            {
                filteredProducts = filteredProducts.Where(p => p.Sustainability.Contains("İklim Nötr Sertifikalı bir markadan"));
            }

            return filteredProducts.ToList();
        }
        public IActionResult Contact()
		{
			return View();
		}
		public IActionResult About()
		{
			return View();
		}

	}
}
