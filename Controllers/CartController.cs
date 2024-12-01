
using Foodie.Models;
using Foodie.SecurityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Foodie.Controllers
{
    public class CartController : Controller
    {
        private readonly FoodieDbContext _context;
        private readonly IDataProtector _Prtector;
        public CartController(FoodieDbContext context,DataSecurityProvider data,IDataProtectionProvider _provider)
        {
            _context = context;
            _Prtector = _provider.CreateProtector(data.Seckey);
            
        }
        [HttpGet]
        public IActionResult ViewCart()
        {
            var carts = _context.Carts
                .Where(c => c.UserId == Convert.ToInt32(User.Identity!.Name))
                .Include(c => c.Product)
                .Select(c => new
                {
                    c.CartId,
                    c.Quantity,
                    c.Product!.Name,
                    c.Product.Price,
                    c.Product!.Description,
                    c.Product!.ImageUrl,
                    c.Product!.Category
                })
                .ToList();
            List<CartEdit> newcarts = new List<CartEdit>();
            decimal totalPrice=0;
            foreach (var cart in carts)
            {
                CartEdit c = new()
                {
                    CartId=cart.CartId,
                    Quantity=cart.Quantity, 
                    ProductName=cart.Name!,
                    ProductPrice=Convert.ToDecimal(cart.Price),
                    Description=cart.Description!,
                    ImageUrl=cart.ImageUrl!,
                    Category=cart.Category!.Name!,
                };
                var TPrice =Convert.ToDecimal( cart.Quantity * cart.Price);
                var price = Convert.ToDecimal(cart.Quantity * cart.Price);
                totalPrice +=TPrice;
                c.Price=price;
                newcarts.Add(c);
            }
            ViewBag.TotalPrice = totalPrice;
            return View(newcarts);
        }

        [HttpGet]
        [Authorize]
        public IActionResult AddToCart(int Id)
        {
            var carts = _context.Carts.Where(cart=>cart.ProductId == Id&&cart.UserId==Convert.ToInt32(User.Identity.Name)).FirstOrDefault();
            if(carts==null)
            {
                Cart c = new()
                {
                    Quantity = 1,
                    ProductId = Id,
                    UserId = Convert.ToInt32(User.Identity?.Name)
                };
                _context.Carts.Add(c);
            }
            else
            {
                carts.Quantity += 1; 
            }
            _context.SaveChanges();
            return Json("Success");
        }

        public IActionResult RemoveCartItem(int Id)
        {
            var carts=_context.Carts.Where(c=>c.CartId == Id).FirstOrDefault();
            _context.Remove(carts);
            _context.SaveChanges();
            //return Json("Delete");
            return RedirectToAction("ViewCart", "Cart");
        }
      
        [HttpPost]
        public IActionResult UpdateCartQuantity(int CartId, int Quantity)
        {
            if (Quantity <= 0)
            {
                return BadRequest("Invalid quantity");
            }
            var cartItem = _context.Carts.FirstOrDefault(c => c.CartId == CartId);
            if (cartItem != null)
            {
                cartItem.Quantity = Quantity;
                _context.SaveChanges();
            }
            return RedirectToAction("ViewCart", "Cart");
        }
        [HttpGet]
        public IActionResult CheckOutPayment()
        {
            var carts = _context.Carts
               .Where(c => c.UserId == Convert.ToInt32(User.Identity!.Name))
               .Include(c => c.Product)
               .Include(c => c.User)
               .Select(c => new
               {
                   c.CartId,
                   c.Quantity,
                   c.Product!.Name,
                   c.Product.Price,
                   c.Product!.Description,
                   c.Product!.ImageUrl,
                   c.Product!.Category,
                   UserName = c.User!.Name,
                   c.User.Address
               })
               .ToList();
            if(carts.Count > 0)
            {
                List<CartEdit> newcarts = new List<CartEdit>();
                decimal totalPrice = 0;
                foreach (var cart in carts)
                {
                    CartEdit c = new()
                    {
                        CartId = cart.CartId,
                        Quantity = cart.Quantity,
                        ProductName = cart.Name!,
                        ProductPrice = Convert.ToDecimal(cart.Price),
                        Description = cart.Description!,
                        ImageUrl = cart.ImageUrl!,
                        Category = cart.Category!.Name!,
                        Address = cart.Address!,
                    };
                    var TPrice = Convert.ToDecimal(cart.Quantity * cart.Price);
                    var price = Convert.ToDecimal(cart.Quantity * cart.Price);
                    totalPrice += TPrice;
                    c.UserName = cart.UserName!;
                    c.Price = price;
                    c.Address = cart.Address!;
                    newcarts.Add(c);
                }
                ViewBag.TotalPrice = totalPrice;
                var userinfo=newcarts.Select(c => new{ c.UserName,c.Address }).FirstOrDefault();
                ViewBag.UserName =userinfo!.UserName;
                ViewBag.Address = userinfo.Address;
                return View(newcarts);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult CheckOutPayment(string UserName,string Address,decimal TotalAmount,string payment)
        {
            Console.WriteLine(UserName);
            Console.WriteLine(Address);
            Console.WriteLine(TotalAmount);
            Console.WriteLine(payment);

            // Now you can use this paymentId to create an order
            int OrderId;
            if (_context.Orders.Any())
            {
                OrderId = _context.Orders.Max(x => x.OrderId) + 1;
            }
            else
            {
                OrderId = 1;
            }
            var order = new Order
            {
                OrderId = OrderId,
                Status = "pending",
                OrderDate = DateTime.Now,
                UserId = Convert.ToInt32(User.Identity?.Name),
                ProductId=10,
                Address=Address,
            };
            _context.Orders.Add(order);
            _context.SaveChanges();
            //Add the data in OrderDetails table here on OrderId has many OrderDetails
            var cartProducts=_context.Carts.Where(x=>x.UserId==Convert.ToInt32(User.Identity!.Name)).ToList();
            if (order != null)
            {
                foreach (var cartProduct in cartProducts)
                {
                    var orderDetailsData = new OrderDetail
                    {
                        Quantity = Convert.ToInt32(cartProduct!.Quantity) ,// Assuming `Quantity` is a property of `Cart`
                        OrderId = order.OrderId,
                        ProductId = Convert.ToInt32(cartProduct!.ProductId) , // Assuming `ProductId` is a property of `Cart`
                        DetailsDate = DateTime.Now
                    };

                    _context.OrderDetails.Add(orderDetailsData);
                }

                // Save changes once after the loop
                _context.SaveChanges();
            }
            else
            {
                // Handle the case where `order` is null
                throw new InvalidOperationException("Order is not initialized.");
            }
            //Add Data into payment table One Order has one payment 
            if (payment == "cod")
            {
                var paymentDetails = new Payment
                {
                    PaymentMode = payment,
                    Address = Address,
                    TotalAmount = TotalAmount,
                    OrderId = order.OrderId,
                    PaymentStatus = "pending"
                    
                };
                _context.Payments.Add(paymentDetails);
                _context.SaveChanges();
                _context.Carts.Where(x=>x.UserId == Convert.ToInt32(User.Identity!.Name)).ExecuteDelete();
                _context.SaveChanges();

            }
            else
            {
                var paymentDetails = new Payment
                {
                    PaymentMode = payment,
                    Address = Address,
                    TotalAmount = TotalAmount,
                    OrderId = order.OrderId,
                    PaymentStatus = "pending"


                };
                _context.Payments.Add(paymentDetails);
                _context.SaveChanges();
                return RedirectToAction("Epay", new
                {
                    PaymentMode = paymentDetails.PaymentMode,
                    Address = paymentDetails.Address,
                    TotalAmount = paymentDetails.TotalAmount,
                    OrderId = paymentDetails.OrderId,
                    EncOrderId= _Prtector.Protect(order.OrderId.ToString())

                });
            }
            return Json("Success");
        }
        [HttpGet]
        public IActionResult Epay(string PaymentMode, string Address, decimal TotalAmount, int OrderId,string EncOrderId)
        {
            // Use the payment data for further processing with eSewa
            var paymentDetails = new PaymentEdit
            {
                PaymentMode = PaymentMode,
                Address = Address,
                TotalAmount = TotalAmount,
                OrderId = OrderId,
                EncOrderId = EncOrderId
            };

            // Handle eSewa payment logic here

            return View(paymentDetails);  // Optionally pass the data to the view
        }
        [HttpGet]
        public IActionResult Success(string q, string oid, string amt, string refId)
        {
            var OrderId = _Prtector.Unprotect(oid);
            var PaymentData=_context.Payments.Where(x => x.OrderId==Convert.ToInt32(OrderId)).FirstOrDefault();
            if (PaymentData != null)
            {
                // Update the existing payment's status
                PaymentData.PaymentStatus = "Paid";

                // Save the changes to the database
                _context.Update(PaymentData);
                _context.SaveChanges();
                //remove the cart items after payments
                _context.Carts.Where(x => x.UserId == Convert.ToInt32(User.Identity!.Name)).ExecuteDelete();
                _context.SaveChanges();
            }

            Console.WriteLine(OrderId);
            return View();
        }
        public IActionResult Failure()
        {
            return View();
        }
        
    }
}
