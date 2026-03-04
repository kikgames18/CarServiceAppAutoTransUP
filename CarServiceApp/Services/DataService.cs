using CarServiceApp.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace CarServiceApp.Services
{
    public class DataService
    {
        public List<User> Users { get; private set; } = new();
        public List<Request> Requests { get; private set; } = new();
        public List<Comment> Comments { get; private set; } = new();

        private readonly IWebHostEnvironment _env;

        public DataService(IWebHostEnvironment env)
        {
            _env = env;
            LoadData();
        }

        private void LoadData()
        {
            var dataPath = Path.Combine(_env.ContentRootPath, "Data");

            // Конфигурация для CSV с разделителем ";"
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                MissingFieldFound = null // не бросать исключение, если поле отсутствует (но у нас все есть)
            };

            // Загрузка пользователей
            using (var reader = new StreamReader(Path.Combine(dataPath, "inputDataUsers.csv")))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    Users.Add(new User
                    {
                        UserID = csv.GetField<int>("userID"),
                        FIO = csv.GetField("fio"),
                        Phone = csv.GetField("phone"),
                        Login = csv.GetField("login"),
                        Password = csv.GetField("password"),
                        Type = csv.GetField("type")
                    });
                }
            }

            // Загрузка заявок
            using (var reader = new StreamReader(Path.Combine(dataPath, "inputDataRequests.csv")))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    Requests.Add(new Request
                    {
                        RequestID = csv.GetField<int>("requestID"),
                        StartDate = csv.GetField<DateTime>("startDate"),
                        CarType = csv.GetField("carType"),
                        CarModel = csv.GetField("carModel"),
                        ProblemDescription = csv.GetField("problemDescryption"),
                        RequestStatus = csv.GetField("requestStatus"),
                        CompletionDate = csv.GetField<DateTime?>("completionDate"),
                        RepairParts = csv.GetField("repairParts"),
                        MasterID = csv.GetField<int?>("masterID"),
                        ClientID = csv.GetField<int>("clientID")
                    });
                }
            }

            // Загрузка комментариев
            using (var reader = new StreamReader(Path.Combine(dataPath, "inputDataComments.csv")))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    Comments.Add(new Comment
                    {
                        CommentID = csv.GetField<int>("commentID"),
                        Message = csv.GetField("message"),
                        MasterID = csv.GetField<int>("masterID"),
                        RequestID = csv.GetField<int>("requestID")
                    });
                }
            }
        }

        public int GetNextRequestId()
        {
            return Requests.Count > 0 ? Requests.Max(r => r.RequestID) + 1 : 1;
        }

        public int GetNextCommentId()
        {
            return Comments.Count > 0 ? Comments.Max(c => c.CommentID) + 1 : 1;
        }
    }
}