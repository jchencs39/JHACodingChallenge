using JHACodingChallenge.Configurations;
using JHACodingChallenge.Services;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
var Configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
#if DEBUG
    .AddJsonFile("appsettings.Development.json", true, true)
#endif
    .Build();

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddSingleton<TwitterConfiguration>(new TwitterConfiguration()
{
    BearerToken = Configuration.GetValue<string>("TwitterConfiguration:BearerToken"),
    BaseUrl = Configuration.GetValue<string>("TwitterConfiguration:BaseUrl"),
    ParamName = Configuration.GetValue<string>("TwitterConfiguration:ParamName"),
    ParamValue = Configuration.GetValue<string>("TwitterConfiguration:ParamValue")
});
builder.Services.AddTransient<ITwitterStreamService, TwitterStreamService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler("/error/500");

app.UseAuthorization();

app.MapControllers();

app.Run();
