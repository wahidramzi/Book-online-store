using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookStoreMVCC.Data;
using BookStoreMVCC.Models;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Stripe;
using Stripe.Checkout;


namespace BookStoreMVCC.Controllers
{
	public static class SessionExtensions
	{
		public static void Set<T>(this ISession session, string key, T value)
		{
			session.SetString(key, JsonSerializer.Serialize(value));
		}

		public static T Get<T>(this ISession session, string key)
		{
			var value = session.GetString(key);
			return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
		}
	}


	public class BooksController : Controller
    {

        private readonly ApplicationDBContext _context;



        public BooksController(ApplicationDBContext context)
        {
            _context = context;
			StripeConfiguration.ApiKey = "sk_test_51NQq4yDHGjRNLcOaSt4ciaOkAxiEOJlIdpoHKMXJAJCjF8Fv7HMsL9gXnOxG03bGF8w0JqS59zqGAPfVFG3nGOY400TvG6LTHY";

		}

		// GET: Books
		[Authorize(Roles ="admin,manager")]
        public async Task<IActionResult> Index()
        {
              return _context.Books != null ? 
                          View(await _context.Books.ToListAsync()) :
                          Problem("Entity set 'ApplicationDBContext.Books'  is null.");
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        
        // GET: Books/Create
        [Authorize(Roles = "admin,manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Author,Category,Price")] Book book)
        {

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        // Log error.ErrorMessage or any other relevant information
                    }
                }
                return View(book);

               
            }
            _context.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "admin,manager")]
        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Author,Category,Price")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Books == null)
            {
                return Problem("Entity set 'ApplicationDBContext.Books'  is null.");
            }
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
          return (_context.Books?.Any(e => e.Id == id)).GetValueOrDefault();
        }



        public async Task<IActionResult> AddToCart(int Id)
        {
            var book = _context.Books.FirstOrDefault(e => e.Id == Id);  

            List<Book> Cart = HttpContext.Session.Get<List<Book>>("Cart");

            if (Cart == null)
            {
                Cart = new List<Book>();
            }
            var exsitbook = Cart.FirstOrDefault(e => e.Id == Id);
            if (exsitbook != null)
            {
                exsitbook.Quantity++;
                exsitbook.Total = exsitbook.Price * exsitbook.Quantity;
			}
            else
            {
                book.Quantity = 1;  
				Cart.Add(book);
                book.Total = book.Price;
			}

            
			HttpContext.Session.Set("Cart", Cart);
            return RedirectToAction("ViewCart");


        }

        public IActionResult ViewCart()
        {

            float fulltotal = 0;
            List<Book> Cart = HttpContext.Session.Get<List<Book>>("Cart");
            if (Cart == null)
            {
                return RedirectToAction("Index", "Home");
            }

            foreach (var item in Cart)
            {
                fulltotal += item.Total;
                
            }
            
            ViewBag.fulltotal = fulltotal;
            return View(Cart);
        }


        public async Task<IActionResult> DeleteItem(int Id)
        {
            var cart = HttpContext.Session.Get<List<Book>>("Cart");
            var book = cart.FirstOrDefault(e => e.Id == Id);
            if(book.Quantity > 1)
            {
                book.Quantity--;
                book.Total -= book.Price;
                
            }
            else
            {
				cart.Remove(book);

			}
			HttpContext.Session.Set("Cart", cart);
            return RedirectToAction("ViewCart");
            
        }


		[HttpPost("create-checkout-session")]
		public ActionResult CreateCheckoutSession()
		{
			var options = new SessionCreateOptions
			{
				LineItems = new List<SessionLineItemOptions>
		{
		  new SessionLineItemOptions
		  {
			PriceData = new SessionLineItemPriceDataOptions
			{
			  UnitAmount = 2000,
			  Currency = "usd",
			  ProductData = new SessionLineItemPriceDataProductDataOptions
			  {
				Name = "T-shirt",
			  },
			},
			Quantity = 1,
		  },
		},
				Mode = "payment",
				SuccessUrl = "https://localhost:7245/",
				CancelUrl = "http://localhost:7245/cancel.html",
			};

			var service = new SessionService();
			Session session = service.Create(options);

			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);
		}



	}





}
