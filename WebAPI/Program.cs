using Microsoft.AspNetCore.Diagnostics;
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
using WebAPI.Routes;
using WebAPI.Services;
using WebAPI.Swagger;
using WebAPI.Utilities;
using WebAPI.Utilities.V1;

var builder = WebApplication.CreateBuilder(args);

// services area


builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(30);
});

//builder.Services.AddStackExchangeRedisOutputCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString("redis");
//});

builder.Services.AddDataProtection();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddControllers(opt =>
{
    opt.Conventions.Add(new VersionGroupConventional());
})
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer("name=DefaultConnection"));

builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<User>>();
builder.Services.AddScoped<SignInManager<User>>();

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IFileStorage, AzureFileStorage>();
//builder.Services.AddTransient<IFileStorage, FileLocalStorage>();

builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddScoped<AuthorRoutes>();
builder.Services.AddScoped<WebAPI.Services.V1.IGeneratorLinks, WebAPI.Services.V1.GeneratorLinks>();
builder.Services.AddScoped<HATEOASAuthorAttribute>();
builder.Services.AddScoped<HATEOASAuthorsAttribute>();

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
        Version = "V1",
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
    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "V2",
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
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

    // Middlewares
    app.UseExceptionHandler(exception => exception.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error!;

        var error = new Error()
        {
            ErrorMessage = exception.Message,
            StrackTrace = exception.StackTrace,
            Date = DateTime.UtcNow
        };

        var dbContext = context.RequestServices.GetService<ApplicationDbContext>();
        dbContext.Add(error);
        await dbContext.SaveChangesAsync();
        await Results.InternalServerError(new
        {
            type = "Error",
            message = "Ha ocurrido un error",
            status = 500
        }).ExecuteAsync(context);
    }));
app.UseSwagger();
app.UseSwaggerUI( options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API V1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Library API V2");
}
    );

app.UseCors();
app.UseStaticFiles();

app.UseOutputCache();

app.MapControllers();

app.Run();

public partial class Program { }
