using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using UniversityClubAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON FIX (IMPORTANT)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // enum take reaction as string instead of int 
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());

        //  CYCLE FIX (MAIN SOLUTION)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // optional (pretty JSON)
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Swagger (optional)
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

//  JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        )
    };
});

//  EF Core DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

// Swagger (optional)
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();