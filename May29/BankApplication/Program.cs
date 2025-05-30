using BankApplication.Contexts;
using BankApplication.Models;
using BankApplication.Interfaces;
using Microsoft.EntityFrameworkCore;
using BankApplication.Repositories;
using BankApplication.Misc;
using BankApplication.Services;
using BankApplication.Mappers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                    opts.JsonSerializerOptions.WriteIndented = true;
                });



builder.Services.AddDbContext<BankingContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddTransient<IRepository<int, Account>, AccountRepository>();
builder.Services.AddTransient<IRepository<int, User>, UserRepository>();
builder.Services.AddTransient<IRepository<int, BankTransaction>, BankTransactionRepository>();
builder.Services.AddTransient<IOtherContextFunctionalities, OtherFunctionalitiesImplementation>();

builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IBankTransactionService, BankTransactionService>();

builder.Services.AddSingleton<AccountMapper>();
builder.Services.AddSingleton<UserMapper>();
builder.Services.AddSingleton<BankTransactionMapper>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

app.Run();
