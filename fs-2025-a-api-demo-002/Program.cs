using fs_2025_a_api_demo_002.Endpoints;
using fs_2025_a_api_demo_002.Startup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddDependencies();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();





app.AddWeatherEndPoints();
app.AddRootEndPoints();
app.AddBookEndPoints();
app.AddCourseEndPoints();

app.Run();


