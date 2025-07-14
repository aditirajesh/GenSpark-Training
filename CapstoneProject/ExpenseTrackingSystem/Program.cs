using ExpenseTrackingSystem.Contexts;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Mappers;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Repositories;
using ExpenseTrackingSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AzureBlobStorage; // 🆕 ADD: For Azure Blob Storage logging
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ✅ FIXED: Single, consistent JSON configuration
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // ✅ Handle circular refs properly
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Expense Tracking System API", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// 🆕 ENHANCED: Serilog configuration with Azure Blob Storage support
builder.Host.UseSerilog((context, configuration) =>
{
    var isDevelopment = context.HostingEnvironment.IsDevelopment();
    var azureStorageConnectionString = context.Configuration.GetConnectionString("AzureStorage");

    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "ExpenseTrackingSystem")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("MachineName", Environment.MachineName);

    if (isDevelopment)
    {
        // Pretty console for development
        configuration.WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}" +
            "    💬 {Message:lj}{NewLine}" +
            "    📊 {Properties:j}{NewLine}");
    }
    else
    {
        // Structured for production
        configuration.WriteTo.Console();
    }

    // Keep your existing local file logging
    configuration.WriteTo.File(
        path: "Logs/expense-tracking-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}");

    // 🆕 ADD: Azure Blob Storage logging for real-time cloud storage
    if (!string.IsNullOrEmpty(azureStorageConnectionString))
    {
        try
        {
            configuration.WriteTo.AzureBlobStorage(
                connectionString: azureStorageConnectionString,
                storageContainerName: "log-files",
                storageFileName: "application/{yyyy}/{MM}/{dd}/expense-tracking-{HH}.log",
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}");

            // 🆕 ADD: Separate sink for errors/warnings
            configuration.WriteTo.AzureBlobStorage(
                connectionString: azureStorageConnectionString,
                storageContainerName: "log-files",
                storageFileName: "errors/{yyyy}/{MM}/{dd}/errors-{HH}.log",
                restrictedToMinimumLevel: LogEventLevel.Warning,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}");

            Log.Information("Azure Blob Storage logging configured successfully");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to configure Azure Blob Storage logging, continuing with local logging only");
        }
    }
    else
    {
        Log.Warning("Azure Storage connection string not found, using local logging only");
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200") // Angular dev server
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });

    // More permissive policy for development
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ✅ FIXED: Consistent JSON configuration for controllers
builder.Services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // ✅ Same as above
                    opts.JsonSerializerOptions.WriteIndented = true;
                    opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

builder.Services.AddDbContext<ExpenseContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

#region  Repositories
builder.Services.AddTransient<IRepository<Guid, AuditLog>, AuditRepository>();
builder.Services.AddTransient<IRepository<Guid, Expense>, ExpenseRepository>();
builder.Services.AddTransient<IRepository<Guid, Receipt>, ReceiptRepository>();
builder.Services.AddTransient<IRepository<string, User>, UserRepository>();
#endregion

#region Services
builder.Services.AddTransient<IExpenseService, ExpenseService>();
builder.Services.AddTransient<IReceiptService, ReceiptService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IEncryptionService, EncryptionService>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddTransient<IAuditLogService, AuditLogService>();
builder.Services.AddTransient<IReportService, ReportService>();
#endregion

#region Mappers 
builder.Services.AddSingleton<AuditMapper>();
builder.Services.AddSingleton<ExpenseMapper>();
builder.Services.AddSingleton<ReceiptMapper>();
builder.Services.AddSingleton<UserMapper>();
#endregion

#region AuthenticationFilter
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Keys:JwtTokenKey"]))
                    };
                });
#endregion

#region RateLimiter
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("fixed", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1000,
                Window = TimeSpan.FromHours(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));
});
#endregion

var app = builder.Build();

// 🆕 ENHANCED: More detailed request logging with user context
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null 
        ? LogEventLevel.Error 
        : httpContext.Response.StatusCode > 499 
            ? LogEventLevel.Error 
            : LogEventLevel.Information;
    
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("RequestSize", httpContext.Request.ContentLength ?? 0);
        diagnosticContext.Set("ResponseSize", httpContext.Response.ContentLength ?? 0);
        
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
            diagnosticContext.Set("IsAuthenticated", true);
        }
        else
        {
            diagnosticContext.Set("IsAuthenticated", false);
        }
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCors"); // More permissive for development
}
else
{
    app.UseCors("AllowAngularApp"); // Specific origins for production
}
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting ExpenseTrackingSystem web host with Azure Blob Storage logging enabled");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}