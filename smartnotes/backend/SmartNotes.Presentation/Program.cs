using SmartNotes.Application.Extensions;
using SmartNotes.Infrastructure.Extensions;
using SmartNotes.Presentation.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using SmartNotes.Presentation.Validators;
using Swashbuckle.AspNetCore.Annotations;
using SmartNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateNoteRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SmartNotes API",
        Version = "v1",
        Description = "A RESTful API for managing notes with tags and search functionality",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "SmartNotes Team",
            Email = "support@smartnotes.com"
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add example schemas
    options.EnableAnnotations();
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("SmartNotesPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",    // React development server
                "https://localhost:3000",   // React development server with HTTPS
                "http://localhost:3001",    // Alternative React port
                "https://localhost:3001"    // Alternative React port with HTTPS
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowedToAllowWildcardSubdomains();
    });

    // Add a more restrictive policy for production
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy
            .WithOrigins("https://smartnotes.yourdomain.com") // Replace with actual production domain
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization")
            .AllowCredentials();
    });
});

// Add application layer services
builder.Services.AddApplication();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Run database migrations in production
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SmartNotesDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Use development CORS policy
    app.UseCors("SmartNotesPolicy");
}
else
{
    // Use production CORS policy
    app.UseCors("ProductionPolicy");
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

// Make Program class accessible for integration testing
public partial class Program { }