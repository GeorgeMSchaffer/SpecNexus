using SargentNexus.Infrastructure;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SargentNexus.Application.Auth;

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

await using (var scope = app.Services.CreateAsyncScope())
{
	var authSeeder = scope.ServiceProvider.GetRequiredService<IAuthSeeder>();
	await authSeeder.SeedSiteAdminAsync(CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
	var authorizationHeader = context.Request.Headers.Authorization.ToString();

	if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
	{
		var accessToken = authorizationHeader["Bearer ".Length..].Trim();
		var accessTokenReader = context.RequestServices.GetRequiredService<IAccessTokenReader>();
		var principal = accessTokenReader.Read(accessToken);

		if (principal is not null)
		{
			var claims = new List<Claim>
			{
				new(ClaimTypes.NameIdentifier, principal.UserId.ToString()),
				new(ClaimTypes.Role, principal.Role),
				new(ClaimTypes.Email, principal.Email)
			};

			if (principal.OrganizationId.HasValue)
			{
				claims.Add(new Claim("organization_id", principal.OrganizationId.Value.ToString()));
			}

			context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
		}
	}

	await next();
});

app.UseAuthorization();

app.MapControllers();

app.Run();
