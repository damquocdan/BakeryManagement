using System;
using System.Collections.Generic;

namespace BakeryManagement.Models​;

public partial class Cart
{
    public int CartId { get; set; }

    public int CustomerId { get; set; }

    public int CakeId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual Cake Cake { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;
}
