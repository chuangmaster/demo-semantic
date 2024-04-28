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
using Microsoft.SemanticKernel.ChatCompletion;
using demo_semantic.Plugins.ReservePlugin;
using demo_semantic.Plugins.BenefitRequestPlugin;

namespace demo_semantic
{
    public class OpenAi : IOpenAi
    {
        private IConfiguration _Configuration;
        private string ApiKey { get; set; }
        public OpenAi(IConfiguration configuration)
        {
            _Configuration = configuration;
            ApiKey = configuration.GetValue<string>("ApiKey") ?? "default key";
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
            // Enable auto function calling
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
                    System.Console.WriteLine(responose.GetValue<string>());

                }

            }
        }

        public async Task UseOpenAIChatCompletionAsync(bool trace = false)
        {
            IKernelBuilder builder = Kernel.CreateBuilder()
               .AddOpenAIChatCompletion("gpt-3.5-turbo", ApiKey);
            builder.Plugins.AddFromType<BenefitRequestPlugin>();
            Kernel kernel = builder.Build();
            if (trace)
            {
                ShowInvokeLog(kernel);
            }
            // Enable auto function calling
            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
            // Get chat completion service
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var historyContent = @"你是一個協助企業內部活動點數申請的助理，可以協助查詢員工所擁有的點數、協助登記點數等功能。
                           請先確認員工(User)需要什麼服務?
                           你所提供服務分為以下幾種
                           - 查詢點數
                           - 協助申請活動點數
                           - 根據對話紀錄提供使用點數的明細
                           ---根據員工提供訊息進行接下來的判斷---
                           <查詢點數>
                           請依照以下步驟，協助活動點數查詢
                           1. 協助確認員工`empId`
                           2. 呼叫Plugin方法 `GetBenefit`  並使用empId作為參數查詢員工點數
                           3. 提供以下格式的回應 
                           您好查詢後的點數為${點數}，謝謝

                           <協助申請活動點數>
                           請依照以下步驟，協助活動點數申請登記
                           1. 協助確認員工empId
                           2. 呼叫Plugin方法 `GetBenefit`  並使用empId作為參數查詢員工點數
                           3. 詢問員工要申請的[活動內容]與[使用多少點數]
                           4. 當員工回應的使用點數超過目前所擁有的點數，可以用第2步驟得知是否足夠，必須拒絕申請
                           5. 當驗證沒有任何問題，呼叫Plugin方法 `DoBenefitApply` 進行點數申請，並提供員工編號、申請的活動、使用的點數
                           6. 告知申請結果

                           <根據對話紀錄提供使用點數的明細>
                           1. 根據先前對話告訴員工目前使用點數紀錄
                           2. 提供以下格式回應
                           您好查詢後的紀錄為
                           ${活動1} 使用點數為${點數1}
                           ${活動2} 使用點數為${點數2}

                           ---
                           請全部使用中文作為回答方式，且是女性的助理腳色";
            ChatHistory history = new ChatHistory(historyContent);
            System.Console.Write("User > ");
            string? userInput;
            while (!string.IsNullOrEmpty(userInput = Console.ReadLine()))
            {
                history.AddUserMessage(userInput);

                var responose = await chatCompletionService.GetChatMessageContentAsync(history, settings, kernel);
                Console.ForegroundColor = ConsoleColor.Blue;

                System.Console.WriteLine("AI says > " + responose);


                // if (!string.IsNullOrEmpty(responose?.InnerContent?.ToString()))
                // {
                //     System.Console.WriteLine(aiResponse);
                // }
                history.AddMessage(responose.Role, responose.InnerContent?.ToString() ?? "");

                Console.ResetColor();
                System.Console.Write("User > ");
            }

        }
    }
}
