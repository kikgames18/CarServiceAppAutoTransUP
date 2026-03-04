namespace CarServiceApp.Models
{
    public class Comment
    {
        public int CommentID { get; set; }
        public string Message { get; set; }
        public int MasterID { get; set; }
        public int RequestID { get; set; }
    }
}