using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ToDoContext>(options =>
    options.UseSqlite($"Data Source=todos.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/todos", async (ToDoContext db) =>
    await db.ToDos.OrderByDescending(t => t.CreatedAt).ToListAsync());

app.MapGet("/todos/{id:int}", async (int id, ToDoContext db) =>
    await db.ToDos.FindAsync(id) is ToDoItem todo ? Results.Ok(todo) : Results.NotFound());

app.MapPost("/todos", async (ToDoItem todo, ToDoContext db) =>
{
    db.ToDos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapPut("/todos/{id:int}", async (int id, ToDoItem updated, ToDoContext db) =>
{
    var todo = await db.ToDos.FindAsync(id);
    if (todo == null) return Results.NotFound();

    todo.Title = updated.Title;
    todo.IsCompleted = updated.IsCompleted;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/todos/{id:int}", async (int id, ToDoContext db) =>
{
    var todo = await db.ToDos.FindAsync(id);
    if (todo == null) return Results.NotFound();

    db.ToDos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
