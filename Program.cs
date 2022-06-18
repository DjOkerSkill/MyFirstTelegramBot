using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.ReplyMarkups;

using MyFirstTelegramBot;

namespace MyFirstTelegraBot 
{ 
    public class Program 
    {
        public static async Task Main()
        {
            
            Program program = new Program();
            var botClient = new TelegramBotClient(AppSetting.BotToken);

            var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

            botClient.StartReceiving(program.HandleUpdatesAsync, program.HandleErrorsAsync, receiverOptions, cancellationToken : cts.Token);

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Начал прослушку {me.Username}");

            Console.ReadLine();

            cts.Cancel();  
        }

        async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                await HandleMessage(botClient, update.Message);
                return;
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery, update);
                return;
            }
        }

        async Task HandleMessage(ITelegramBotClient botClient, Message message) 
        {
            CallbackQuery callbackQuery = new CallbackQuery();
           
            if(message.Text == "/start")

                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите команду: /inline");

            if(message.Text == "/inline")
            {
                InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Длина строки", "длина строки"),
                        InlineKeyboardButton.WithCallbackData("Сумма чисел", "сумма чисел"),
                    },
                });
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Выберите действие:", replyMarkup: inlineKeyboardMarkup);
                return;
            }

            if(message.Text != "/start" && message.Text != "/inline")
            
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Длина строки равна: {message.Text.Length}");
           
            if(callbackQuery.Data == "сумма чисел")
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Сумма равна: {GetSum(message)}");
        }
        async Task HandleErrorsAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) 
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Ошибка телеграмм АПИ {apiRequestException.ErrorCode}\n {apiRequestException.Message}",

                _ => exception.ToString()
            };
            Console.WriteLine(errorMessage);
            return;
        }
        async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, Update update)
        {
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Вы нажали {callbackQuery.Data}");

            if (callbackQuery.Data == "длина строки")
            {
                await botClient.SendTextMessageAsync(callbackQuery.From.Id, $"Введите текст");
                return;
            }

            if (callbackQuery.Data == "сумма чисел")
            {
                await botClient.SendTextMessageAsync(callbackQuery.From.Id, $"Введите числа");
                return;
            }
        }

        private int GetSum(Message e)
        {
            string tempmessage = e.Text;
            char separator = ' ';
            string[] tempArr = tempmessage.Split(separator);
            int result = 0;
            foreach (var item in tempArr)
            {
                int temp = Convert.ToInt32(item);
                result += temp;
            }
            return result;
        }
    }
}