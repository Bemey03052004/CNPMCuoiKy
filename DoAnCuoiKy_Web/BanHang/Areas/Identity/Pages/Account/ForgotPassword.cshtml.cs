// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using BanHang.Services;

namespace BanHang.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
		private readonly EmailSender _emailSender;

		public ForgotPasswordModel(UserManager<IdentityUser> userManager, EmailSender emailSender)
		{
			_userManager = userManager;
			_emailSender = emailSender;
		}


		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null /*|| !(await _userManager.IsEmailConfirmedAsync(user))*/)
                {
                    // Không tiết lộ rằng tài khoản không tồn tại hoặc chưa xác nhận
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Tạo mã đặt lại mật khẩu
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                // Tạo URL đặt lại mật khẩu
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                try
                {
                    // Gửi email đặt lại mật khẩu
                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Yêu cầu đặt lại mật khẩu",
                        $@"
                    <div style='font-family: Arial, sans-serif; color: #333;'>
                        <p style='font-size: 16px;'>Xin chào,</p>
                        <p style='font-size: 16px;'>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Để đặt lại mật khẩu, vui lòng nhấp vào liên kết dưới đây:</p>
                        <p style='font-size: 16px;'>
                            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='color: #007bff; text-decoration: none;'>Nhấp vào đây để đặt lại mật khẩu</a>
                        </p>
                        <p style='font-size: 16px;'>Chú ý: Nếu bạn không yêu cầu thay đổi mật khẩu, vui lòng bỏ qua email này. Mật khẩu của bạn sẽ không bị thay đổi.</p>
                        <p style='font-size: 16px;'>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.</p>
                        <p style='font-size: 16px;'>Trân trọng, <br/> Đội ngũ hỗ trợ</p>
                    </div>");
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi gửi email (nếu cần)
                    Console.WriteLine($"Error sending email: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi gửi email. Vui lòng thử lại.");
                    return Page();
                }

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }

    }
}
