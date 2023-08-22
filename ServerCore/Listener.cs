using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
	public class Listener
	{
		Socket _listenSocket;
		Func<Session> _sessionFactory; // 생성할 Session을 반환하는 Delegate

		public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
		{
			_listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			_sessionFactory += sessionFactory;

			// Bind
			_listenSocket.Bind(endPoint);

			// Listen
			// 인자는 클라이언트 최대 대기수
			_listenSocket.Listen(10);

			// 이벤트 객체 생성 후 Accept 작업 등록
			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

			RegisterAccept(args);
		}

		// Accept 작업 등록
		void RegisterAccept(SocketAsyncEventArgs args)
		{
			args.AcceptSocket = null;

			// 비동기 Accept
			bool pending = _listenSocket.AcceptAsync(args);
			if (pending == false)
				OnAcceptCompleted(null, args);
		}

		// Accept 작업 Callback 함수
		void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
		{
			// Session 생성 후 소켓 할당
			if (args.SocketError == SocketError.Success)
			{
				Session session = _sessionFactory.Invoke();
				session.Start(args.AcceptSocket);
				session.OnConnected(args.AcceptSocket.RemoteEndPoint);
			}
			else
				Console.WriteLine(args.SocketError.ToString());

			// 다음 Accpet 작업 등록
			RegisterAccept(args);
		}
	}
}
