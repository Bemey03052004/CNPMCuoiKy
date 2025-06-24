// Trong file: Models/UserProfileViewModel.cs
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

public class UserProfileViewModel
{
    // Thông tin của người dùng mà chúng ta đang xem profile
    public IdentityUser UserProfile { get; set; }

    // Danh sách các sản phẩm mà người dùng này đã đăng
    public IEnumerable<Product> UserProducts { get; set; }
}