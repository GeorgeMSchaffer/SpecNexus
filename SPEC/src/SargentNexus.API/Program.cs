using SargentNexus.Infrastructure;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
	options.InvalidModelStateResponseFactory = context =>
	{
		var problemDetails = new ValidationProblemDetails(context.ModelState)
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "One or more validation errors occurred.",
			Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
		};

		return new BadRequestObjectResult(problemDetails)
		{
			ContentTypes = { "application/problem+json" }
		};
	};
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
