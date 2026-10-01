using Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

// SignalR sends credentials, so origins must be listed explicitly (no AllowAnyOrigin)
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy
        .WithOrigins(corsOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

var app = builder.Build();

app.UseCors();

app.MapHub<ChatHub>("/hub");

app.Run();
