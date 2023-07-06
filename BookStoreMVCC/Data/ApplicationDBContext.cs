using BookStoreMVCC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStoreMVCC.Data
{
	public class ApplicationDBContext : IdentityDbContext
	{

		public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }

		public DbSet<Book> Books { get; set; }


		public DbSet<ApplicationUser> applicationUsers { get; set; }	


	}
}
