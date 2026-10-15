using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Core.Entities;

public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Registered";
    public int? BibNumber { get; set; }
    public string PaymentStatus { get; set; } = "Pending";

    // Navigation properties
    public User Participant { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Event Event { get; set; } = null!;
    public Result? Result { get; set; }
}
