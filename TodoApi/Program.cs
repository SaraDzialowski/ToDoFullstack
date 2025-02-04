using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin() // Allows access from any origin
              .AllowAnyMethod() // Allows all HTTP methods (GET, POST, PUT, DELETE)
              .AllowAnyHeader(); // Allows all headers
    });
});

// Add DbContext service with MySQL database connection
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))));

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable CORS for the API
app.UseCors();

// Enable Swagger if the application is in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapping all routes

// Get all tasks
app.MapGet("/tasks", async (HttpContext httpContext) =>
{
    using var scope = httpContext.RequestServices.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();

    try
    {
        var tasks = await dbContext.Items.ToListAsync();
        return Results.Ok(tasks);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return Results.Problem("An error occurred while retrieving tasks.");
    }
});

// Add a new task
app.MapPost("/tasks", async (ToDoDbContext dbContext, Item newItem) =>
{
    dbContext.Items.Add(newItem);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/tasks/{newItem.Id}", newItem);
});

// Update an existing task
app.MapPut("/tasks/{id}", async (ToDoDbContext dbContext, int id, Item updatedItem) =>
{
    var item = await dbContext.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.IsComplete = updatedItem.IsComplete;

    await dbContext.SaveChangesAsync();
    return Results.Ok(item);
});

// Delete a task
app.MapDelete("/tasks/{id}", async (ToDoDbContext dbContext, int id) =>
{
    var item = await dbContext.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    dbContext.Items.Remove(item);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/", () => "ToDoApi is running");

app.Run();
