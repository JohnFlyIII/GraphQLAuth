using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using HotChocolate;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.GraphQL;
using GraphQLAuth.Api.GraphQL.Types;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Add Entity Framework with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
});

// Add custom services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GraphQLAuth.Api.Auth.IAuthorizationService, GraphQLAuth.Api.Auth.AuthorizationService>();
builder.Services.AddScoped<IAuthorizationHandler, ClientAccessHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ClientOwnerHandler>();
builder.Services.AddScoped<IAuthorizationHandler, BlogOwnerNotesHandler>();
builder.Services.AddScoped<BlogsAuthorizer>();

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddType<BlogType>()
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

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();

app.Run();

public partial class Program { }
