using BookingBot.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BookingBot.Application
{
    class Server
    {
        public Server()
        {
            repository = new Repository();
        }


        Repository repository;// = new Repository();

        private void SendResponse(HttpListenerContext context, string data)
        {
            HttpListenerResponse response = context.Response;
            // создаем ответ в виде кода html
            string responseStr = data;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseStr);
            // получаем поток ответа и пишем в него ответ
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // закрываем поток
            output.Close();

        }

        public void HandleRequest(HttpListenerContext context)
        {
            //Console.WriteLine("message resived");
            var tokens = context.Request.Url.AbsolutePath.TrimStart('/').Split('/');
            var switchtoken = tokens[0];
            Console.WriteLine(switchtoken);
            switch (switchtoken)
            {
                case "Initialize":
                    {
                        var chatId = long.Parse(tokens[2]);
                        var userName = tokens[3];
                        if (repository.ContainUser(chatId))
                        {
                            SendResponse(context, "no");
                        }
                        else
                        {
                            repository.AddUser(chatId, userName);
                            SendResponse(context, "ok");
                        }
                        break;
                    }
                case "EnableToreserve":
                    {
                        var commandtoken = tokens[1];
                        var chatId = long.Parse(tokens[2]);
                        switch (commandtoken)
                        {
                            case "ShowAllMyRes":
                                {
                                    var reservations = repository.ResereveSessionsOfUser(chatId);

                                    if (!reservations.Any())
                                    {
                                        SendResponse(context, "haven't any res");
                                        break;
                                    }

                                    StringBuilder builder = new StringBuilder();
                                    foreach (var r in reservations)
                                        builder.Append(r.ToString() + "\n");
                                    SendResponse(context, builder.ToString());
                                    break;
                                }
                        }
                        break;
                    }
                case "ChoosingRoom":
                    {
                        //var commandtoken = tokens[1];
                        var chatId = long.Parse(tokens[2]);
                        var date = DateTime.Parse(tokens[3]);
                        var roomNum = int.Parse(tokens[4]);
                        var reservations = repository.ReserveSessionInDay(date, roomNum);

                        if (!reservations.Any())
                        {
                            SendResponse(context, "haven't any res");
                            break;
                        }
                        
                        StringBuilder builder = new StringBuilder();
                        foreach (var r in reservations)
                            builder.Append(r.ToString() + "\n");
                        SendResponse(context, builder.ToString());
                        break;

                    }
                case "ApproveReservation":
                    {
                        var command = tokens[1];
                        var chatId = long.Parse(tokens[2]);
                        var date = new DateTime();
                        var roomNum = 1;
                        switch (command)
                        {
                            case "yes":
                                {
                                    repository.AddReservation(chatId, date, roomNum);
                                    break;
                                }
                            case "no":
                                {
                                    break;
                                }
                            case "data":
                                {
                                    date = DateTime.Parse(tokens[3]);
                                    roomNum = int.Parse(tokens[4]);
                                    SendResponse(context, tokens[3] + "  " + tokens[4]);
                                    break;
                                }
                        }
                        break;
                    }
            }
            // $"http://localhost:8888/{condition}/{command}/{chatId}/{query}"

        }
    }
}
