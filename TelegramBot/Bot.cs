using com.LandonKey.SocksWebProxy;
using com.LandonKey.SocksWebProxy.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public class Bot
    {
        static class StringHelper
        {
            public static string incorectInputData = "InputDataWasIncorrect";
            public static string helloString = "Hello, I'm booking boot, please write your name\n or press any key if you already registrated";
            public static string exeptionString = "Smth go wrong, try again";
            public static string chooseString = "Hello, what do you want?\nPress any key to see all commands";
            public static string dataInputString = "Please write Date Format DD/MM/YYYY";
            public static string roomInputString = "enter classroom number";
        }
        static private Dictionary<long, Conditions> userConditions = new Dictionary<long, Conditions>();
        static ITelegramBotClient botClient;
        static private Dictionary<long, UserPreferences> userPreferences = new Dictionary<long, UserPreferences>();
        private class UserPreferences
        {
            public bool isReservation = false;
            public bool findNearest = false;
            public DateTime localDate = new DateTime();
            public int localRoomNum = 0;
            public int resNumber = 0;
        }

        public void CreateBot()
        {
            botClient = new TelegramBotClient("641172514:AAHG1IMwYqmTEnrL-JEEaP01Sz1etUX-2HM", new SocksWebProxy(
                new ProxyConfig(
                    IPAddress.Parse("127.0.0.1"),
                    GetNextFreePort(),
                    IPAddress.Parse("185.36.191.39"),
                    4443,
                    ProxyConfig.SocksVersion.Five,
                    "userid87sU",
                    "e3rLFG69"),
                false));

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
                $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );
        }

        private static int GetNextFreePort()
        {
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }

        public void StartBot()
        {
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            while (true)
            {
                Thread.Sleep(int.MaxValue);
            }
        }


        private static InlineKeyboardButton[][] GetInlineKeyboard(string[] stringArray)
        {
            var keyboardInline = new InlineKeyboardButton[stringArray.Length][];
            for (var i = 0; i < stringArray.Length; i++)
            {
                var keyboardButtons = new InlineKeyboardButton[1];
                keyboardButtons[0] = new InlineKeyboardButton
                {
                    Text = stringArray[i],
                    CallbackData = stringArray[i],
                };
                keyboardInline[i] = keyboardButtons;
            }
            return keyboardInline;
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            var chatId = e.Message.Chat.Id;
            if (!userConditions.ContainsKey(chatId))
            {
                userConditions.Add(chatId, Conditions.PreInitialize);
            }
            if (!userPreferences.ContainsKey(chatId))
            {
                userPreferences.Add(chatId, new UserPreferences());
            }
            var userState = userConditions[chatId];

            switch (userState)
            {
                case Conditions.PreInitialize:
                    {
                        await botClient.SendTextMessageAsync(chatId, StringHelper.helloString);
                        userConditions[chatId] = Conditions.Initialize;
                        break;
                    }
                case Conditions.Initialize:
                    {
                        var userName = e.Message.Text;
                        // запрос на сервер
                        var response = SendPreferencesRequest(userName, chatId, Conditions.Initialize);
                        //
                        switch (response)
                        {
                            case "ok":
                                {
                                    await botClient.SendTextMessageAsync(chatId, $"User {userName} was successfully created");
                                    userConditions[chatId] = Conditions.EnableToreserve;
                                    break;
                                }
                            case "no":
                                {
                                    await botClient.SendTextMessageAsync(chatId, $"UserName {userName} already exist, please select another one");
                                    break;
                                }
                            case "hi":
                                {
                                    await botClient.SendTextMessageAsync(chatId, StringHelper.chooseString);
                                    userConditions[chatId] = Conditions.EnableToreserve;
                                    break;
                                }
                            default:
                                {
                                    await botClient.SendTextMessageAsync(chatId, StringHelper.exeptionString);
                                    break;
                                }
                        }
                        break;
                    }
                case Conditions.EnableToreserve:
                    {
                        var buttonItem = new[] { "/Res - to reserve",
                            "/FN - to find nearest free time to reserve classroom",
                            "/SARInDay- to show all reservations in selected day",
                            "/SARToday - to show all reservations today",
                            "/SAMyRes - to show all your reservations",
                            "/Del - to delete one of your reservations"};
                        var keyboardMarkup = new InlineKeyboardMarkup(GetInlineKeyboard(buttonItem)); //replyMarkup: keyboardMarkup
                        var command = e.Message.Text.ToLower();
                        switch (command)
                        {
                            case "/del":
                                {
                                    userPreferences[chatId].isReservation = true; var response = SendPreferencesRequest("ShowAllMyRes", chatId, Conditions.EnableToreserve);
                                    switch (response)
                                    {
                                        case "haven't any res":
                                            {
                                                await botClient.SendTextMessageAsync(chatId, "You Haven't any resorve");
                                                break;
                                            }
                                        default:
                                            {
                                                await botClient.SendTextMessageAsync(chatId, $"choose number of reservation, which you would like to delete\n{response}");
                                                userConditions[chatId] = Conditions.DeletingRes;
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case "/res":
                                {
                                    userPreferences[chatId].isReservation = true;
                                    await botClient.SendTextMessageAsync(chatId, StringHelper.dataInputString + " HH:MM:SS");
                                    userConditions[chatId] = Conditions.ChoosingDay;
                                    break;
                                }
                            case "/fn":
                                {
                                    userPreferences[chatId].isReservation = true;
                                    userPreferences[chatId].findNearest = true;
                                    await botClient.SendTextMessageAsync(chatId, StringHelper.roomInputString);
                                    userPreferences[chatId].localDate = DateTime.Now;
                                    userConditions[chatId] = Conditions.ChoosingRoom;
                                    break;
                                }
                            case "/sarinday":
                                {
                                    userPreferences[chatId].isReservation = false;
                                    await botClient.SendTextMessageAsync(chatId, StringHelper.dataInputString);
                                    userPreferences[chatId].localDate = DateTime.Now;
                                    userConditions[chatId] = Conditions.ChoosingDay;
                                    break;
                                }
                            case "/sartoday":
                                {
                                    userPreferences[chatId].isReservation = false;
                                    await botClient.SendTextMessageAsync(chatId, StringHelper.roomInputString);
                                    userPreferences[chatId].localDate = DateTime.Now;
                                    userConditions[chatId] = Conditions.ChoosingRoom;
                                    break;
                                }
                            case "/samyres":
                                {
                                    var response = SendPreferencesRequest("ShowAllMyRes", chatId, Conditions.EnableToreserve);
                                    switch (response)
                                    {
                                        case "haven't any res":
                                            {
                                                await botClient.SendTextMessageAsync(chatId, "You Haven't any resorve");
                                                break;
                                            }
                                        default:
                                            {
                                                await botClient.SendTextMessageAsync(chatId, response);
                                                break;
                                            }
                                    }
                                    break;
                                }

                            default:
                                {
                                    await botClient.SendTextMessageAsync(chatId, "Command was Incorect");
                                    await botClient.SendTextMessageAsync(chatId, "Please write on of next operations:", replyMarkup: keyboardMarkup);
                                    break;
                                }
                        }
                        break;
                    }
                case Conditions.DeletingRes:
                    {
                        var response = SendPreferencesRequest("ShowAllMyRes", chatId, Conditions.EnableToreserve);
                        var numOfRes = response.Split('\n').Select(s => int.Parse(s.Split(' ')[0]));
                        var input = e.Message.Text;
                        Console.WriteLine(input);
                        try
                        {
                            var intInput = int.Parse(input);
                            if (numOfRes.Contains(intInput))
                            {
                                Console.WriteLine("ok");
                                userPreferences[chatId].resNumber = intInput;
                                userConditions[chatId] = Conditions.DeletingApprove;
                                await botClient.SendTextMessageAsync(chatId, $"are you sure, want you want delete this res?(yes/no)");
                                break;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(chatId, StringHelper.incorectInputData);
                            }
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(chatId, StringHelper.incorectInputData);
                            await botClient.SendTextMessageAsync(chatId, $"choose number of reservation, which you would like to delete\n{response}");
                        }
                        break;
                    }
                case Conditions.DeletingApprove:
                    {
                        Console.WriteLine(userPreferences[chatId].resNumber);
                        var userResponse = e.Message.Text;
                        switch (userResponse)
                        {
                            case "yes":
                                {
                                    var resp = SendPreferencesRequest($"{userPreferences[chatId].resNumber}", chatId, Conditions.DeletingApprove, "yes");
                                    switch (resp)
                                    {
                                        case "ok":
                                            {
                                                await botClient.SendTextMessageAsync(chatId, "your reservation delete");
                                                userConditions[chatId] = Conditions.EnableToreserve;
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case "no":
                                {
                                    await botClient.SendTextMessageAsync(chatId, "deleting was canceled");
                                    userConditions[chatId] = Conditions.EnableToreserve;
                                    break;
                                }
                            default:
                                {
                                    await botClient.SendTextMessageAsync(chatId, $"are you sure, want you want delete this res?(yes/no)");
                                    break;
                                }
                        }
                        break;
                    }
                case Conditions.ChoosingDay:
                    {
                        var date = e.Message.Text;
                        try
                        {
                            if (DateTime.Parse(date) != null)
                            {
                                if (userPreferences[chatId].isReservation && DateTime.Parse(date) < DateTime.Now)
                                {
                                    await botClient.SendTextMessageAsync(chatId, "Sorry you still can't travel in past, choose another date");
                                    break;
                                }
                                userPreferences[chatId].localDate = DateTime.Parse(date);
                            }
                            userConditions[chatId] = Conditions.ChoosingRoom;
                            await botClient.SendTextMessageAsync(chatId, StringHelper.roomInputString);
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(chatId, StringHelper.incorectInputData);
                            if (userPreferences[chatId].isReservation)
                                await botClient.SendTextMessageAsync(chatId, StringHelper.dataInputString + " HH:MM:SS");
                            else
                                await botClient.SendTextMessageAsync(chatId, StringHelper.dataInputString);
                        }
                        break;
                    }
                case Conditions.ChoosingRoom:
                    {
                        try
                        {
                            var roomNum = int.Parse(e.Message.Text);
                            userPreferences[chatId].localRoomNum = roomNum;
                            if (userPreferences[chatId].isReservation)
                            {
                                var response = SendPreferencesRequest(userPreferences[chatId].localRoomNum.ToString(), chatId, Conditions.ChoosingRoom, "addRoom");
                                userConditions[chatId] = Conditions.ApproveReservation;
                                await botClient.SendTextMessageAsync(chatId, "press any key");
                            }
                            else
                            {
                                var response = SendPreferencesRequest($"{userPreferences[chatId].localDate}/{userPreferences[chatId].localRoomNum}", chatId, Conditions.ChoosingRoom);
                                await botClient.SendTextMessageAsync(chatId, response);
                                userConditions[chatId] = Conditions.EnableToreserve;
                            }
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(chatId, StringHelper.incorectInputData);
                            await botClient.SendTextMessageAsync(chatId, StringHelper.roomInputString);
                        }

                        break;
                    }
                case Conditions.ApproveReservation:
                    {
                        var userResponse = e.Message.Text;
                        string response;
                        if (userPreferences[chatId].findNearest)
                        {
                            response = SendPreferencesRequest(userPreferences[chatId].localRoomNum.ToString(), chatId, Conditions.ApproveReservation, "findNearest");
                            var data = response.Split(' ');
                            var date = DateTime.Parse(data[0] + " " + data[1]);
                            var roomNum = int.Parse(data[2]);
                            userPreferences[chatId].localDate = date;
                            userPreferences[chatId].localRoomNum = roomNum;
                            userPreferences[chatId].findNearest = !userPreferences[chatId].findNearest;
                        }
                        else
                            response = SendPreferencesRequest($"{userPreferences[chatId].localDate}/{userPreferences[chatId].localRoomNum}", chatId, Conditions.ApproveReservation, "data");
                        switch (userResponse)
                        {
                            case "yes":
                                {
                                    var resp = SendPreferencesRequest($"{userPreferences[chatId].localDate}/{userPreferences[chatId].localRoomNum}", chatId, Conditions.ApproveReservation, "yes");
                                    switch (resp)
                                    {
                                        case "ok":
                                            {
                                                await botClient.SendTextMessageAsync(chatId, "your reservation approve");
                                                userConditions[chatId] = Conditions.EnableToreserve;
                                                break;
                                            }
                                        case "no":
                                            {
                                                await botClient.SendTextMessageAsync(chatId, "you can't have reservation in this time");
                                                userConditions[chatId] = Conditions.EnableToreserve;
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case "no":
                                {
                                    await botClient.SendTextMessageAsync(chatId, "your reservation canceled");
                                    userConditions[chatId] = Conditions.EnableToreserve;
                                    break;
                                }
                            default:
                                {
                                    var dataInStr = response.Split(' ');
                                    DateTime date;
                                    var room = "";
                                    if (dataInStr[0].Contains('%'))
                                    {
                                        room = $"room: {dataInStr[1]}";
                                        var dateInStr = dataInStr[0].Split('%');
                                        date = DateTime.Parse(dateInStr[0] + " " + dateInStr[1].Remove(0, 2));
                                    }
                                    else
                                    {
                                        room = $"room: {dataInStr[2]}";
                                        date = DateTime.Parse(dataInStr[0] + " " + dataInStr[1]);
                                    }

                                    await botClient.SendTextMessageAsync(chatId, $"are you satisfied with this time? (yes/no)\n {date} {room}");

                                    break;
                                }
                        }
                        break;
                    }
            }


        }

        private static string SendPreferencesRequest(string query, long chatId, Conditions condition, string command = "default")
        {
            try
            {
                var request = WebRequest.Create($"http://localhost:8888/{condition}/{command}/{chatId}/{query}");
                var response = request.GetResponse();
                var responseStream = response.GetResponseStream();

                using (var reader = new StreamReader(responseStream))
                {
                    var content = reader.ReadToEnd();
                    return content;
                }
            }
            catch (Exception e)
            { Console.WriteLine(e); }
            return "";
        }
    }
}
