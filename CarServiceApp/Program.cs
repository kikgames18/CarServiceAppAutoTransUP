using CarServiceApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы MVC
builder.Services.AddControllersWithViews();

// Регистрируем DataService как синглтон (загружает CSV и хранит данные в памяти)
builder.Services.AddSingleton<DataService>();

// Настраиваем сессии
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Конфигурация pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // обязательно после UseRouting и до UseAuthorization
app.UseAuthorization();

// Маршрут по умолчанию – сразу на страницу входа
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();