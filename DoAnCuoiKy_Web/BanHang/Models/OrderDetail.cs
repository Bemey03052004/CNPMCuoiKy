using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; } // Khóa ngoại từ Order
        public int ProductId { get; set; } // Khóa ngoại từ Product

        [Required]
        public int Quantity { get; set; } // Số lượng sản phẩm

        [Required]
        [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; } // Giá tại thời điểm mua

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
