using System;
using System.Collections.Generic;

namespace Wsc2023Day2Paper1Api.Models;

public partial class BookingReference
{
    public string Id { get; set; } = null!;

    public decimal TotalAmt { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
