using System;
using System.Collections.Generic;

namespace Wsc2023Day2Paper1Api.Models;

public partial class Country
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Airport> Airports { get; set; } = new List<Airport>();
}
