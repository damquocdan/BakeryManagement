using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Thêm thư viện này

namespace BakeryManagement.Models​;

public partial class Admin
{
    [Display(Name = "Mã Admin")]
    public int AdminId { get; set; }

    [Display(Name = "Tên Đăng Nhập")]
    public string Username { get; set; } = null!;

    [Display(Name = "Mật Khẩu")]
    public string Password { get; set; } = null!;

    [Display(Name = "Họ và Tên")]
    public string Name { get; set; } = null!;

    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Display(Name = "Số Điện Thoại")]
    public string? Phone { get; set; }

    [Display(Name = "Vai Trò")]
    public string Role { get; set; } = null!;

    [Display(Name = "Ngày Tạo")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Ngày Cập Nhật")]
    public DateTime? UpdatedAt { get; set; }
}
