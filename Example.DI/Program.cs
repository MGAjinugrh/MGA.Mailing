using Example.DI.Services;
using MGA.Mailing;

var builder = WebApplication.CreateBuilder(args);

// 1. Register mailing library from configuration
builder.Services.AddMailing(builder.Configuration, "Smtp");

// 2. Register your own app services
builder.Services.AddScoped<MailService>();

builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();
app.Run();
