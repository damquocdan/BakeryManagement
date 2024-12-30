using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Thêm thư viện này

namespace BakeryManagement.Models​;

public partial class Order
{
    [Display(Name = "Mã Đơn Hàng")]
    public int OrderId { get; set; }

    [Display(Name = "Mã Khách Hàng")]
    public int CustomerId { get; set; }

    [Display(Name = "Mã Nhân Viên")]
    public int? EmployeeId { get; set; }

    [Display(Name = "Ngày Đặt Hàng")]
    [DataType(DataType.Date)]
    public DateTime OrderDate { get; set; }

    [Display(Name = "Ngày Giao Hàng")]
    [DataType(DataType.Date)]
    public DateTime? DeliveryDate { get; set; }

    [Display(Name = "Trạng Thái")]
    public string Status { get; set; } = null!;

    [Display(Name = "Tổng Tiền")]
    [DataType(DataType.Currency)]
    public decimal TotalAmount { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
