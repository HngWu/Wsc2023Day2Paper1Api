using Microsoft.AspNetCore.Mvc;
using Wsc2023Day2Paper1Api.Models;

namespace Wsc2023Day2Paper1Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AirlinesController : Controller
    {
        Wsc2023Day2Paper1Context context = new Wsc2023Day2Paper1Context();

        public class tempSchedule
        {
            public int Id { get; set; }

            public string Date { get; set; }

            public string Time { get; set; }

            public int AircraftId { get; set; }

            public int RouteId { get; set; }

            public Double EconomyPrice { get; set; }

            public bool Confirmed { get; set; }

            public string? FlightNumber { get; set; }
            public string departureIata { get; set; }
            public string departureDate { get; set; }
            public string departureTime { get; set; }

            public string arrivalDate { get; set; }
            public string arrivalTime { get; set; }
            public string TravelingTime { get; set; }

        }

        public class bookingDetails
        {
            public string BookingReference { get; set; }
            public Double totalCost { get; set; }
            public Double EconomyPrice { get; set; }
            public string departureIata { get; set; }
            public string departureDate { get; set; }
            public string departureTime { get; set; }

            public string arrivalDate { get; set; }
            public string arrivalTime { get; set; }

            public string totalTravelingTime { get; set; }
            public List<ticketResponse> ticket { get; set; }
            public Double businessCapacity { get; set; }
            public Double economyCapacity { get; set; }
            public List<tempSchedule> schedule { get; set; }

        }


        [HttpPost("postprice/{bookingReference}/{totalPrice}")]
        public IActionResult PostTotalPrice(string bookingReference, double totalPrice)
        {
            try
            {
                var reference = context.BookingReferences.Where(x => x.Id == bookingReference).FirstOrDefault();

                if(reference != null)
                {
                    reference.TotalAmt = (decimal)totalPrice;
                    context.SaveChanges();
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }



        [HttpGet("bookingdetails/{bookingReference}")]
        public IActionResult GetBookingDetails(string bookingReference)
        {
            try
            {
                var tickets = context.Tickets.Where(x => x.BookingReference == bookingReference).Select(x => new ticketResponse
                {
                    Id = x.Id,
                    ScheduleId = x.ScheduleId,
                    CabinTypeId = x.CabinTypeId,
                    Firstname = x.Firstname,
                    Lastname = x.Lastname,
                    Email = x.Email,
                    Phone = x.Phone,
                    PassportNumber = x.PassportNumber,
                    PassportCountryId = x.PassportCountryId,
                    BookingReference = x.BookingReference,
                    Confirmed = x.Confirmed,
                    SeatNo = x.SeatNo,
                    TicketTypeId = x.TicketTypeId
                }).ToList();

                var schedules = tickets.Select(x => x.ScheduleId).Distinct().ToList();
                var scheduleList = context.Schedules.Where(x => schedules.Contains(x.Id))
                    .Select(x=> new tempSchedule
                    {
                        Id = x.Id,
                        Date = x.Date.ToString(),
                        Time = x.Time.ToString(),
                        AircraftId = x.AircraftId,
                        RouteId = x.RouteId,
                        EconomyPrice = (Double)x.EconomyPrice,
                        Confirmed = x.Confirmed,
                        FlightNumber = x.FlightNumber,
                        departureIata = x.Route.DepartureAirport.Iatacode,
                        departureDate = x.Date.ToString(),
                        departureTime = x.Time.ToString(),
                        arrivalDate = x.Date.ToString(),
                        arrivalTime = x.Time.Add(TimeSpan.FromMinutes(x.Route.FlightTime)).ToString(),
                        TravelingTime = x.Route.FlightTime.ToString()

                    })
                    .OrderBy(x=>x.Date)
                    .ThenBy(x=>x.Time)
                    .ToList();



                

                var scheduleId = tickets[0].ScheduleId;
                var maxbusinessCapacity = context.Schedules.Where(x => x.Id == tickets[0].ScheduleId).Select(x => x.Aircraft.BusinessSeats).FirstOrDefault();
                var maxeconomyCapacity = context.Schedules.Where(x => x.Id == tickets[0].ScheduleId).Select(x => x.Aircraft.EconomySeats).FirstOrDefault();


                var numberofbusinessSeats = context.Tickets.Where(x => x.CabinTypeId == 2 && x.ScheduleId == scheduleId).Count();
                var numberofeconomySeats = context.Tickets.Where(x => x.CabinTypeId == 1 && x.ScheduleId == scheduleId).Count();


                var businessCapacity = (int)(((decimal)numberofbusinessSeats / (decimal)maxbusinessCapacity) * 100);
                var economyCapacity = (int)(((decimal)numberofeconomySeats / (decimal)maxeconomyCapacity) * 100);

                

                var totalPrice = context.BookingReferences.Where(x => x.Id == bookingReference).Select(x => x.TotalAmt).FirstOrDefault();

                Double economyPrice = context.Schedules.Where(x => x.Id == tickets[0].ScheduleId).Select(x => (double)x.EconomyPrice).FirstOrDefault();
                var bookingDetails = new bookingDetails
                {
                    BookingReference = bookingReference,
                    totalCost = (double)totalPrice,
                    EconomyPrice = economyPrice,
                    departureIata = context.Schedules.Where(x => x.Id == tickets[0].ScheduleId).Select(x => x.Route.DepartureAirport.Iatacode).FirstOrDefault(),
                    departureDate = context.Schedules.Where(x => x.Id == tickets[0].ScheduleId).Select(x => x.Date).FirstOrDefault().ToString(),
                    arrivalDate = context.Schedules.Where(x => x.Id == tickets[0].ScheduleId).Select(x => x.Date).FirstOrDefault().ToString(),
                    arrivalTime = context.Schedules.Where(x => x.Id == tickets[0].ScheduleId).Select(x => x.Time.Add(TimeSpan.FromMinutes(x.Route.FlightTime))).FirstOrDefault().ToString(),
                    departureTime = context.Schedules.Where(x => x.Id == tickets[0].ScheduleId).Select(x => x.Time).FirstOrDefault().ToString(),
                    totalTravelingTime = context.Schedules.Where(x => x.Id == tickets[0].ScheduleId).Select(x => x.Route.FlightTime).FirstOrDefault().ToString(),
                    ticket = tickets,
                    businessCapacity = businessCapacity,
                    economyCapacity = economyCapacity,
                    schedule = scheduleList
                };  

                return Ok(bookingDetails);
            }
            catch (Exception)
            {

                return NotFound();
            }
        }


        public class ConfirmTicket
        {
            public int ticketId { get; set; }
            public string seatNumber { get; set; }
        }
        [HttpGet("getbookedtickets/{bookingReference}")]
        public IActionResult GetBookedTickets(string bookingReference)
        {
            try
            {
                var tickets = context.Tickets.Where(x => x.BookingReference == bookingReference && x.Confirmed == false).Select(x => new ticketResponse
                {
                    Id = x.Id,
                    ScheduleId = x.ScheduleId,
                    CabinTypeId = x.CabinTypeId,
                    Firstname = x.Firstname,
                    Lastname = x.Lastname,
                    Email = x.Email,
                    Phone = x.Phone,
                    PassportNumber = x.PassportNumber,
                    PassportCountryId = x.PassportCountryId,
                    BookingReference = x.BookingReference,
                    Confirmed = x.Confirmed,
                    SeatNo = x.SeatNo,
                    TicketTypeId = x.TicketTypeId
                }).ToList();

                if (tickets.Select(x => x.ScheduleId).Distinct().Count() >= 4)
                {
                    tickets = tickets.Take(tickets.Count / 4).ToList();
                }
                else if (tickets.Select(x => x.ScheduleId).Distinct().Count() >= 2)
                {
                    tickets = tickets.Take(tickets.Count / 2).ToList();
                }
                var scheduleID = tickets.Select(x => x.ScheduleId).Distinct().FirstOrDefault();

                var BookedTickets = context.Tickets.Where(x => x.ScheduleId == scheduleID && x.Confirmed == true).Select(x => new ticketResponse
                {
                    Id = x.Id,
                    ScheduleId = x.ScheduleId,
                    CabinTypeId = x.CabinTypeId,
                    Firstname = x.Firstname,
                    Lastname = x.Lastname,
                    Email = x.Email,
                    Phone = x.Phone,
                    PassportNumber = x.PassportNumber,
                    PassportCountryId = x.PassportCountryId,
                    BookingReference = x.BookingReference,
                    Confirmed = x.Confirmed,
                    SeatNo = x.SeatNo,
                    TicketTypeId = x.TicketTypeId
                }).ToList();



                return Ok(BookedTickets);
            }
            catch (Exception)
            {

                return NotFound();
            }



        }

        [HttpPost("confirmticket")]
        public IActionResult ConfirmTickets(List<ConfirmTicket> confirmTicketList)
        {
            try
            {
                foreach(var confirmTicket in confirmTicketList)
                {
                    var ticket = context.Tickets.Find(confirmTicket.ticketId);
                    ticket.Confirmed = true;
                    ticket.SeatNo = confirmTicket.seatNumber;
                    context.Tickets.Update(ticket);

                }

                context.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {

                return NotFound();
            }
        }

        public class ticketResponse
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
        }

        [HttpGet("gettickets/{bookingReference}")]
        public IActionResult GetTickets(string bookingReference)
        {
            try
            {
                var tickets = context.Tickets.Where(x => x.BookingReference == bookingReference && x.Confirmed == false).Select(x=> new ticketResponse
                {
                    Id = x.Id,
                    ScheduleId = x.ScheduleId,
                    CabinTypeId = x.CabinTypeId,
                    Firstname = x.Firstname,
                    Lastname = x.Lastname,
                    Email = x.Email,
                    Phone = x.Phone,
                    PassportNumber = x.PassportNumber,
                    PassportCountryId = x.PassportCountryId,
                    BookingReference = x.BookingReference,
                    Confirmed = x.Confirmed,
                    SeatNo = x.SeatNo,
                    TicketTypeId = x.TicketTypeId
                }).ToList();

                var ticketsToCheck = context.Tickets.Where(x => x.BookingReference == bookingReference).ToList();

                foreach (var ticket in ticketsToCheck)
                {
                    if (ticket.Confirmed == true)
                    {
                        return Ok(new List<object>());
                    }
                }

                var scheduleID = tickets.Select(x => x.ScheduleId).FirstOrDefault();


                if (tickets.Select(x => x.ScheduleId).Distinct().Count() >= 4)
                {
                    tickets = tickets.Take(tickets.Count / 4).ToList();
                }
                else if (tickets.Select(x => x.ScheduleId).Distinct().Count() >= 2)
                {
                    tickets = tickets.Take(tickets.Count / 2).ToList();
                }


                return Ok(tickets);
            }
            catch (Exception)
            {

                return NotFound();
            }
            


        }




        public class TempTicket
        {
            public int ScheduleId { get; set; }
            public string CabinType { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string PassportNumber { get; set; }
            public string PassportCountryId { get; set; }
            public string TicketType { get; set; }
            public Boolean? IsTransfer { get; set; }
            public int? TransferScheduleId { get; set; }
        }

        [HttpPost("tickets")]
        public IActionResult bookTicket(List<TempTicket> ticketsList)
        {
            try
            {
                var bookingReference = new BookingReference
                {
                    Id = DateTime.Now.ToString("ddMMyyyyHHmmssff")
                };
                context.BookingReferences.Add(bookingReference);

                foreach (var ticket in ticketsList)
                {
                    var ticketType = context.CabinTypes.Where(x => x.Name == ticket.CabinType).FirstOrDefault();
                    var schedule = context.Schedules.Find(ticket.ScheduleId);
                    var ticketType1 = context.TicketTypes.Where(x => x.Name == ticket.TicketType).FirstOrDefault();
                    var countryid = context.Countries.Where(x => x.Name == ticket.PassportCountryId).FirstOrDefault().Id;


                    var ticket1 = new Ticket
                    {
                        ScheduleId = ticket.ScheduleId,
                        CabinTypeId = ticketType.Id,
                        Firstname = ticket.FirstName,
                        Lastname = ticket.LastName,
                        Email = ticket.Email,
                        Phone = ticket.Phone,
                        PassportNumber = ticket.PassportNumber,
                        PassportCountryId = countryid,
                        BookingReference = bookingReference.Id,
                        Confirmed = false,
                        TicketTypeId = ticketType1.Id
                    };
                    context.Tickets.Add(ticket1);


                    if (ticket.IsTransfer == true)
                    {
                        var transferTicket = new Ticket
                        {
                            ScheduleId = (int)ticket.TransferScheduleId,
                            CabinTypeId = ticketType.Id,
                            Firstname = ticket.FirstName,
                            Lastname = ticket.LastName,
                            Email = ticket.Email,
                            Phone = ticket.Phone,
                            PassportNumber = ticket.PassportNumber,
                            PassportCountryId = countryid,
                            BookingReference = bookingReference.Id,
                            Confirmed = false,
                            TicketTypeId = ticketType1.Id
                        };
                        context.Tickets.Add(transferTicket);
                    }




                }
                context.SaveChanges();
                return Ok(bookingReference.Id);
            }
            catch (Exception)
            {

                return NotFound();
            }
        }


        [HttpPost("getdepartureflights")]
        public IActionResult getDepartureFlights(searchQuery searchQuery)
        {
            searchQuery.from = searchQuery.from.Split("-")[0];
            searchQuery.to = searchQuery.to.Split("-")[0];
            try
            {
                var flights = context.Schedules
                     .Where(s => s.Date.ToString() == searchQuery.departDate &&
                             s.Route.DepartureAirport.Name == searchQuery.from &&
                             s.Route.ArrivalAirport.Name == searchQuery.to &&
                             s.Aircraft.TotalSeats >= searchQuery.noOfAdults + searchQuery.noOfChildren &&
                             s.Tickets.Count < s.Aircraft.TotalSeats + searchQuery.noOfAdults + searchQuery.noOfChildren)
                     .Select(x => new
                     ScheduleResponse
                     {
                         id = x.Id,
                         Date = x.Date,
                         DepartureIata = x.Route.DepartureAirport.Iatacode,
                         ArrivalAirIata = x.Route.ArrivalAirport.Iatacode,
                         departureTime = x.Time,
                         arrivalTime = x.Time.Add(TimeSpan.FromMinutes(x.Route.FlightTime)),
                         Duration = x.Route.FlightTime,
                         Price = x.EconomyPrice
                     })
                         .ToList();


                var transferFlights = context.Schedules.Where(s =>
                    s.Route.ArrivalAirport.Name == searchQuery.to &&
                    s.Aircraft.TotalSeats >= searchQuery.noOfAdults + searchQuery.noOfChildren &&
                    s.Tickets.Count < s.Aircraft.TotalSeats + searchQuery.noOfAdults + searchQuery.noOfChildren 
                ).ToList();


                foreach (var transferFlight in transferFlights)
                {
                    var start = transferFlight.Time;
                    var arrivalAirportiata = context.Routes.Where(x => x.Id == transferFlight.RouteId).Select(x => x.DepartureAirport.Iatacode).FirstOrDefault();
                    var transferDateTime = transferFlight.Date.ToDateTime(transferFlight.Time);
                    var departureAirportiata = context.Airports.Where(x => x.Name == searchQuery.from).Select(x => x.Iatacode).FirstOrDefault();

                    var StartFlights = context.Schedules
                        .Select(x => new
                        {
                            x.Id,
                            x.Date,
                            departureAirport = x.Route.DepartureAirport.Iatacode,
                            arrivalAirport = x.Route.ArrivalAirport.Iatacode,
                            x.Time,
                            x.Route.FlightTime,
                            x.EconomyPrice,
                            x.Tickets,
                            x.Aircraft

                        })
                        .Where(s =>
                                    s.departureAirport == departureAirportiata &&
                                    s.arrivalAirport == arrivalAirportiata &&
                                    s.Date.ToString() == searchQuery.departDate &&
                                    s.Aircraft.TotalSeats >= searchQuery.noOfAdults + searchQuery.noOfChildren &&
                                    s.Tickets.Count < s.Aircraft.TotalSeats + searchQuery.noOfAdults + searchQuery.noOfChildren)
                                    .OrderBy(x => x.Time)
                                    .ToList();

                    if (StartFlights.Count() > 0)
                    {
                        foreach (var StartFlight in StartFlights)
                        {
                            var startFlightDateTime = StartFlight.Date.ToDateTime(StartFlight.Time);
                            if (startFlightDateTime < transferDateTime)
                            {
                                var transferFlightData = context.Schedules.Where(x=>x.Id == transferFlight.Id)
                                    .Select(x=> new
                                    {
                                        x.Date,
                                        DepartureAirport= x.Route.DepartureAirport.Iatacode,
                                        ArrivalAirport = x.Route.ArrivalAirport.Iatacode,
                                        x.Time,
                                        x.Route.FlightTime,
                                        x.EconomyPrice
                                    }).FirstOrDefault();



                                var newFlight = new ScheduleResponse
                                {
                                    id = StartFlight.Id,
                                    Date = StartFlight.Date,
                                    DepartureIata = StartFlight.departureAirport,
                                    ArrivalAirIata = StartFlight.arrivalAirport,
                                    departureTime = StartFlight.Time,
                                    arrivalTime = StartFlight.Time.Add(TimeSpan.FromMinutes(StartFlight.FlightTime)),
                                    Duration = StartFlight.FlightTime,
                                    Price = StartFlight.EconomyPrice,
                                    isTransfer = true,
                                    transferScheduleId = transferFlight.Id,
                                    TransferDate = transferFlightData.Date,
                                    transferDepartureIata = transferFlightData.DepartureAirport,
                                    transferArrivalIata = transferFlightData.ArrivalAirport,
                                    transferDepartureTime = transferFlightData.Time,
                                    transferArrivalTime = transferFlightData.Time.Add(TimeSpan.FromMinutes(transferFlightData.FlightTime)),
                                    transferDuration = transferFlightData.FlightTime,
                                    transferPrice = transferFlightData.EconomyPrice

                                };

                                var flightExists = flights.Where(x => x.id == newFlight.id).Any();
                                if (flightExists)
                                {
                                    var flightStartTime = flights.Where(x => x.id == newFlight.id).Select(x => x.departureTime).FirstOrDefault();
                                    var newFlightStartTime = newFlight.departureTime;
                                    if (newFlightStartTime < flightStartTime)
                                    {
                                        flights.Remove(flights.Where(x => x.id == newFlight.id).FirstOrDefault());
                                        flights.Add(newFlight);
                                    }



                                }


                                flights.Add(newFlight);
                            }



                        }



                    }
                }
                return Ok(flights.DistinctBy(x => x.id));

            }
            catch (Exception)
            {

                return NotFound();
            }
        }


        [HttpPost("getreturnflights")]
        public IActionResult getReturnFlights(searchQuery searchQuery)
        {
            try
            {
                searchQuery.from = searchQuery.from.Split("-")[0];
                searchQuery.to = searchQuery.to.Split("-")[0];
                var flights = context.Schedules
                    .Where(s => s.Date.ToString() == searchQuery.returnDate &&
                            s.Route.DepartureAirport.Name == searchQuery.to &&
                            s.Route.ArrivalAirport.Name == searchQuery.from &&
                            s.Aircraft.TotalSeats >= searchQuery.noOfAdults + searchQuery.noOfChildren &&
                            s.Tickets.Count < s.Aircraft.TotalSeats + searchQuery.noOfAdults + searchQuery.noOfChildren)
                    .Select(x=> new
                    ScheduleResponse
                    {
                        id = x.Id,
                        Date = x.Date,
                        DepartureIata = x.Route.DepartureAirport.Iatacode,
                        ArrivalAirIata = x.Route.ArrivalAirport.Iatacode,
                        departureTime = x.Time,
                        arrivalTime = x.Time.Add(TimeSpan.FromMinutes(x.Route.FlightTime)),
                        Duration = x.Route.FlightTime,
                        Price = x.EconomyPrice
                    })
                        .ToList();


                var transferFlights = context.Schedules.Where(s =>
                    s.Route.ArrivalAirport.Name == searchQuery.from &&
                    s.Aircraft.TotalSeats >= searchQuery.noOfAdults + searchQuery.noOfChildren &&
                    s.Tickets.Count < s.Aircraft.TotalSeats + searchQuery.noOfAdults + searchQuery.noOfChildren
                ).ToList();


                foreach (var transferFlight in transferFlights)
                {
                    var start = transferFlight.Time;
                    var arrivalAirportiata = context.Routes.Where(x => x.Id == transferFlight.RouteId).Select(x => x.DepartureAirport.Iatacode).FirstOrDefault();
                    var transferDateTime = transferFlight.Date.ToDateTime(transferFlight.Time);
                    var departureAirportiata = context.Airports.Where(x => x.Name == searchQuery.to).Select(x => x.Iatacode).FirstOrDefault();

                    var StartFlights = context.Schedules
                        .Select(x => new
                        {
                            x.Id,
                            x.Date,
                            departureAirport = x.Route.DepartureAirport.Iatacode,
                            arrivalAirport = x.Route.ArrivalAirport.Iatacode,
                            x.Time,
                            x.Route.FlightTime,
                            x.EconomyPrice,
                            x.Tickets,
                            x.Aircraft

                        })
                        .Where(s =>
                                    s.departureAirport == departureAirportiata &&
                                    s.arrivalAirport == arrivalAirportiata &&
                                    s.Date.ToString() == searchQuery.returnDate &&
                                    s.Aircraft.TotalSeats >= searchQuery.noOfAdults + searchQuery.noOfChildren &&
                                    s.Tickets.Count < s.Aircraft.TotalSeats + searchQuery.noOfAdults + searchQuery.noOfChildren)
                                    .OrderBy(x => x.Time)
                                    .ToList();

                    if (StartFlights.Count() > 0)
                    {
                        foreach (var StartFlight in StartFlights)
                        {
                            var startFlightDateTime = StartFlight.Date.ToDateTime(StartFlight.Time);
                            if (startFlightDateTime < transferDateTime)
                            {
                                var transferFlightData = context.Schedules.Where(x=>x.Id == transferFlight.Id)
                                    .Select(x=> new
                                    {
                                        x.Date,
                                        DepartureAirport= x.Route.DepartureAirport.Iatacode,
                                        ArrivalAirport = x.Route.ArrivalAirport.Iatacode,
                                        x.Time,
                                        x.Route.FlightTime,
                                        x.EconomyPrice
                                    }).FirstOrDefault();



                                var newFlight = new ScheduleResponse
                                {
                                    id = StartFlight.Id,
                                    Date = StartFlight.Date,
                                    DepartureIata = StartFlight.departureAirport,
                                    ArrivalAirIata = StartFlight.arrivalAirport,
                                    departureTime = StartFlight.Time,
                                    arrivalTime = StartFlight.Time.Add(TimeSpan.FromMinutes(StartFlight.FlightTime)),
                                    Duration = StartFlight.FlightTime,
                                    Price = StartFlight.EconomyPrice,
                                    isTransfer = true,
                                    transferScheduleId = transferFlight.Id,
                                    TransferDate = transferFlightData.Date,
                                    transferDepartureIata = transferFlightData.DepartureAirport,
                                    transferArrivalIata = transferFlightData.ArrivalAirport,
                                    transferDepartureTime = transferFlightData.Time,
                                    transferArrivalTime = transferFlightData.Time.Add(TimeSpan.FromMinutes(transferFlightData.FlightTime)),
                                    transferDuration = transferFlightData.FlightTime,
                                    transferPrice = transferFlightData.EconomyPrice

                                };

                                flights.Add(newFlight);
                            }



                        }



                    }
                }
                return Ok(flights.DistinctBy(x => x.id));

            }
            catch (Exception)
            {

                return NotFound();
            }
        }

        [HttpGet("countries")]
        public IActionResult GetCountries()
        {
            try
            {
                var countries = context.Countries.Select(x => x.Name).OrderBy(x => x).ToList();
                return Ok(countries);
            }
            catch (Exception)
            {

                return NotFound();
            }
        }


        [HttpGet("airports")]
        public IActionResult getAirports()
        {
            try
            {
                var airports = context.Airports.Select(x=>x.Name  + "-(" + x.Iatacode + ")" + " (" + x.Country.Name + ")").OrderBy(x=>x).ToList();
                return Ok(airports);
            }
            catch (Exception)
            {

                return NotFound();
            }
        }

    }
    public class ScheduleResponse
    {
        public int id { get; set; }
        public DateOnly Date { get; set; }
        public string DepartureIata { get; set; }
        public string ArrivalAirIata { get; set; }
        public TimeOnly departureTime { get; set; }
        public TimeOnly arrivalTime { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }


        public Boolean isTransfer { get; set; }
        public int? transferScheduleId { get; set; }
        public DateOnly? TransferDate { get; set; }
        public string? transferDepartureIata { get; set; }
        public string? transferArrivalIata { get; set; }
        public TimeOnly? transferDepartureTime { get; set; }
        public TimeOnly transferArrivalTime { get; set; }
        public int? transferDuration { get; set; }
        public decimal? transferPrice { get; set; }
    }


    public class searchQuery
    {
        public string from { get; set; }
        public string to { get; set; }
        public string departDate { get; set; }
        public string? returnDate { get; set; }
        public int noOfAdults { get; set; } 
        public int noOfChildren { get; set; }
    }
}
