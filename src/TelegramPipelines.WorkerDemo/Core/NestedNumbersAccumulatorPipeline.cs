using Telegram.Bot;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.Demo.Core;

public class NestedNumbersAccumulatorPipeline : ITelegramPipelineClass<Accumulator>
{
    private readonly string _name;

    public NestedNumbersAccumulatorPipeline(string name)
    {
        _name = name;
    }
    
    public async Task<Accumulator?> Execute(TelegramPipelineContext context)
    {
        Accumulator acc = await context.LocalStorage.Get<Accumulator>("acc") ?? new Accumulator(0);

        await context.TelegramBotClient.SendTextMessageAsync(context.Update.Message!.Chat, $"{_name}, {acc.Value}");

        acc = acc with { Value = acc.Value + 1 };
        await context.LocalStorage.Save("acc", acc);

        return null;
    }
}