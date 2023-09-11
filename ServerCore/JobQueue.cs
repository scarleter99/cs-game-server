using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
	// JobQueue 인터페이스
	public interface IJobQueue
	{
		void Push(Action job);
	}

	public class JobQueue : IJobQueue
	{
		Queue<Action> _jobQueue = new Queue<Action>();
		object _lock = new object();
		bool _flush = false;

		public void Push(Action job)
		{
            // 다른 쓰레드에서 flush 중이 아니면 flush
            bool flush = false;

			lock (_lock)
			{
				_jobQueue.Enqueue(job);
				if (_flush == false)
					flush = _flush = true;
			}

			if (flush)
				Flush();
		}

		// _jopQueue의 모든 Job 처리
		void Flush()
		{
			while (true)
			{
				Action action = Pop();
				if (action == null)
					return;

				action.Invoke();
			}
		}

		Action Pop()
		{
			lock (_lock)
			{
				if (_jobQueue.Count == 0)
				{
					_flush = false;
					return null;
				}
				return _jobQueue.Dequeue();
			}
		}
	}
}
