namespace TelegramPipelines.LocalStorage;

public interface ITelegramPipelineIdentitySerializer
{
    string Serialize(TelegramPipelineIdentity identity);
}