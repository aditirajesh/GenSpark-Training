using BlobApplication.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddScoped<BlobStorageService>();

var app = builder.Build();

app.Urls.Add("http://0.0.0.0:8080");


// Configure the HTTP request pipeline.
// Enable Swagger in both Development AND Production for Docker
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add a default route for the root path
app.MapGet("/", () => new
{
    message = "BlobApplication API is running!",
    version = "1.0.0",
    swagger = "/swagger",
    endpoints = new[]
    {
        "/swagger - API Documentation",
        "/api/[controller] - Your API endpoints"
    }
});

// Add a health check endpoint
app.MapGet("/health", () => new { status = "Healthy", timestamp = DateTime.UtcNow });

app.UseAuthorization();

app.MapControllers();

app.Run();