using AutoMapper;
using GameShopWebAPI.Configurations;
using GameShopWebAPI.CustomMiddleware;
using GameShopWebAPI.Data;
using GameShopWebAPI.DTO;
using GameShopWebAPI.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GameDBContext>(opt => opt.UseInMemoryDatabase("GameDb"));

builder.Services.AddAutoMapper(typeof(AutoMapperConfig));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<GameDBContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 1;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
});

var issuer = builder.Configuration["JWT:Issuer"];
var audience = builder.Configuration["JWT:Audience"];
var key = builder.Configuration["JWT:Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddAuthorization(options =>
 {
     options.AddPolicy("admin_greetings", policy =>
     policy.RequireAuthenticatedUser());
 });

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
app.MapGet("/games", async (GameDBContext db) => Results.Ok(await db.Games.ToListAsync()))
    .RequireAuthorization("admin_greetings");

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

app.MapPost("/signup", async (GameDBContext db, SignUpDTO userDTO, UserManager<ApplicationUser> userManager, IMapper mapper) =>
{
    var user = mapper.Map<ApplicationUser>(userDTO);
    var newUser = await userManager.CreateAsync(user, userDTO.Password);
    if (newUser.Succeeded)
    {
        return user;
    }
    else
    {
        return null;
    }

});

app.MapPost("/login", async (GameDBContext db, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration appConfig, LoginDTO loginDTO) =>
{
    var issuer = appConfig["JWT:Issuer"];
    var audience = appConfig["JWT:Audience"];
    var key = appConfig["JWT:Key"];

    if (loginDTO is not null)
    {
        var loginResult = await signInManager.PasswordSignInAsync(loginDTO.UserName, loginDTO.Password, loginDTO.RememberMe, false);
        if (loginResult.Succeeded)  
        {
            // generate a token
            var user = await userManager.FindByEmailAsync(loginDTO.UserName);
            if (user != null)
            {
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var theKey = new SymmetricSecurityKey(keyBytes); // 256 bits of key
                var creds = new SigningCredentials(theKey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(issuer, audience, null, expires: DateTime.Now.AddMinutes(30), signingCredentials: creds);
                return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) }); // token 
            }
        }
    }
    return Results.BadRequest();
});

app.UseCors(MyAllowSpecificOrigins);


app.UseMiddleware<ApiKeyAuthMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

