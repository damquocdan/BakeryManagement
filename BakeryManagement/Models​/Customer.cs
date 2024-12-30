using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Thêm thư viện này

namespace BakeryManagement.Models​;

public partial class Customer
{
    [Display(Name = "Mã Khách Hàng")]
    public int CustomerId { get; set; }

    [Display(Name = "Họ Tên")]
    public string Name { get; set; } = null!;

    [Display(Name = "Số Điện Thoại")]
    [DataType(DataType.PhoneNumber)]
    public string? Phone { get; set; }

    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [Display(Name = "Địa Chỉ")]
    public string? Address { get; set; }

    [Display(Name = "Ngày Sinh")]
    [DataType(DataType.Date)]
    public DateOnly? Birthday { get; set; }

    [Display(Name = "Mật Khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
