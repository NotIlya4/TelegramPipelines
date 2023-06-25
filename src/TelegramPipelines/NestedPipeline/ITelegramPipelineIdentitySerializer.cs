using TelegramPipelines.Abstractions;

namespace TelegramPipelines.NestedPipeline;

public interface ITelegramPipelineIdentitySerializer
{
    string Serialize(TelegramPipelineIdentity identity);
}