using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Profiles;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// เพิ่ม DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// เพิ่ม AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// เพิ่ม Controllers
builder.Services.AddControllers();

// เพิ่มการรองรับ JSON และปิดการตรวจสอบ Reference Loop
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// เพิ่ม Swagger (ถ้าต้องการ)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAll");
// การใช้ Swagger ใน Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// การอนุญาตให้เข้าถึงผ่าน CORS (ถ้าต้องการ)
app.UseCors(builder =>
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());

app.UseAuthorization();

app.MapControllers();

app.Run();
