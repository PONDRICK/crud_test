using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Profiles;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ���� DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ���� AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ���� Controllers
builder.Services.AddControllers();

// ��������ͧ�Ѻ JSON ��лԴ��õ�Ǩ�ͺ Reference Loop
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// ���� Swagger (��ҵ�ͧ���)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAll");
// ����� Swagger � Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// ���͹حҵ�����Ҷ֧��ҹ CORS (��ҵ�ͧ���)
app.UseCors(builder =>
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());

app.UseAuthorization();

app.MapControllers();

app.Run();
