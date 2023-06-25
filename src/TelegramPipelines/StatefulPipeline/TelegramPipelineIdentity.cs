namespace TelegramPipelines;

public record TelegramPipelineIdentity(long ChatId, IReadOnlyList<string> Root)
{
    public TelegramPipelineIdentity CreateChild(string childPipelineName)
    {
        List<string> childRoot = Root.ToList();
        childRoot.Add(childPipelineName);

        return this with { Root = childRoot };
    }

    public string ColonConcat()
    {
        return SeparatorConcat(":");
    }

    public string SeparatorConcat(string separator = ":")
    {
        return $"{ChatId}{separator}{string.Join(separator, Root)}";
    }
};