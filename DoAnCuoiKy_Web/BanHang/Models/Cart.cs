using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BanHang.Models
{
	public class Cart
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string UserId { get; set; } // Dùng string vì Identity sử dụng string Id

		[ForeignKey("UserId")]
		public virtual IdentityUser User { get; set; }  // Liên kết với IdentityUser

		[Required]
		public int ProductId { get; set; }

		[ForeignKey("ProductId")]
		public virtual Product Product { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
		public int Quantity { get; set; }

		[Required]
        [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; } 

		[NotMapped]
		public decimal TotalPrice => Quantity * Price;

		public DateTime DateAdded { get; set; } = DateTime.Now;
	}
}
