using TaklaciGuvercin.Infrastructure;
using TaklaciGuvercin.Infrastructure.Data;
using TaklaciGuvercin.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Taklaci Guvercin API", Version = "v1" });
});

// Add Infrastructure (EF Core, Repositories, SignalR)
builder.Services.AddInfrastructure(builder.Configuration);

// CORS for Unity client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUnity", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed database in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowUnity");
app.UseAuthorization();

app.MapControllers();
app.MapHub<AirspaceHub>("/hubs/airspace");

app.Run();
