using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Text;
using HotChocolate;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.GraphQL;
using GraphQLAuth.Api.GraphQL.Types;
using GraphQLAuth.Api.GraphQL.Blogs;
using GraphQLAuth.Api.GraphQL.Assets;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Configure JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Add Entity Framework with PostgreSQL
builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AppDbContext>(provider => 
    provider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());


// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthConstants.Policies.RequireAuthenticated,
        policy => policy.RequireAuthenticatedUser());

    options.AddPolicy(AuthConstants.Policies.RequireSystemAdmin,
        policy => policy.RequireAuthenticatedUser()
            .AddRequirements(new ClientAccessRequirement(AuthConstants.Roles.SystemAdmin)));

    options.AddPolicy(AuthConstants.Policies.RequireClientOwnerOrAdmin,
        policy => policy.RequireAuthenticatedUser()
            .AddRequirements(new ClientOwnerRequirement()));

    options.AddPolicy(AuthConstants.Policies.RequireBlogOwnerNotesAccess,
        policy => policy.RequireAuthenticatedUser()
            .AddRequirements(new BlogOwnerNotesRequirement()));

    options.AddPolicy(AuthConstants.Policies.RequireClientTenant,
        policy => policy.RequireAuthenticatedUser()
            .AddRequirements(new ClientTenantRequirement()));
});

// Add custom services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GraphQLAuth.Api.Auth.IAuthorizationService, GraphQLAuth.Api.Auth.AuthorizationService>();
builder.Services.AddScoped<IAuthorizationHandler, ClientAccessHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ClientOwnerHandler>();
builder.Services.AddScoped<IAuthorizationHandler, BlogOwnerNotesHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ClientTenantRequirementHandler>();
builder.Services.AddScoped<BlogsAuthorizer>();
builder.Services.AddScoped<AssetAuthorizer>();
builder.Services.AddScoped<TokenGenerator>(provider => 
    new TokenGenerator(provider.GetRequiredService<IOptions<JwtSettings>>().Value));

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .RegisterDbContextFactory<AppDbContext>()  
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<BlogType>()
    .AddType<AssetType>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddAuthorization()
    // .AddHttpRequestInterceptor<HttpRequestInterceptor>()
    .AddMaxExecutionDepthRule(5)  // Prevent deeply nested queries
    .SetMaxAllowedValidationErrors(10)  // Limit validation errors
    .ModifyRequestOptions(opt =>
    {
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
        opt.ExecutionTimeout = TimeSpan.FromSeconds(30);
    })  // Control error details and timeout
    .AddErrorFilter(error =>
    {
        if (error.Exception is UnauthorizedAccessException)
        {
            return ErrorBuilder.New()
                .SetMessage("You are not authorized to access this resource.")
                .SetCode("AUTH_NOT_AUTHORIZED")
                .Build();
        }
        return error;
    });

var app = builder.Build();

try
{
    Log.Information("Starting GraphQL Auth API");

    // Seed the database
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await GraphQLAuth.Api.DbSeeder.SeedAsync(context);
    }

    // Configure the HTTP request pipeline.
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
            
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.Identity.Name);
                var clientRoles = httpContext.User.FindAll("client_roles")
                    .Select(c => c.Value)
                    .ToList();
                if (clientRoles.Any())
                {
                    diagnosticContext.Set("ClientRoles", clientRoles);
                }
            }
        };
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGraphQL();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
