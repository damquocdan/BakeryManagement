using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Thêm thư viện này

namespace BakeryManagement.Models​;

public partial class Employee
{
    [Display(Name = "Mã Nhân Viên")]
    public int EmployeeId { get; set; }

    [Display(Name = "Họ Tên")]
    public string Name { get; set; } = null!;

    [Display(Name = "Số Điện Thoại")]
    [DataType(DataType.PhoneNumber)]
    public string? Phone { get; set; }

    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [Display(Name = "Chức Vụ")]
    public string? Position { get; set; }

    [Display(Name = "Mức Lương")]
    [DataType(DataType.Currency)]
    public decimal Salary { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
