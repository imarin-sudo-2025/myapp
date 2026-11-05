var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var products = new List<Product>
{
    new(1, "Laptop Pro", 1299.99m, true),
    new(2, "Mouse Wireless", 49.99m, true),
    new(3, "Teclado Mecánico", 89.00m, true),
    new(4, "Monitor 27\"", 279.99m, false)
};

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.MapGet("/products", () => Results.Ok(products));

app.MapGet("/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.MapPost("/products", (Product product) =>
{
    if (product is null || product.Id <= 0)
    {
        return Results.BadRequest(new { message = "Product is invalid" });
    }

    if (products.Any(p => p.Id == product.Id))
    {
        return Results.Conflict(new { message = $"Product with id {product.Id} already exists" });
    }

    products.Add(product);
    return Results.Created($"/products/{product.Id}", product);
});

app.MapPost("/products/{name:string}", (string name) =>
{
    var product = products.FirstOrDefault(p => p.Name == name);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.MapPut("/products/{id:int}", (int id, Product updatedProduct) =>
{
    var index = products.FindIndex(p => p.Id == id);
    if (index == -1)
    {
        return Results.NotFound();
    }

    products[index] = updatedProduct with { Id = id };
    return Results.Ok(products[index]);
});

app.MapDelete("/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
    {
        return Results.NotFound();
    }

    products.Remove(product);
    return Results.NoContent();
});

app.Run();

public record Product(int Id, string Name, decimal Price, bool Active);
