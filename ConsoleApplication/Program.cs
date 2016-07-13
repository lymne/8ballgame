using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using ConsoleApplication.Helper;
using Fleck;
using Newtonsoft.Json;

namespace TopoWebSocket
{

    public class Message
    {
        public string action { get; set; }
        public bool ishit { get; set; }
        public HitParams HitParams { get; set; }
        public int score1 { get; set; }
        public int score2 { get; set; }
        public List<Position> positionArray { get; set; }
    }
    //击球参数
    public class HitParams
    {
        public decimal px { get; set; }
        public decimal py { get; set; }
        public decimal x { get; set; }
        public decimal y { get; set; }
    }

    public class Position
    {
        public decimal x { get; set; }
        public decimal y { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://192.168.1.112:7181");
            var player1Port = 0;
            var player2Port = 0;
            var score1 = 0;
            var score2 = 0;
            var currentPlayer = 0;

            bool player1ready = false;
            bool player2ready = false;
            bool canUpdatePosition = false;
            List<Position> positionArray = null;



            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    if (allSockets.Count < 2)
                    {
                        allSockets.Add(socket);
                        var json = new { action = "prepare", data = allSockets.Count };
                        allSockets.ToList().ForEach(s => s.Send(JsonConvert.SerializeObject(json)));
                        if (allSockets.Count == 2)
                        {
                            player1Port = allSockets[0].ConnectionInfo.ClientPort;
                            player2Port = allSockets[1].ConnectionInfo.ClientPort;
                            currentPlayer = player1Port;
                            score1 = 0;
                            score2 = 0;
                        }
                    }
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Close!");
                    allSockets.Remove(socket);
                    score1 = 0;
                    score2 = 0;
                    positionArray = null;
                    canUpdatePosition = false;
                };
                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);
                    var msg = JsonConvert.DeserializeObject<Message>(message);
                    switch (msg.action)
                    {
                        case "ready":
                            checkReady(allSockets, socket, player1Port, currentPlayer);
                            break;
                        case "start":
                            if (socket.ConnectionInfo.ClientPort == player1Port)
                            {
                                player1ready = true;
                            }
                            else if (socket.ConnectionInfo.ClientPort == player2Port)
                            {
                                player2ready = true;
                            }
                            if (player1ready && player2ready)
                            {
                                UpdateCallBack(currentPlayer, allSockets);
                            }
                            break;
                        case "update":
                            UpdateCallBack(currentPlayer, allSockets);
                            break;
                       
                        case "hit":
                            HitParams hitParams = msg.HitParams;
                            allSockets.ToList().ForEach(m => m.Send(JsonConvert.SerializeObject(new
                            {
                                action = "hitCallback",
                                hitParams,
                            })));
                            break;
                        case "hitpocket":
                            if (socket.ConnectionInfo.ClientPort == currentPlayer && currentPlayer == player1Port)
                            {
                                score1 += 100;
                            }
                            else
                            {
                                score2 += 100;
                            }
                            var json = new { action = "updateScore", score1, score2 };
                            allSockets.ToList().ForEach(s => s.Send(JsonConvert.SerializeObject(json)));
                            break;
                        //当前玩家打完球，主球停止后发送球组坐标，更新服务器保存的最新坐标
                        case "updatePosition":

                            positionArray = msg.positionArray;//更新服务器保存的最新坐标
                            //向等待同步的玩家发送  准许请求坐标信号
                             var other = allSockets.FirstOrDefault(m => m.ConnectionInfo.ClientPort != currentPlayer);
                             other.Send(JsonConvert.SerializeObject(new {
                                     action = "canRequestPosition",
                              }));
                            //切换当前玩家并通知双方
                            if (socket.ConnectionInfo.ClientPort == currentPlayer)
                            {
                                currentPlayer =
                                    allSockets.FirstOrDefault(m => m.ConnectionInfo.ClientPort != currentPlayer)
                                        .ConnectionInfo.ClientPort;
                            }

                            UpdateCallBack(currentPlayer, allSockets);
                            break;
                         //回调给请求更新球体坐标的客户端
                        case "positionCallback":
                            socket.Send(JsonConvert.SerializeObject(new
                            {
                                action = "positionCallback",
                                positionArray,
                            }));
                            break;
                        default:
                            break;
                    }

                };
            });

            var input = Console.ReadLine();
            while (input != "exit")
            {
                foreach (var socket in allSockets.ToList())
                {
                    socket.Send(input);
                }
                input = Console.ReadLine();
            }
        }




        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="currentPlayer">当前玩家端口</param>
        /// <param name="allSockets"></param>
        /// <param name="param">击球参数</param>
        /// <returns></returns>
        private static void UpdateCallBack(int currentPlayer, List<IWebSocketConnection> allSockets)
        {
            allSockets.FirstOrDefault(m => m.ConnectionInfo.ClientPort == currentPlayer).Send(JsonConvert.SerializeObject(new
            {
                action = "updateCallback",
                msg = "Your Move",
                code = 1
            }));
            var other = allSockets.FirstOrDefault(m => m.ConnectionInfo.ClientPort != currentPlayer);
            other.Send(JsonConvert.SerializeObject(new
             {
                 action = "updateCallback",
                 msg = "Your Opponent Move",
                 code = 0,
             }));
        }
        /// <summary>
        /// 检查开局
        /// </summary>
        /// <param name="allSockets"></param>
        /// <param name="socket"></param>
        /// <param name="player1Port"></param>
        private static void checkReady(List<IWebSocketConnection> allSockets, IWebSocketConnection socket, int player1Port, int currentPlayer)
        {
            //必须由玩家1开局
            bool isReady = allSockets.Count == 2 && socket.ConnectionInfo.ClientPort == player1Port;
            allSockets.ToList().ForEach(s => s.Send(JsonConvert.SerializeObject(new
            {
                action = "ready",
                isReady
            })));
            //UpdateCallBack(socket, currentPlayer, false, allSockets, null);
        }



    }

}