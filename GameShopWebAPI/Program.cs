using GameShopWebAPI.Data;
using GameShopWebAPI.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GameDBContext>(opt => opt.UseInMemoryDatabase("GameDb"));

// Add services to the container.
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

app.MapGet("/games", async (GameDBContext db) => await db.Games.ToListAsync());

app.MapGet("/games/{id}", async (GameDBContext db, int id) =>
    await db.Games.FindAsync(id)
    is Game game ? Results.Ok(game) : Results.NotFound());

app.MapPost("/games", async (GameDBContext db, Game game) => 
{
    db.Games.Add(game);
    await db.SaveChangesAsync();

    return Results.Created($"/game/{game.Id}", game);
});

app.MapPut("/games/{id}", async (GameDBContext db, Game game, int id) =>
{
    var oldGame = await db.Games.FindAsync(id);
    if(game is null)
    {
        return Results.NotFound();
    }
    else
    {
        oldGame.gameName = game.gameName;
        oldGame.gameDescription = game.gameDescription;
        oldGame.gamePrice = game.gamePrice;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }
});


app.MapDelete("/games/{id}", async (GameDBContext db, int id) =>
{
    if(await db.Games.FindAsync(id) is Game game)
    {
        db.Games.Remove(game);
        await db.SaveChangesAsync();
        return Results.Ok(game);
    }
    return Results.NotFound();
    
});

app.Run();

