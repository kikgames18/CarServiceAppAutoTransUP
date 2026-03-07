namespace CarServiceApp.Models
{
    public class Comment
    {
        public int CommentID { get; set; }
        public string Message { get; set; }  // NOT NULL в БД
        public int MasterID { get; set; }
        public int RequestID { get; set; }

        // Навигационные свойства
        public User? Master { get; set; }
        public Request? Request { get; set; }
    }
}