namespace demo_semantic
{
    public interface IOpenAi
    {
        Task EasyPromptAskingAsync();
        Task UseSumarizePluginAsync(bool trace = false);
        Task UseDateTimePluginAsync(bool trace = false);
        Task UseTranslationPluginAsync(bool trace = false);
        Task UsechatCompletionAsync(bool trace = false);
    }
}
