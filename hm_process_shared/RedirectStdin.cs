using System;
using System.Diagnostics;
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
				if (dstProcessStdIn != null)
				{
					dstProcessStdIn.Close();
					dstProcessStdIn = null;
				}
			}
		}
		void MainLoop()
		{
			Debug.WriteLine("[Stdin] MainLoop start");
			try
			{
				while (!ExitRequest())
				{
					/*Debug.WriteLine("[Stdin] Peek start.");
					if (Console.In.Peek() == -1)
					{
						Debug.WriteLine("[Stdin] Peek continue.");
						continue;
					}
					*/
					Debug.WriteLine("[Stdin] ReadLine start.");
					//var c = Console.In.Read();
					var srcStr = Console.In.ReadLine();
					Debug.WriteLine("[Stdin] ReadLine finish.");
					//自プロセスのstdinの文字列を、相手先プロセスのstdinへ書き込む
					lock (locker)
					{
						if (dstProcessStdIn != null)
						{
							if (srcStr == null)
							{
								//Ctrl-C / Ctrl-Z
								dstProcessStdIn.Close();
								dstProcessStdIn = null;

								Debug.WriteLine("[Stdin] Close");

								return;
							}

							Debug.Write(string.Format("[Stdin] Len={0}: {1}", srcStr.Length, srcStr));
							dstProcessStdIn.WriteLine(srcStr);
							
							
							//Debug.Write(string.Format("[Stdin] {1}", c));
							//dstProcessStdIn.Write(c);
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(string.Format("[Stdin-Err] Exception!!",e.ToString()));
			}

			Debug.WriteLine("[Stdin] MainLoop finish");
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
