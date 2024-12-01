using Foodie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Foodie.Controllers
{
    public class ProductController : Controller
    {
        private readonly FoodieDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ProductController(FoodieDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        [Authorize]
        public IActionResult AddCategory()
        {

            return View();
        }
        [HttpPost]
        [Authorize]
        public IActionResult AddCategory(CategoryEdit edit)
        {
            if (edit.Name == null)
            {
                ModelState.AddModelError("", "Category name is required");
                return View(edit);
            }
            else
            {
                Category category = new()
                {
                    Name = edit.Name,
                    CreatedDate = DateTime.Now,

                };
                _context.Categories.Add(category);
                _context.SaveChanges();
                return Json("Success");
            }
        }
        [HttpGet]
        [Authorize]
        public IActionResult ViewCategory()
        {
            var category= _context.Categories.ToList();
            List<CategoryEdit> catagories= new List<CategoryEdit>();
            if (category != null)
            {
                foreach(var ctg in category)
                {
                    CategoryEdit c = new()
                    {
                        CategoryId= ctg.CategoryId,
                        Name=ctg.Name!,
                        CreatedDate= ctg.CreatedDate,
                    };
                catagories.Add(c);
                }
                return View(catagories);
            }
            else
            {
                ModelState.AddModelError("", "Category is not created at please create some category");
                return View();
            }
        }
        [HttpGet]
        [Authorize]
        public IActionResult RemoveCategory(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);
            if (category != null)
            {
                _context.Categories.Remove(category!);
                _context.SaveChanges();
                return RedirectToAction("ViewCategory", "Product");
            }
            else
            {
                ModelState.AddModelError("", "No category found with that id");
                return RedirectToAction("ViewCategory", "Product");
            }
        }
        [HttpGet]
        [Authorize]
        public IActionResult EditCategory(int id)
        {
           var category=_context.Categories.Where(c=>c.CategoryId== id).FirstOrDefault();
            CategoryEdit c = new()
            {
                CategoryId = category!.CategoryId,
                Name = category.Name!
            };
            return PartialView("_EditCategory", c);
        }
        [HttpPost]
        [Authorize]
        public IActionResult EditCategory(CategoryEdit edit)
        {
            if (User.IsInRole("Admin"))
            {
                var category = _context.Categories.Where(c => c.CategoryId == edit.CategoryId).FirstOrDefault();
                if (edit.Name != null)
                {
                    category!.Name = edit.Name;
                    _context.Categories.Update(category);
                    _context.SaveChanges();
                    return RedirectToAction("ViewCategory");
                }
                else
                {
                    ModelState.AddModelError("", "Category name can't be null");
                    return PartialView("_EditCategory",edit);
                }
            }
            else
            {
                return RedirectToAction("ViewCategory");
            }
        }
        [HttpGet]
        public IActionResult SearchCategory(string Search)
        {
            var category = _context.Categories.ToList();
            var filterdCategory = category;
            if (category != null && Search != null)
            {
                filterdCategory = category.Where(c => c.Name!.ToLower().Contains(Search.ToLower())).ToList();
            }
            List<CategoryEdit> catagories = new List<CategoryEdit>();
            if (filterdCategory.Count > 0)
            {
                foreach (var ctg in filterdCategory)
                {
                    CategoryEdit c = new()
                    {
                        CategoryId = ctg.CategoryId,
                        Name = ctg.Name!,
                        CreatedDate = ctg.CreatedDate,
                    };
                    catagories.Add(c);
                }
                return PartialView("_SearchCategory", catagories);
            }
            else
            {
                if (category != null)
                {
                    foreach (var ctg in category)
                    {
                        CategoryEdit c = new()
                        {
                            CategoryId = ctg.CategoryId,
                            Name = ctg.Name!,
                            CreatedDate = ctg.CreatedDate,
                        };
                        catagories.Add(c);
                    }
                    return PartialView("_SearchCategory", catagories);
                }
                else
                {
                    ModelState.AddModelError("", "No categories match");
                    return PartialView("_SearchCategory");
                }
            }
        }
        [HttpGet]
        public IActionResult ViewProduct()
        {
            
            var products=_context.Products.ToList();
            List<ProductEdit> productEdits = new List<ProductEdit>();
            foreach (var product in products)
            {
                ProductEdit p = new()
                {

                    ProductId = product!.ProductId,
                    Name = product.Name!,
                    Description = product.Description!,
                    Price = Convert.ToDecimal(product.Price),
                    ImageUrl = product.ImageUrl!,
                    Quantity = Convert.ToInt32(product.Quantity)
                };
                productEdits.Add(p);
            }
            return View(productEdits);
            
        }
        [HttpGet]
        [Authorize(Roles="Admin")]
        public IActionResult AddProduct()
        {
            ViewData["category"] = new SelectList(_context.Categories, nameof(Category.CategoryId), nameof(Category.Name));
            return View();
        }
        [HttpPost]
        [Authorize]
        public IActionResult AddProduct(ProductEdit edit)
        {
            if (edit != null)
            {
                if (edit.foodImage != null)
                {
                    var fileName=Guid.NewGuid()+"_"+Path.GetExtension(edit.foodImage.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "ProductImages", fileName);
                    using FileStream str = new FileStream(filePath, FileMode.Create);
                    edit.foodImage.CopyTo(str);

                    Product p = new()
                    {
                        Name = edit.Name,
                        Description = edit.Description,
                        Price = edit.Price,
                        Quantity = edit.Quantity,
                        CategoryId = edit.CategoryId,
                        IsActive = true,
                        ImageUrl = fileName,
                        CreatedDate= DateTime.Now,
                    };
                    _context.Products.Add(p);
                    _context.SaveChanges();
                    TempData["success"] = "Product added successfully";
                    return RedirectToAction("ViewProduct", "Product");

                }
                else
                {
                    ModelState.AddModelError("", "Image fields are required"); 
                    return View(edit);
                }
            }
            else
            {
                ModelState.AddModelError("", "All fields are required");
                return View(edit);
            }
            
        }
        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var products=_context.Products.Where((product)=>product.ProductId==id).FirstOrDefault();
            ProductEdit p = new()
            {

                ProductId = products!.ProductId,
                Name = products.Name!,
                Description = products.Description!,
                Price = Convert.ToDecimal(products.Price),
                ImageUrl = products.ImageUrl!,
                Quantity = Convert.ToInt32(products.Quantity)
            };
            //return Json(p);
            return PartialView("_EditProduct", p);
        }
    }
}
