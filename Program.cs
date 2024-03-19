using demo_semantic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder2 = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// IConfigurationRoot configuration = builder.Build();

 var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<IOpenAi, OpenAi>();
            })
            .Build();


// Easy question and answer
IOpenAi openAi = host.Services.GetRequiredService<IOpenAi>();

// await openAi.EasyPromptAskingAsync();

// await openAi.UseDateTimePlugin();

// await openAi.UseTranslationPlugin(true);

// await openAi.UseSumarizePlugin(false);


await openAi.UsechatCompletionAsync(true);



