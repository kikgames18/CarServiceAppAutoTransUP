using System;

namespace CarServiceApp.Models
{
    public class Request
    {
        public int RequestID { get; set; }
        public DateTime StartDate { get; set; }
        public string? CarType { get; set; }
        public string? CarModel { get; set; }
        public string? ProblemDescription { get; set; }
        public string? RequestStatus { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? RepairParts { get; set; }  // было string, стало string?
        public int? MasterID { get; set; }
        public int ClientID { get; set; }

        // Навигационные свойства
        public User? Client { get; set; }
        public User? Master { get; set; }
    }
}