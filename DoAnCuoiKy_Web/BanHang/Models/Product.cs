using BanHang.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

public enum ProductApprovalStatus
{
    [Display(Name = "Chờ duyệt")]
    ChoDuyet = 0,

    [Display(Name = "Đã duyệt")]
    DaDuyet = 1,

    [Display(Name = "Đã hủy")]
    DaHuy = 2
}

public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [DisplayName("Mã sản phẩm")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc"), StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự")]
    [DisplayName("Tên sản phẩm")]
    public string Name { get; set; }

    [Range(1.000, 1000000000000000000, ErrorMessage = "Giá sản phẩm phải nằm trong khoảng từ 1.000 đến 10000000000000.000")]
    [DisplayName("Giá bán")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [DisplayName("Mô tả")]
    public string Description { get; set; }

    [DisplayName("Hình ảnh")]
    public string? ImageUrl { get; set; }

    public List<ProductImage>? Images { get; set; }

    [ForeignKey("Category")]
    [DisplayName("Mã danh mục")]
    public int CategoryId { get; set; }

    [DisplayName("Danh mục")]
    public Category? Category { get; set; }

    [DisplayName("Tồn kho")]
    public int StockQuantity { get; set; }

    [DisplayName("Đơn vị tính")]
    public string Unit { get; set; } // Đơn vị tính như "kg", "hộp", "trái"

    public string? UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual IdentityUser? User { get; set; }

    [Required(ErrorMessage = "Link liên hệ là bắt buộc")]
    [Url(ErrorMessage = "Vui lòng nhập một đường link hợp lệ (ví dụ: https://...)")]
    [DisplayName("Link liên hệ (Facebook, Zalo, SĐT)")]
    public string ContactLink { get; set; }

    [DisplayName("Trạng thái duyệt")]
    public ProductApprovalStatus ApprovalStatus { get; set; } = ProductApprovalStatus.ChoDuyet;
}
