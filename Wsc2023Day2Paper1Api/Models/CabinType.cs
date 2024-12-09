using System;
using System.Collections.Generic;

namespace Wsc2023Day2Paper1Api.Models;

public partial class CabinType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
