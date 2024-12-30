using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Thêm thư viện này

namespace BakeryManagement.Models​;

public partial class Cake
{
    [Display(Name = "Mã Bánh")]
    public int CakeId { get; set; }

    [Display(Name = "Tên Bánh")]
    public string Name { get; set; } = null!;

    [Display(Name = "Mô Tả")]
    public string? Description { get; set; }

    [Display(Name = "Giá Bán")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Display(Name = "Loại Bánh")]
    public string? Category { get; set; }

    [Display(Name = "Hình Ảnh")]
    public string? Image { get; set; }

    [Display(Name = "Số Lượng Tồn Kho")]
    public int Stock { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
