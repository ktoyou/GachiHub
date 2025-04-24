using GachiHubBackend.Hubs;
using GachiHubBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSingleton<RoomService>();

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 102400; // 100kb
    options.StreamBufferCapacity = 20;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapHub<RoomHub>("/audio");

app.Run();