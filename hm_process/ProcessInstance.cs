using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace hm_process
{
	class ProcessInstance
	{
		public class Redirect
		{
			//
			//(memo)
			//秀丸エディタへ返す文字列の長さ
			//
			//秀丸エディタの文字列変数は最大で4MBの領域を利用できます。
			//そのため、返す文字列の最大長に制限を設けています。
			//
			const int _max_read_line = 100;

			public List<string> _lines = new List<string>();
			IntPtr _ptr = IntPtr.Zero;

			~Redirect()
			{
				if (_ptr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(_ptr);
					_ptr = IntPtr.Zero;
				}
				_lines = null;
			}

			public string ReadAsString()
			{
				string result;

				lock (_lines)
				{
					result = string.Join("\n", _lines);
					_lines.Clear();
				}
				return result;
			}

			public IntPtr ReadAsIntPtr()
			{
				//前回の文字列を解放する
				Marshal.FreeHGlobal(_ptr);
				_ptr = IntPtr.Zero;
				//今回の文字列を確保する
				_ptr = Marshal.StringToHGlobalUni(ReadAsString());
				return _ptr;
			}

			public void Received(object sender, DataReceivedEventArgs e)
			{
				if (e.Data == null)
				{
					return;
				}

				while (_max_read_line < _lines.Count)
				{
					Thread.Sleep(10);
				}

				lock (_lines)
				{
					_lines.Add(e.Data);
				}
			}
		}
		public void ExitedHandler(object sender, EventArgs e)
		{
			_cancellationTokenSource.Cancel();
		}
		public bool Destroy()
		{
			try
			{
				var p = _process;

				_process = null;
				_stderr = null;
				_stdout = null;

				try
				{
					p.Kill();
				}
				catch (Exception)
				{
					//pass
				}

				if (p.HasExited)
				{
					p.Close();
					return true;
				}

				try
				{
					p.CancelErrorRead();
					p.CancelOutputRead();
					p.CloseMainWindow();
				}
				catch (Exception)
				{
					//pass
				}

				try
				{
					p.Close();
				}
				catch (Exception)
				{
					//pass
				}
				return true;
			}
			catch (Exception)
			{
				//pass
			}
			return false;
		}
		
		public Process _process = new Process();
		public Redirect _stdout = new Redirect();
		public Redirect _stderr = new Redirect();

		CancellationTokenSource _cancellationTokenSource =new CancellationTokenSource();
		//public bool _auto_destroy=false;///プロセス終了時に自動削除するかどうか。
	}

	

}
