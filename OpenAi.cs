using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using demo_semantic.Plugin.DateTimePlugin;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace demo_semantic
{
    public class OpenAi : IOpenAi
    {
        private IConfiguration _Configuration;
        private string ApiKey { get; set; }
        public OpenAi(IConfiguration configuration)
        {
            _Configuration = configuration;
            ApiKey = configuration["ApiKey"] ?? "ApiKey";
        }
        public async Task EasyPromptAskingAsync()
        {

            //model 可以參考: https://learn.microsoft.com/en-us/semantic-kernel/agents/kernel/adding-services?tabs=Csharp#openai
            Kernel kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion("gpt-3.5-turbo", ApiKey)
                        .Build();
            while (true)
            {
                string words = Console.ReadLine() ?? "What do you do?";
                System.Console.WriteLine("User says > " + words);
                var responose = await kernel.InvokePromptAsync(words);
                System.Console.WriteLine("AI says > " + responose.GetValue<string>());
            }
        }

        /// <summary>
        /// Trace invoke 
        /// </summary>
        /// <param name="kernel"></param>
        private void ShowInvokeLog(Kernel kernel)
        {
            kernel.PromptRendered += (sender, args) =>
            {
                Console.WriteLine("=========== PromptRendered Start ===========");
                Console.WriteLine(args.RenderedPrompt);
                Console.WriteLine("=========== PromptRendered End ===========\n\n");
            };


            kernel.FunctionInvoking += (sender, args) =>
            {
                Console.WriteLine("=========== FunctionInvoking Start ===========");
                Console.WriteLine(args.Function.Name);
                Console.WriteLine("=========== FunctionInvoking End ===========\n\n");

            };
        }
        public async Task UseDateTimePluginAsync(bool trace = false)
        {

            //model 可以參考: https://learn.microsoft.com/en-us/semantic-kernel/agents/kernel/adding-services?tabs=Csharp#openai
            Kernel kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion("gpt-3.5-turbo", ApiKey)
                        .Build();

            if (trace)
            {
                ShowInvokeLog(kernel);
            }

            DateTimePlugin dateTimePlugin = new DateTimePlugin();
            // kernel.ImportPluginFromObject(dateTimePlugin, "DateTimePlugin");
            kernel.Plugins.AddFromObject(dateTimePlugin);
            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
            var prompt = "請根據 **DateTimePlugin 所提供 UTC0 的時間 **，回答 User 詢問的時間問題. \nUser:";
            while (true)
            {
                System.Console.Write("User > ");
                string message = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrEmpty(message))
                {
                    System.Console.WriteLine("Please ask your question.");
                }
                else
                {
                    var responose = await kernel.InvokePromptAsync(prompt + message, new(settings));
                    System.Console.WriteLine("AI says > " + responose.GetValue<string>());
                }

            }
        }

        public async Task UseTranslationPluginAsync(bool trace = false)
        {
            Kernel kernel = Kernel.CreateBuilder()
                   .AddOpenAIChatCompletion("gpt-3.5-turbo", ApiKey)
                   .Build();
            if (trace)
            {
                ShowInvokeLog(kernel);
            }
            // 撰寫資料夾規則 Plugin 規則 Plugin/TranslatePlugin
            // 內部的都是 Function
            var translatePlugn = kernel.ImportPluginFromPromptDirectory(Path.Combine("./Plugins", "TranslatePlugin"));
            System.Console.WriteLine("This is translator, please tell me what you want to translate.");
            while (true)
            {
                System.Console.Write("User > ");
                string message = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrEmpty(message))
                {
                    System.Console.WriteLine("Please ask your question.");
                    continue;
                }
                else
                {
                    System.Console.WriteLine("Which language do you want to translate?");
                    string lang = Console.ReadLine() ?? string.Empty;
                    KernelFunction function = translatePlugn["Translate"];
                    var responose = await kernel.InvokeAsync(function, new()
                    {
                        ["input"] = message,
                        ["language"] = lang
                    });
                    System.Console.WriteLine("AI says > " + responose.GetValue<string>());
                }

            }
        }
        /// <summary>
        /// 使用 yaml 來作為 plugin 的資料
        /// </summary>
        /// <returns></returns>
        public async Task UseSumarizePluginAsync(bool trace = false)
        {
            Kernel kernel = Kernel.CreateBuilder()
                    .AddOpenAIChatCompletion("gpt-3.5-turbo", ApiKey)
                    .Build();
            if (trace)
            {
                ShowInvokeLog(kernel);
            }
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "SummarizePlugin", "Summarize", "Summarize.yaml");
            using StreamReader reader = new StreamReader(path);
            var function = kernel.CreateFunctionFromPromptYaml(reader.ReadToEnd());
            while (true)
            {
                System.Console.WriteLine("Provide short article:");
                System.Console.Write("User > ");
                string message = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrEmpty(message))
                {
                    System.Console.WriteLine("Please give me your article");
                    continue;
                }
                else
                {
                    var responose = await kernel.InvokeAsync(function, new()
                    {
                        ["content"] = message
                    });
                    System.Console.WriteLine("AI says > ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine(responose.GetValue<string>());

                }

            }

        }
    }
}
