using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
	public class Connector
	{
		Func<Session> _sessionFactory; // 생성할 Session을 반환하는 Delegate

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
		{
			Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			_sessionFactory = sessionFactory;

			// 이벤트 객체 생성 후 Connect 작업 등록
			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			args.Completed += OnConnectCompleted;
			args.RemoteEndPoint = endPoint; // 연결할 서버 주소 설정
			args.UserToken = socket; // 사용자 정의 데이터로 사용할 소켓 설정

			RegisterConnect(args);
		}

		// Connect 작업 등록
		void RegisterConnect(SocketAsyncEventArgs args)
		{
			Socket socket = args.UserToken as Socket;
			if (socket == null)
				return;

			// 비동기 Connect
			bool pending = socket.ConnectAsync(args);
			if (pending == false)
				OnConnectCompleted(null, args);
		}

        // Connect 작업 Callback 함수
        void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
		{
			if (args.SocketError == SocketError.Success)
			{
                // Session 생성 후 소켓 할당
                Session session = _sessionFactory.Invoke();
				session.Start(args.ConnectSocket);
				session.OnConnected(args.RemoteEndPoint);
			}
			else
			{
				Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
			}
		}
	}
}
