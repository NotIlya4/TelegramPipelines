using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramPipelines;

public record TelegramRequestContext(TelegramBotClient TelegramBotClient, Update Update);