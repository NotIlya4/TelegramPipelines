using Telegram.Bot;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.Demo.Core;

public class AskUserPipeline : ITelegramPipelineClass<string>
{
    public string MessageWhatToAsk { get; set; }

    public AskUserPipeline(string messageWhatToAsk)
    {
        MessageWhatToAsk = messageWhatToAsk;
    }


    public async Task<string?> Execute(TelegramPipelineContext context)
    {
        string asked = await context.LocalStorage.Get<string>("asked") ?? "";

        if (asked == "asked")
        {
            return context.Update.Message!.Text;
        }

        await context.TelegramBotClient.SendTextMessageAsync(context.Update.Message!.Chat, MessageWhatToAsk);
        await context.LocalStorage.Save("asked", "asked");
        return null;
    }
}