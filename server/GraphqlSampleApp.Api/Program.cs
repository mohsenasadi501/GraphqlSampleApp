using GraphqlSampleApp.Api.Repositories;
using GraphqlSampleApp.Api.Types;
using GraphqlSampleApp.Api.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;

string AllowedOrigin = "allowedOrigin";

var builder = WebApplication.CreateBuilder(args);

// configure strongly typed settings object
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddSingleton<IMongoClient, MongoClient>(sp => new MongoClient(builder.Configuration.GetConnectionString("MongoDb")));
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddGraphQLServer()
        .AddAuthorization()
        //for inmemory subscription
        .AddInMemorySubscriptions()
        .AddQueryType<Query>()
        .AddMutationType<Mutation>()
        .AddSubscriptionType<Subscription>()
        .AddGlobalObjectIdentification()
        // Registers the filter convention of MongoDB
        .AddMongoDbFiltering()
        // Registers the sorting convention of MongoDB
        .AddMongoDbSorting()
        // Registers the projection convention of MongoDB
        .AddMongoDbProjections()
        // Registers the paging providers of MongoDB
        .AddMongoDbPagingProviders();

// CORS
builder.Services.AddCors(option =>
{
    option.AddPolicy(AllowedOrigin, builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var tokenSettings = builder.Configuration
                .GetSection("JwtSettings").Get<JwtSettings>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = tokenSettings.Issuer,
                    ValidateIssuer = true,
                    ValidAudience = tokenSettings.Audience,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Secret)),
                    ValidateIssuerSigningKey = true,
                };
            });

builder.Services
    .AddAuthorization(options =>
    {
        options.AddPolicy("roles-policy", policy =>
        {
            policy.RequireRole(new string[] { "admin", "super-admin" });
        });
        options.AddPolicy("claim-policy-1", policy =>
        {
            policy.RequireClaim("LastName");
        });
        options.AddPolicy("claim-policy-2", policy =>
        {
            policy.RequireClaim("LastName", new string[] { "Bommidi", "Test" });
        });
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors(AllowedOrigin);
app.UseWebSockets();
app.MapGraphQL();
app.Run();
