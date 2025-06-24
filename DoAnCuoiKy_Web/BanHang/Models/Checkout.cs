using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
	public class Checkout
	{
        public List<Location> Locations;

        public int Id { get; set; }
		public string UserId { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal TotalCartPrice { get; set; }
		public List<CartItem> CartItems { get; set; }
	}
}
