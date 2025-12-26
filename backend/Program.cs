using SignalRDemo.Hubs;
using SignalRDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Register the LLM Response Background Service
builder.Services.AddSingleton<LLMResponseService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<LLMResponseService>());

// Add CORS policy for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("ReactApp");

app.UseAuthorization();

app.MapControllers();

// Map SignalR hub
app.MapHub<ChatHub>("/chathub");

app.Run();
