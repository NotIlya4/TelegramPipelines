using NotIlya.Extensions.Configuration;
using NotIlya.Extensions.Redis;
using NotIlya.Extensions.Serilog;
using Telegram.Bot;
using TelegramPipelines.Demo.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();
IServiceCollection services = builder.Services;
IConfiguration config = builder.Configuration;

string redisConn = config.GetRedisConnectionString("Redis");
string telegramApiKey = config.GetRequiredValue("TelegramApiKey");
        
services.AddHostedService<TelegramBotWorker>();
services.AddSerilog(config.GetAddSerilogOptions("Serilog"));
services.AddSingleton(new TelegramOptions(redisConn, telegramApiKey));
services.AddSingleton(s => new TelegramBotClient(s.GetRequiredService<TelegramOptions>().TelegramApiKey));
services.AddRedis(redisConn);
services.AddHostedService<TelegramBotWorker>();

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();