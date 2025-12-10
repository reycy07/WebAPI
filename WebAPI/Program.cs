using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using WebAPI;
using WebAPI.Data;
using WebAPI.Entities;
using WebAPI.Services;
using WebAPI.Swagger;

var builder = WebApplication.CreateBuilder(args);

// services area
builder.Services.AddDataProtection();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer("name=DefaultConnetion"));

builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<User>>();
builder.Services.AddScoped<SignInManager<User>>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IFileStorage, AzureFileStorage>();
//builder.Services.AddTransient<IFileStorage, FileLocalStorage>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwtkey"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("isAdmin", policy => policy.RequireClaim("isAdmin"));
});

var permittedOrigins = builder.Configuration.GetSection("permittedOrigins").Get<string[]>()!;

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(CORSoptions =>
    {
        CORSoptions.WithOrigins(permittedOrigins).AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("total-qty-recodrs");
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Library API",
        Description = "Este es una web api para trabajar con datos de autores y libros",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Email = "contact@cristianfullstack.com",
            Name = "Cristia Rojas",
            Url = new Uri("https://cristianfullstack.com")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Beare",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.OperationFilter<AuthFilter>();
});




var app = builder.Build();

// Middlewares
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseStaticFiles();

app.MapControllers();

app.Run();
