using System;
using System.Collections.Generic;

namespace Wsc2023Day2Paper1Api.Models;

public partial class Ticket
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public int CabinTypeId { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string? Email { get; set; }

    public string Phone { get; set; } = null!;

    public string PassportNumber { get; set; } = null!;

    public int PassportCountryId { get; set; }

    public string BookingReference { get; set; } = null!;

    public bool Confirmed { get; set; }

    public string? SeatNo { get; set; }

    public int TicketTypeId { get; set; }

    public virtual BookingReference BookingReferenceNavigation { get; set; } = null!;

    public virtual CabinType CabinType { get; set; } = null!;

    public virtual Schedule Schedule { get; set; } = null!;

    public virtual TicketType TicketType { get; set; } = null!;
}
