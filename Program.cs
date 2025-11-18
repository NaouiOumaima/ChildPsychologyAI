using ChildPsychologyAI.Services.Data;
using ChildPsychologyAI.Services.Analysis;
using ChildPsychologyAI.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configuration MongoDB
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<MongoDbContext>();

// Services d'analyse
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IColorAnalysisService, ColorAnalysisService>();
builder.Services.AddScoped<IDrawingAnalysisService, DrawingAnalysisService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// NOUVEAUX SERVICES - Gestion des utilisateurs et enfants
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChildService, ChildService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();