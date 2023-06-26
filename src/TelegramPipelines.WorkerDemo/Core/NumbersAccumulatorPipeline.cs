using Telegram.Bot;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.Demo.Core;

public class NumbersAccumulatorPipeline : ITelegramPipelineClass<Accumulator>
{
    private readonly string _name;

    public NumbersAccumulatorPipeline(string name)
    {
        _name = name;
    }
    
    public async Task<Accumulator?> Execute(TelegramPipelineContext context)
    {
        Accumulator acc = await context.LocalStorage.Get<Accumulator>("acc") ?? new Accumulator(0);
        
        if (context.Update.Message!.Text == "stop")
        {
            return acc;
        }

        await context.TelegramBotClient.SendTextMessageAsync(context.Update.Message!.Chat, $"{_name}, {acc.Value}");

        acc = acc with { Value = acc.Value + 1 };
        await context.LocalStorage.Save("acc", acc);

        await context.NestedPipelineExecutor.Execute("biba", new NestedNumbersAccumulatorPipeline("nested"));

        return null;
    }
}

public record Accumulator(int Value);