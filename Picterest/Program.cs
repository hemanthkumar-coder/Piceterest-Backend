using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Picterest.Configuration;
using Picterest.Context;
using Picterest.Repositories.Implementation;
using Picterest.Repositories.Interface;
using Picterest.Services.Implementation;
using Picterest.Services.Interface;
using Picterest.Workers;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddDbContext<ImageDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ImageConnection"));
});

// Add services to the container.
builder.Services.Configure<SeaweedOptions>(
    builder.Configuration.GetSection(SeaweedOptions.SectionName));

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var configuration  = sp.GetRequiredService<IConfiguration>();
    return new AmazonS3Client(
        configuration["SeaweedFS:AccessKey"],
        configuration["SeaweedFS:SecretKey"],
        new AmazonS3Config
        {
            ServiceURL = configuration["SeaweedFS:ServiceUrl"],
            ForcePathStyle = true
        });
});

builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICleanUpStorageService, CleanUpStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddHostedService<StorageCleanUpWorker>();
builder.Services.AddHostedService<DbFileCleanUpWorker>();
builder.Services.AddHttpClient();


builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token =
                context.Request.Cookies["access_token"];

            return Task.CompletedTask;
        }
    };
});



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    var origins = builder.Configuration
    .GetSection("origins")
    .Get<string[]>()
    ?? throw new InvalidOperationException("Origin URLs are not specified.");


    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.WithOrigins(origins) // React app
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

Log.Information("Picterest API started successfully.");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AngularPolicy");



app.UseAuthentication();  
app.UseAuthorization();

app.MapControllers();

Log.Information("Application is starting...");

app.Run();
