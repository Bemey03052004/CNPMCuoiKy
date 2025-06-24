using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
    public class CartItem
    {
			public int Id { get; set; }
			public string ProductName { get; set; }
			public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal TotalPrice { get; set; }
			public int CheckoutId { get; set; }
			public Checkout Checkout { get; set; } // Mối quan hệ giữa Checkout và CartItem
		}
	}
