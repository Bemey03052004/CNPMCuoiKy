// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace BanHang.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {

            [Required]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Quyền")]
            public string Role { get; set; }
        }

        // Dùng để View kiểm tra và hiển thị dropdown
        public bool IsAdmin { get; set; }
        public List<SelectListItem> RoleList { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            IsAdmin = User.Identity.IsAuthenticated && User.IsInRole("Admin");

            if (IsAdmin)
            {
                // Lấy danh sách Role từ database
                RoleList = _roleManager.Roles.Select(x => new SelectListItem { Value = x.Name, Text = x.Name }).ToList();
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            IsAdmin = User.Identity.IsAuthenticated && User.IsInRole("Admin");

            if (ModelState.IsValid)
            {
                // === SỬA LỖI TẠI ĐÂY ===
                // Tạo user mới, sử dụng Email cho cả UserName và Email.
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email, PhoneNumber = Input.PhoneNumber };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Logic gán Role
                    string roleToAssign = (IsAdmin && !string.IsNullOrEmpty(Input.Role)) ? Input.Role : "User";

                    if (await _roleManager.RoleExistsAsync(roleToAssign))
                    {
                        await _userManager.AddToRoleAsync(user, roleToAssign);
                    }
                    else
                    {
                        _logger.LogWarning($"Role '{roleToAssign}' does not exist. Assigning default 'User' role.");
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    // Logic gửi email xác nhận
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code, returnUrl },
                        protocol: Request.Scheme);
                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }
                    else
                    {
                        if (!IsAdmin)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            TempData["SuccessMessage"] = $"Tài khoản {user.Email} đã được tạo thành công với vai trò {roleToAssign}.";
                            return RedirectToAction("User", "Admin");
                        }
                    }
                }

                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                    {
                        ModelState.AddModelError(string.Empty, $"Email '{Input.Email}' đã được sử dụng.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // Nếu có lỗi, load lại RoleList cho admin và hiển thị lại trang
            if (IsAdmin)
            {
                RoleList = _roleManager.Roles.Select(x => new SelectListItem { Value = x.Name, Text = x.Name }).ToList();
            }
            return Page();
        }

        // CreateUser và GetEmailStore không cần thiết nữa vì chúng ta đã dùng constructor để inject
        // và tạo user trực tiếp
    }
}