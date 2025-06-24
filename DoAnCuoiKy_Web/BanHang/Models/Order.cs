using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }  // Khóa ngoại tham chiếu đến User

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = "Pending";

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        // Thêm LocationId vào Order
        public int? LocationId { get; set; } // Khóa ngoại tham chiếu đến Location

        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }  // Điều hướng đến Location
    }


}
