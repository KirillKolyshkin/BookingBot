using BookingBot.Core.Entities;
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
                        //var userName = tokens[3];
                        //if (repository.ContainUser(userName))
                        //{
                        //    SendResponse(context, "no");
                        //}
                        //else
                        //{
                        //    repository.AddUser(chatId, userName);
                            SendResponse(context, "ok");
                        //}
                        break;
                    }
                case "EnableToreserve":
                    {
                        var commandtoken = tokens[3];
                        Console.WriteLine("   " + commandtoken);
                        var chatId = long.Parse(tokens[2]);
                        switch (commandtoken)
                        {
                            case "ShowAllMyRes":
                                {
                                    try
                                    {
                                        Console.WriteLine(chatId);
                                        var reservations = repository.ResereveSessionsOfUser(chatId);

                                        Console.WriteLine(reservations.Count());
                                        //if (!reservations.Any())
                                        //{
                                        //    Console.WriteLine("sending response");
                                        //    SendResponse(context, "haven't any res");
                                        //    break;
                                        //}

                                        StringBuilder builder = new StringBuilder();
                                        foreach (var r in reservations)
                                            builder.Append( repository.ReservationToStr(r) + "\n");
                                        SendResponse(context, builder.ToString());
                                        break;
                                    }
                                    catch
                                    {
                                        Console.WriteLine("sending response");
                                        SendResponse(context, "haven't any res");
                                        break;
                                    }
                                }
                        }
                        break;
                    }
                case "ChoosingRoom":
                    {
                        var command = tokens[1];
                        switch (command)
                        {
                            case "addRoom":
                                {
                                    var roomNum = int.Parse(tokens[3]);
                                    //Console.WriteLine("added");
                                    if (!repository.ContainRoom(roomNum))
                                    {
                                        repository.AddClassroom(roomNum);
                                        //repository.SaveChanges();
                                        Console.WriteLine("added");
                                    }

                                    //
                                    //

                                    SendResponse(context, "ok");
                                    break;
                                }
                            case "default":
                                {
                                    var chatId = long.Parse(tokens[2]);
                                    var dateInStr = tokens[3].Split('%');
                                    var date = DateTime.Parse(dateInStr[0] + " " + dateInStr[1].Remove(0, 2));
                                    var roomNum = int.Parse(tokens[4]);
                                    var reservations = repository.ReserveSessionInDay(date, roomNum);

                                    if (!reservations.Any())
                                    {
                                        SendResponse(context, "haven't any res");
                                        break;
                                    }

                                    StringBuilder builder = new StringBuilder();
                                    foreach (var r in reservations)
                                        builder.Append(repository.ReservationToStr(r) + "\n");
                                    SendResponse(context, builder.ToString());
                                    break;
                                }
                        }
                        break;
                    }
                case "ApproveReservation":
                    {
                        var command = tokens[1];
                        var chatId = long.Parse(tokens[2]);
                        //var date = new DateTime();
                        Console.WriteLine(command);
                        switch (command)
                        {
                            case "yes":
                                {
                                    var dateInStr = tokens[3].Split('%');
                                    var date = DateTime.Parse(dateInStr[0] + " " + dateInStr[1].Remove(0, 2));
                                    var roomNum = int.Parse(tokens[4]);
                                    var timeSession = new TimeSession(date, new TimeSpan(0, 45, 0), Guid.NewGuid());
                                    var resp = "";
                                    if (repository.EnableToReserve(timeSession, roomNum))
                                    {
                                        repository.AddReservation(chatId, date, roomNum);
                                        resp = "ok";
                                    }
                                    else
                                    {
                                        resp = "no";
                                    }
                                    SendResponse(context, resp);
                                    break;
                                }
                            case "no":
                                {
                                    break;
                                }
                            case "findNearest":
                                {
                                    var roomNum = int.Parse(tokens[3]);
                                    Console.WriteLine(roomNum);
                                    var data = repository.FindNearest(chatId, roomNum, new TimeSpan(0, 45, 0));
                                    Console.WriteLine(data.Id);
                                    var room = repository.GetRumById(data.RoomId);
                                    Console.WriteLine(room.RoomNumber);
                                    var date = repository.GetTimeSessionById(data.TimeSessionId);
                                    Console.WriteLine(date.StartTime.ToString() + " " + room.RoomNumber);
                                    SendResponse(context, date.StartTime.ToString() + " " + room.RoomNumber);
                                    break;
                                }
                            case "data":
                                {
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
