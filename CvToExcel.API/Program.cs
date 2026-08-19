using CvToExcel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CvToExcel.Application.Features.UploadCv;
using CvToExcel.Application.Interfaces;
using CvToExcel.Infrastructure.AiExtraction;
using CvToExcel.Infrastructure.FileStorage;
using FluentValidation;
using CvToExcel.Application.Contracts;
using CvToExcel.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => 
cfg.RegisterServicesFromAssembly(typeof(UploadCvCommand).Assembly));

builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();
builder.Services.AddScoped<IValidator<CvExtractionResult>, CvExtractionResultValidator>();
builder.Services.AddScoped<ICvDocumentRepository, CvDocumentRepository>();

builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));

builder.Services.AddHttpClient<IAiExtractor, GeminiAiExtractor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CvToExcel API");
    });
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();

