using Telegram.Bot.Types.Enums;

namespace Core.TelegramFramework.SimpleTelegramClient;

public record SendTextOptions(ParseMode ParseMode = ParseMode.Markdown);