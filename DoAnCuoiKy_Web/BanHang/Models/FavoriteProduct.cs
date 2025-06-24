using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
    public class FavoriteProduct
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required]
        public string UserId { get; set; } // Dùng string vì Identity sử dụng string Id

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }  // Liên kết với IdentityUser
        public Product Product { get; set; }
    }

}
