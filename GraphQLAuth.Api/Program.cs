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
using GraphQLAuth.Api.GraphQL.Concepts.Blog.Types;
using GraphQLAuth.Api.GraphQL.Concepts.Blog.Queries;
using GraphQLAuth.Api.GraphQL.Concepts.Blog.Mutations;
using GraphQLAuth.Api.GraphQL.Concepts.Asset.Types;
using GraphQLAuth.Api.GraphQL.Concepts.Asset.Queries;
using GraphQLAuth.Api.GraphQL.Concepts.Asset.Mutations;
using GraphQLAuth.Api.GraphQL.Concepts.Status.Types;
using GraphQLAuth.Api.GraphQL.Concepts.Status.Queries;
using GraphQLAuth.Api.GraphQL.Concepts.Status.Mutations;
using GraphQLAuth.Api.GraphQL.Concepts.Auth.Queries;
using GraphQLAuth.Api.GraphQL.Concepts.Auth.Mutations;
using GraphQLAuth.Api.GraphQL.Concepts.Client.Types;
using GraphQLAuth.Api.GraphQL.Authorization;
using GraphQLAuth.Api.Models;
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
    // Generic client owner role policy
    options.AddPolicy(AuthConstants.Policies.RequireClientOwnerRole,
        policy => policy.RequireAuthenticatedUser()
            .AddRequirements(new ClientOwnerRoleRequirement()));

    options.AddPolicy(AuthConstants.Policies.RequireClientTenant,
        policy => policy.RequireAuthenticatedUser()
            .AddRequirements(new ClientTenantRequirement()));

    // New resource-based policies
    options.AddPolicy(AuthConstants.Policies.RequireAnyRole,
        policy => policy.RequireAuthenticatedUser()
            .AddRequirements(new AnyRoleRequirement()));


    options.AddPolicy(AuthConstants.Policies.RequireSystemAdmin,
        policy => policy.RequireAuthenticatedUser()
            .AddRequirements(new AnyRoleRequirement(AuthConstants.Roles.SystemAdmin)));
});

// Add custom services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GraphQLAuth.Api.Auth.IAuthorizationService, GraphQLAuth.Api.Auth.AuthorizationService>();
// Legacy authorization handlers (keeping for compatibility)
builder.Services.AddScoped<IAuthorizationHandler, ClientOwnerRoleHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ClientTenantRequirementHandler>();

// New resource-based authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, AnyRoleAuthorizationHandler>();
// Generic client resource authorizers
builder.Services.AddScoped<ClientResourceAuthorizer<Blog>>();
builder.Services.AddScoped<ClientResourceAuthorizer<Asset>>();
builder.Services.AddScoped<TokenGenerator>(provider => 
    new TokenGenerator(provider.GetRequiredService<IOptions<JwtSettings>>().Value));

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .RegisterDbContextFactory<AppDbContext>()  
    .AddQueryType(d => d.Name("Query"))
    .AddMutationType(d => d.Name("Mutation"))
    // Blog concept
    .AddTypeExtension<GetBlogs>()
    .AddTypeExtension<GetBlog>()
    .AddTypeExtension<CreateBlog>()
    .AddTypeExtension<AssociateBlogAsset>()
    .AddType<BlogType>()
    // Asset concept  
    .AddTypeExtension<GetAssetLibrary>()
    .AddTypeExtension<CreateAsset>()
    .AddType<GraphQLAuth.Api.GraphQL.Concepts.Asset.Types.AssetType>()
    // Status concept
    .AddTypeExtension<GetStatus>()
    .AddTypeExtension<UpdateStatus>()
    .AddType<StatusType>()
    // Auth concept
    .AddTypeExtension<GetTestClientIds>()
    .AddTypeExtension<GraphQLAuth.Api.GraphQL.Concepts.Auth.Mutations.GenerateToken>()
    // Client type (navigation only, no top-level queries)
    .AddType<ClientType>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddAuthorization()
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
