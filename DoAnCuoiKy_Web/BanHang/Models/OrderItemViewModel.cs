namespace BanHang.Models
{
    public class OrderItemViewModel
    {
        public int OrderId { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }
}
