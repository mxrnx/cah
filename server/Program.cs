using Microsoft.EntityFrameworkCore;
using Server;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// Add services
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddTransient<CardParseService>();

builder.Services.AddDbContext<CahContext>(opt =>
    opt.UseInMemoryDatabase("CAH"));

// Set up Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(corsPolicyBuilder =>
{
    corsPolicyBuilder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
});

Console.WriteLine(app.Environment.IsDevelopment());

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
