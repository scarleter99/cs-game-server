using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
	public class SendBufferHelper
	{
		// 현재 쓰레드에서 사용할 Send 버퍼 참조값 보관
		public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

		public static int ChunkSize { get; set; } = 65535 * 100; // 생성할 Send 버퍼 크기

        // SendBuffer에 데이터를 담기 전 실행
        public static ArraySegment<byte> Open(int reserveSize)
		{
			if (CurrentBuffer.Value == null)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			if (CurrentBuffer.Value.FreeSize < reserveSize)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			return CurrentBuffer.Value.Open(reserveSize);
		}

        // SendBuffer에 데이터를 담은 후 실행
        public static ArraySegment<byte> Close(int usedSize)
		{
			return CurrentBuffer.Value.Close(usedSize);
		}
	}

	public class SendBuffer
	{
		byte[] _buffer;
		int _usedSize = 0; // Write 커서

		public int FreeSize { get { return _buffer.Length - _usedSize; } }

		public SendBuffer(int chunkSize)
		{
			_buffer = new byte[chunkSize];
		}

		// SendBuffer에 데이터를 담기 전 실행
		public ArraySegment<byte> Open(int reserveSize)
		{
			if (reserveSize > FreeSize)
				return null;

            // 요청 크기만큼 빈 버퍼 반환
            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
		}

        // SendBuffer에 데이터를 담은 후 실행
        public ArraySegment<byte> Close(int usedSize)
		{
            // 데이터가 담긴 버퍼 반환
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
			_usedSize += usedSize;
			return segment;
		}
	}
}
