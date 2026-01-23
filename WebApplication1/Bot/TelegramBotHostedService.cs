using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using WebApplication1.Services;

namespace WebApplication1.Bot
{
    public class TelegramBotHostedService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILlmClient _llmClient;
        private readonly ILogger<TelegramBotHostedService> _logger;

        public TelegramBotHostedService(IConfiguration configuration, ILlmClient llmClient, ILogger<TelegramBotHostedService> logger)
        {
            var token = configuration["TelegramBot:Token"] ?? throw new ArgumentNullException("TelegramBot:Token");
            _botClient = new TelegramBotClient(token);
            _llmClient = llmClient;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: stoppingToken);
            return Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text is not { } messageText)
                return;

            var response = await _llmClient.GenerateAsync(messageText, cancellationToken);
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, response, cancellationToken: cancellationToken);
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}] {apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(errorMessage);
            return Task.CompletedTask;
        }
    }
}
