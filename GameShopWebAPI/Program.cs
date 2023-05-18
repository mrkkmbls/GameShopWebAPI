using GameShopWebAPI.CustomMiddleware;
using GameShopWebAPI.Data;
using GameShopWebAPI.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GameDBContext>(opt => opt.UseInMemoryDatabase("GameDb"));

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(opt =>
{
    opt.AddPolicy( name: MyAllowSpecificOrigins, policy =>
    {
         policy.WithOrigins("http://localhost:41763")
       // policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

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

//get all games
app.MapGet("/games", async (GameDBContext db) => Results.Ok(await db.Games.ToListAsync()));

app.MapGet("/games/{id}", async (GameDBContext db, int id) =>
    await db.Games.FindAsync(id)
    is Game game ? Results.Ok(game) : Results.NotFound());


//add game
app.MapPost("/games", async (GameDBContext db, Game game) => 
{
    db.Games.Add(game);
    await db.SaveChangesAsync();

    return Results.Created($"/game/{game.Id}", game);
});

//get specific game
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

//delete a game
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

app.UseCors(MyAllowSpecificOrigins);

//has a valid api key if it is valid then allow, else 
app.UseMiddleware<ApiKeyAuthMiddleware>();

app.Run();

