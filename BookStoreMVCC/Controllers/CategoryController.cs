using BookStoreMVCC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreMVCC.Controllers
{
	public class CategoryController : Controller
	{
		public readonly ApplicationDBContext _context;

		public CategoryController(ApplicationDBContext context)
		{
			_context = context;
		
		}

		public IActionResult SortCategory(string CategoryName)
		{
			var books = _context.Books.Where(b=> b.Category == CategoryName).ToList();
			return View(books);
		}
	}
}
