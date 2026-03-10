using Microsoft.EntityFrameworkCore;
using CarServiceApp.Models;

namespace CarServiceApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Имена таблиц
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Request>().ToTable("requests");
            modelBuilder.Entity<Comment>().ToTable("comments");

            // Первичные ключи (указываем, что они генерируются БД)
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserID);

            modelBuilder.Entity<Request>()
                .HasKey(r => r.RequestID);

            modelBuilder.Entity<Comment>()
                .HasKey(c => c.CommentID);

            // Сопоставление свойств с колонками и дополнительные настройки
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.UserID)
                    .HasColumnName("user_id")
                    .ValueGeneratedOnAdd(); // автоинкремент

                entity.Property(u => u.FIO)
                    .HasColumnName("fio")
                    .IsRequired();

                entity.Property(u => u.Phone)
                    .HasColumnName("phone");

                entity.Property(u => u.Login)
                    .HasColumnName("login")
                    .IsRequired();

                entity.Property(u => u.Password)
                    .HasColumnName("password")
                    .IsRequired();

                entity.Property(u => u.Type)
                    .HasColumnName("type")
                    .IsRequired();
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.Property(r => r.RequestID)
                    .HasColumnName("request_id")
                    .ValueGeneratedOnAdd(); // автоинкремент

                entity.Property(r => r.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("date") // явно указываем тип date
                    .IsRequired();

                entity.Property(r => r.CarType)
                    .HasColumnName("car_type");

                entity.Property(r => r.CarModel)
                    .HasColumnName("car_model");

                entity.Property(r => r.ProblemDescription)
                    .HasColumnName("problem_description");

                entity.Property(r => r.RequestStatus)
                    .HasColumnName("request_status");

                entity.Property(r => r.CompletionDate)
                    .HasColumnName("completion_date")
                    .HasColumnType("date"); // явно указываем тип date

                entity.Property(r => r.RepairParts)
                    .HasColumnName("repair_parts");

                entity.Property(r => r.MasterID)
                    .HasColumnName("master_id");

                entity.Property(r => r.ClientID)
                    .HasColumnName("client_id")
                    .IsRequired();

                // Новые поля
                entity.Property(r => r.ExtendedDeadline)
                    .HasColumnName("extended_deadline")
                    .HasColumnType("date"); // явно указываем тип date

                entity.Property(r => r.DeadlineAgreed)
                    .HasColumnName("deadline_agreed")
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(c => c.CommentID)
                    .HasColumnName("comment_id")
                    .ValueGeneratedOnAdd(); // автоинкремент

                entity.Property(c => c.Message)
                    .HasColumnName("message")
                    .IsRequired();

                entity.Property(c => c.MasterID)
                    .HasColumnName("master_id")
                    .IsRequired();

                entity.Property(c => c.RequestID)
                    .HasColumnName("request_id")
                    .IsRequired();
            });

            // Настройка связей (отношений)

            // Связь Request.Client -> User (клиент)
            modelBuilder.Entity<Request>()
                .HasOne(r => r.Client)
                .WithMany(u => u.ClientRequests)
                .HasForeignKey(r => r.ClientID)
                .OnDelete(DeleteBehavior.Restrict); // запрещаем удаление клиента, если есть заявки

            // Связь Request.Master -> User (механик)
            modelBuilder.Entity<Request>()
                .HasOne(r => r.Master)
                .WithMany(u => u.MasterRequests)
                .HasForeignKey(r => r.MasterID)
                .OnDelete(DeleteBehavior.SetNull); // при удалении механика поле MasterID становится NULL

            // Связь Comment.Master -> User (автор комментария)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Master)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.MasterID)
                .OnDelete(DeleteBehavior.Restrict); // запрещаем удаление пользователя, если есть комментарии

            // Связь Comment.Request -> Request (заявка)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Request)
                .WithMany() // у Request нет коллекции комментариев, поэтому WithMany без параметров
                .HasForeignKey(c => c.RequestID)
                .OnDelete(DeleteBehavior.Cascade); // при удалении заявки удаляются все её комментарии
        }
    }
}