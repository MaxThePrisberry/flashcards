using Flashcards.APIs;
using Flashcards.APIs.Exceptions;
using Flashcards.APIs.Responses;
using Flashcards.APIs.Services.Decks;
using Flashcards.APIs.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var details = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => string.IsNullOrEmpty(e.Key)
                        ? e.Key
                        : string.Join(".", e.Key.Split('.').Select(
                            seg => seg.Length > 0 ? char.ToLowerInvariant(seg[0]) + seg[1..] : seg)),
                    e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray()
                );
            var response = new ErrorResponse(ErrorCodes.ValidationError, "One or more fields are invalid.", details);
            return new BadRequestObjectResult(response);
        };
    });

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
        options.Events = new JwtBearerEvents {
            OnChallenge = async context => {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var error = new ErrorResponse(ErrorCodes.Unauthorized, "Missing or invalid token.");
                await context.Response.WriteAsJsonAsync(error);
            }
        };
    });

builder.Services.AddAuthorization();

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Register services
builder.Services.AddScoped<DeckService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable Swagger in development only
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        var (statusCode, errorCode) = ex switch {
            NotFoundException => (404, ErrorCodes.NotFound),
            ConflictException => (409, ErrorCodes.Conflict),
            UnauthorizedException => (401, ErrorCodes.Unauthorized),
            _ => (500, ErrorCodes.ServerError)
        };
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var message = statusCode == 500 ? "An unexpected error occurred." : ex.Message;
        var error = new ErrorResponse(errorCode, message);
        await context.Response.WriteAsJsonAsync(error);
    }
});

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
