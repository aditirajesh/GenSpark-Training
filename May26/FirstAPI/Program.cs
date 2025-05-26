var builder = WebApplication.CreateBuilder(args); //sets up the basic web app - starting point

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer(); //describe all the end-points/routes so that the swagger can use them 
builder.Services.AddSwaggerGen(); //swagger helps show your api in a nice web page
builder.Services.AddControllers();

var app = builder.Build(); //building the actual app

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) //if im just testing my app, then
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();


