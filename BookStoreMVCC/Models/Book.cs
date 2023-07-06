using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreMVCC.Models
{
	public class Book
	{
		public int Id {  get; set; }


		public string Title { get; set; }

		public string Description { get; set; }


        public string Author { get; set; }

		public string Category { get; set; }

		public float Price { get; set; }

		public int Quantity { get; set; }

		public float Total { get; set; }



        
    }
}
