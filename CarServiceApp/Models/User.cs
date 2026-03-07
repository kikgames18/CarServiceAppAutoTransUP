using System.Collections.Generic;

namespace CarServiceApp.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FIO { get; set; }
        public string? Phone { get; set; }  // добавлен знак вопроса, так как в БД может быть NULL
        public string Login { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }

        // Навигационные свойства
        public ICollection<Request>? ClientRequests { get; set; }
        public ICollection<Request>? MasterRequests { get; set; }
        public ICollection<Comment>? Comments { get; set; }
    }
}