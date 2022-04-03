using Microsoft.EntityFrameworkCore;
using FileBox.Models;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<FileContext>(opt =>
    opt.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=filedb;Trusted_Connection=True;"));


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
