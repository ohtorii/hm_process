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
		CancellationToken	cancellationToken;
		StreamWriter		streamWriter;
		public RedirectStdin(StreamWriter writer, CancellationToken token)
		{
			streamWriter = writer;
			cancellationToken = token;
		}
		public void StartASync()
		{
			Task.Run(() => MainLoop());
		}
		void MainLoop()
		{
			while (cancellationToken.IsCancellationRequested==false)
			{
				var line = Console.In.ReadLine();
				streamWriter.WriteLine(line);
			}
		}
	}
}
