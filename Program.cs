using GachiHubBackend.Hubs;
using GachiHubBackend.Services;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo { Title = "GachiHubBackend", Version = "v1" });
    opts.AddSignalRSwaggerGen();
});
builder.Services.AddControllers();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<UserService>();

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 4194304; // 100kb
    options.StreamBufferCapacity = 20;
    options.EnableDetailedErrors = true;
});

builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(b =>
    {
        b.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();
app.UseCors();
app.MapHub<RoomHub>("/room");

app.Run();