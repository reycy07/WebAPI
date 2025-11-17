using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI;
using WebAPI.Data;

var builder = WebApplication.CreateBuilder(args);
// services area
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers().AddNewtonsoftJson();
//builder.Services.AddControllers()
//    .AddNewtonsoftJson(options =>
//    {
//        options.SerializerSettings.ReferenceLoopHandling =
//            Newtonsoft.Json.ReferenceLoopHandling.Ignore;
//    });

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer("name=DefaultConnetion"));


var app = builder.Build();

// Middlewares

app.MapControllers();

app.Run();
