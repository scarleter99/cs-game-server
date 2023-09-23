using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>(); // Room에 속한 Session
        JobQueue _jobQueue = new JobQueue(); // 수행할 Job 목록
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>(); // 등록된 Send 데이터

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        // _pendingList의 모든 데이터를 전체 플레이어에 Send
        public void Flush()
        {
            // N ^ 2
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        // 전체 플레이어에게 Send할 패킷을 _pendingList에 추가
        public void Broadcast(ArraySegment<byte> segment)
        {
            _pendingList.Add(segment);
        }

        public void Enter(ClientSession session)
        {
            // 새 플레이어 추가
            _sessions.Add(session);
            session.Room = this;

            // 새 플레이어한테 전체 플레이어 목록 전송
            S_PlayerList players = new S_PlayerList();
            foreach (ClientSession s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                });
            }
            session.Send(players.Write());

            // 전체 플레이어에게 알림
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            Broadcast(enter.Write());
        }

        public void Leave(ClientSession session)
        {
            // 플레이어 제거
            _sessions.Remove(session);

            // 전체 플레이어에게 알림
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionId;
            Broadcast(leave.Write());
        }

        // 플레이어 이동
        public void Move(ClientSession session, C_Move packet)
        {
            // 좌표 변경
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            // 전체 플레이어에게 알림
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
            Broadcast(move.Write());
        }
    }
}
