using Microsoft.OpenApi.Models;
using PizzaStore.Models;
using Microsoft.EntityFrameworkCore;

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=Pizzas.db";
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items"));
builder.Services.AddSqlite<PizzaDb>(connectionString);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PizzaStore API",
        Description = "Making the Pizzas you love",
        Version = "v1"
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
      builder =>
      {
          builder.WithOrigins("*");
      });
});

var app = builder.Build();
// app config
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
});
app.UseCors(MyAllowSpecificOrigins);

// routes
app.MapGet("/", () => "Hello World!");
app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());
app.MapGet("/pizzas/{id}", async (PizzaDb db, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    return Results.Ok(pizza);
});
app.MapPost("/pizzas", async (PizzaDb db, Pizza pizza) =>
{
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizza/{pizza.Id}", pizza);
});
app.MapPut("/pizzas/{id}", async (PizzaDb db, Pizza updatePizza, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    pizza.Name = updatePizza.Name;
    pizza.Description = updatePizza.Description;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/pizzas/{id}", async (PizzaDb db, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
