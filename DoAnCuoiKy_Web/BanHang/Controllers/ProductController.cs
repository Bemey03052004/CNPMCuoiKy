using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System;
using Microsoft.AspNetCore.Http;

public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly UserManager<IdentityUser> _userManager;

    public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, UserManager<IdentityUser> userManager)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _userManager = userManager;
    }

    // === ACTION ĐÃ SỬA ===
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        IEnumerable<Product> products;

        if (User.IsInRole("Admin"))
        {
            products = await _productRepository.GetAllAsync();
        }
        else
        {
            products = await _productRepository.GetByUserIdAsync(currentUser.Id);
        }

        return View(products);
    }

    // Hiển thị form thêm sản phẩm mới
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View();
    }

    // === ACTION ĐÃ SỬA ===
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Product product, IFormFile imageUrl, List<IFormFile> additionalImages)
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            product.UserId = currentUser.Id;

            // Ảnh chính
            if (imageUrl != null)
            {
                product.ImageUrl = await SaveImage(imageUrl);
            }

            // Lưu sản phẩm để lấy Id
            await _productRepository.AddAsync(product);

            // === Lưu ảnh phụ nếu có ===
            if (additionalImages != null && additionalImages.Count > 0)
            {
                int count = 0;
                foreach (var img in additionalImages)
                {
                    if (img.Length > 0 && count < 5)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await img.CopyToAsync(stream);
                        }

                        var productImage = new ProductImage
                        {
                            ProductId = product.Id,
                            Url = "/images/" + fileName
                        };

                        _productRepository.AddProductImage(productImage);
                        count++;
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Nếu ModelState không hợp lệ, cần load lại danh mục và trả lại view
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(product); // ✅ Thêm dòng này để tránh lỗi CS0161
    }


    // --- CÁC PHẦN CÒN LẠI GIỮ NGUYÊN VÌ BẠN ĐÃ LÀM ĐÚNG ---

    private async Task<string> SaveImage(IFormFile image)
    {
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }
        return "/images/" + fileName;
    }

    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();
        var currentUser = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Admin") && product.UserId != currentUser.Id) return Forbid();
        return View(product);
    }

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();
        var currentUser = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Admin") && product.UserId != currentUser.Id) return Forbid();
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Edit(int id, Product product, IFormFile imageUrl)
    {
        if (id != product.Id) return NotFound();

        // Lấy lại sản phẩm gốc từ CSDL để kiểm tra quyền và giữ UserId
        var originalProduct = await _productRepository.GetByIdAsync(id);
        if (originalProduct == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Admin") && originalProduct.UserId != currentUser.Id) return Forbid();

        if (ModelState.IsValid)
        {
            // Gán lại UserId để đảm bảo nó không bị mất
            product.UserId = originalProduct.UserId;

            if (imageUrl != null)
            {
                product.ImageUrl = await SaveImage(imageUrl);
            }
            else
            {
                // Giữ lại ảnh cũ nếu không có ảnh mới được tải lên
                product.ImageUrl = originalProduct.ImageUrl;
            }

            await _productRepository.UpdateAsync(product);
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();
        var currentUser = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Admin") && product.UserId != currentUser.Id) return Forbid();
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();
        var currentUser = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Admin") && product.UserId != currentUser.Id) return Forbid();
        await _productRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateApprovalStatus(int id, ProductApprovalStatus approvalStatus)
    {
        await _productRepository.UpdateApprovalStatusAsync(id, approvalStatus);
        return RedirectToAction(nameof(Index));
    }


}