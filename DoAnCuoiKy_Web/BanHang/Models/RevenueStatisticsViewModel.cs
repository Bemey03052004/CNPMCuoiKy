namespace BanHang.Models
{
    internal class RevenueStatisticsViewModel
    {
        public string FilterType { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }
}