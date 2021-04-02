using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hm_process
{
	class RedirectStdin
	{
		bool				stopRequest;
		CancellationToken	cancellationToken;
		StreamWriter		dstProcessStdIn;
		Object				locker=new Object();

		public RedirectStdin(StreamWriter writer, CancellationToken token)
		{
			stopRequest = false;
			dstProcessStdIn = writer;
			cancellationToken = token;
		}
		public void Start()
		{
			stopRequest = false;
			Task.Run(() => MainLoop());
		}
		public void Stop()
		{
			stopRequest = true;

			lock (locker)
			{
				dstProcessStdIn.Close();
				dstProcessStdIn = null;
			}
		}
		void MainLoop()
		{
			while (! ExitRequest())
			{
				var srcProcessStdIn = Console.In.ReadLine();
				if (srcProcessStdIn.Length == 0)
				{
					continue;
				}
				//自プロセスのstdinの文字列を、相手先プロセスのstdinへ書き込む
				lock (locker)
				{
					if (dstProcessStdIn != null)
					{
						dstProcessStdIn.WriteLineAsync(srcProcessStdIn);
					}
				}
				//dstProcessStdIn.WriteLine(srcProcessStdIn);
			}
		}
		bool ExitRequest()
		{
			if (stopRequest) {
				return true;
			}
			return cancellationToken.IsCancellationRequested;
		}
	}
}
