using System;
using System.Net;
using System.Threading;
using ServerCore;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void FlushRoom()
        {
            // JobQueue Flush 이후 JobQueue Flush 다시 예약
            Room.Push(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 250);
        }

        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            // 첫 JobQueue Flush 예약
            JobTimer.Instance.Push(FlushRoom);

            // 실행 시간에 도달한 JobTimer Flush
            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}
