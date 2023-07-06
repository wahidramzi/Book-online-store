using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStoreMVCC.Controllers
{
	
	public class AppRoleController : Controller
	{

		public readonly RoleManager<IdentityRole> _roleManager;

		public AppRoleController(RoleManager<IdentityRole> roleManager)
		{
			_roleManager = roleManager;
		}

		public IActionResult Index()
		{
			var roles = _roleManager.Roles;
			return View(roles);
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(IdentityRole model)
		{
			if(!await _roleManager.RoleExistsAsync(model.Name))
			{
				await _roleManager.CreateAsync(new IdentityRole (model.Name));
			}
			return RedirectToAction("Index");
		}


		
	}
}
