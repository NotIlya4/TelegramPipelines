using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramPipelines.StatefulPipeline;

public record TelegramRequestContext(ITelegramBotClient TelegramBotClient, Update Update);