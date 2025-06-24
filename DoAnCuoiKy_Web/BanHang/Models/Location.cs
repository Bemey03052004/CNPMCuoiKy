using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
    public class Location
    {
        [Key]
        [Display(Name = "Mã địa chỉ")]
        public int Id { get; set; }

        [Display(Name = "Mã người dùng")]
        public string UserId { get; set; } // Khóa ngoại liên kết với User

        [ForeignKey("UserId")]
        [Display(Name = "Người dùng")]
        public virtual IdentityUser User { get; set; } // Navigation property đến User

        [MaxLength(500, ErrorMessage = "Địa chỉ đường quá dài.")]
        [Display(Name = "Địa chỉ đường")]
        public string StreetAddress { get; set; }

        [Required(ErrorMessage = "Phường/Xã/Thị trấn không được để trống.")]
        [MaxLength(100, ErrorMessage = "Phường/Xã/Thị trấn không được vượt quá 100 ký tự.")]
        [Display(Name = "Phường/Xã/Thị trấn")]
        public string Ward { get; set; }

        [Required(ErrorMessage = "Quận/Huyện không được để trống.")]
        [MaxLength(100, ErrorMessage = "Quận/Huyện không được vượt quá 100 ký tự.")]
        [Display(Name = "Quận/Huyện")]
        public string District { get; set; }

        [Required(ErrorMessage = "Tỉnh/Thành phố không được để trống.")]
        [MaxLength(100, ErrorMessage = "Tỉnh/Thành phố không được vượt quá 100 ký tự.")]
        [Display(Name = "Tỉnh/Thành phố")]
        public string Province { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        public string? description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn miền.")]
        [Display(Name = "Miền")]
        public string Region { get; set; } //  Miền Bắc, Miền Trung, Miền Nam

    }
}