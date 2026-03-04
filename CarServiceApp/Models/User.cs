namespace CarServiceApp.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FIO { get; set; }
        public string Phone { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Type { get; set; } // Менеджер, Автомеханик, Оператор, Заказчик
    }
}