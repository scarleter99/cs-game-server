using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>(); // 패킷번호 - 패킷 처리 함수
	Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>(); // 패킷번호 - 패킷 Handler 함수
		
	public void Register()
	{
		_onRecv.Add((ushort)PacketID.C_Chat, MakePacket<C_Chat>);
		_handler.Add((ushort)PacketID.C_Chat, PacketHandler.C_ChatHandler);

	}

	// 패킷 처리
	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer);
	}
	
	// 패킷 생성
	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
	{
		T pkt = new T();
		pkt.Read(buffer);
		Action<PacketSession, IPacket> action = null;
		if (_handler.TryGetValue(pkt.Protocol, out action))
			action.Invoke(session, pkt);
	}
}