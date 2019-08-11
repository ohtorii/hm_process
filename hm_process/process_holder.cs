using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;



namespace hm_process
{
	class ProcessHolder
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
				const int _max_read_line=100;

				public List<string>	_lines = new List<string>();
				IntPtr _ptr;
				object _sync = new object();

				~Redirect()
				{
					Marshal.FreeHGlobal(_ptr);
					_ptr = IntPtr.Zero;
				}

				public IntPtr Read()
				{
					string result;

					lock (_sync)
					{
						result = string.Join("\n", _lines);
						_lines.Clear();
					}

					//前回の文字列を解放する
					Marshal.FreeHGlobal(_ptr);
					_ptr = IntPtr.Zero;
					//今回の文字列を確保する
					_ptr = Marshal.StringToHGlobalUni(result);
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

					lock (_sync)
					{
						_lines.Add(e.Data);
					}
				}
			}


			public Process _process = new Process();
			public Redirect _stdout = new Redirect();
			public Redirect _stderr = new Redirect();
		}

		public static int Spawn(string filename, string arguments)
		{
			var item = new ProcessInstance();
			var start = item._process.StartInfo;

			start.FileName = filename;
			start.Arguments= arguments;

			var current_handle = _next_handle;
			_process[current_handle] = item;
			++_next_handle;
			return current_handle;
		}

		public static int SpawnWithRedirect(string filename, string arguments, bool redirect_stndard_output, bool redirect_standard_error)
		{
			var item = new ProcessInstance();
			var start = item._process.StartInfo;

			start.FileName = filename;
			start.Arguments = arguments;
			start.RedirectStandardOutput = redirect_stndard_output;
			start.RedirectStandardError = redirect_standard_error;

			start.UseShellExecute = false;
			if (redirect_stndard_output)
			{
				item._process.OutputDataReceived += item._stdout.Received;
			}
			if (redirect_standard_error)
			{
				item._process.ErrorDataReceived += item._stderr.Received;
			}

			var current_handle = _next_handle;
			_process[current_handle] = item;
			++_next_handle;
			return current_handle;
		}

		public static bool SetArguments(int handle,string argments)
		{
			try
			{
				_process[handle]._process.StartInfo.Arguments = argments;
				return true;
			}
			catch (Exception)
			{
				//pass
			}
			return false;
		}

		public static bool SetCreateNoWindow(int handle, bool value)
		{
			try
			{
				_process[handle]._process.StartInfo.CreateNoWindow = value;
				return true;
			}
			catch (Exception)
			{
				//pass
			}
			return false;
		}

		public static bool SetWorkingDirectory(int handle, string value)
		{
			try
			{
				_process[handle]._process.StartInfo.WorkingDirectory = value;
				return true;
			}
			catch (Exception)
			{
				//pass
			}
			return false;
		}
		
		public static bool Start(int handle)
		{
			try
			{
				var item = _process[handle];
				item._process.Start();

				if(item._process.StartInfo.RedirectStandardOutput){
					item._process.BeginOutputReadLine();
				}
				if (item._process.StartInfo.RedirectStandardError)
				{
					item._process.BeginErrorReadLine();
				}
				return true;
			}
			catch (Exception)
			{
				//pass
			}
			return false;
		}

		public static IntPtr ReadStandardOutput(int handle)
		{
			try
			{
				return _process[handle]._stdout.Read();
			}
			catch (Exception)
			{
				//pass
			}
			return new IntPtr();
		}

		public static IntPtr ReadStandardError(int handle)
		{
			try
			{
				return _process[handle]._stderr.Read();
			}
			catch (Exception)
			{
				//pass
			}
			return new IntPtr();
		}

		public static bool WaitForExit(int handle)
		{
			try
			{
				_process[handle]._process.WaitForExit();
				return true;
			}
			catch (Exception)
			{
				//pass
			}
			return false;
		}

		public static bool WaitForExit(int handle, int timeout)
		{
			try
			{
				return _process[handle]._process.WaitForExit(timeout);
			}
			catch (Exception)
			{
				//pass
			}
			return false;
		}

		public static bool HasExited(int handle)
		{
			try
			{
				return _process[handle]._process.HasExited;
			}
			catch (Exception)
			{
				//pass
			}
			return false;
		}

		public static int ExitCode(int handle)
		{
			try
			{
				return _process[handle]._process.ExitCode;
			}
			catch (Exception)
			{
				//pass
			}
			return 0;
		}

		public static bool Destroy(int handle)
		{
			try
			{
				var p=_process[handle]._process;
				_process.Remove(handle);
				Debug.Write("C#@1");
				//p.Refresh();

				if (p.HasExited)
				{
					Debug.Write("C# Exit start");
					//p.CloseMainWindow();
					p.Close();
					Debug.Write("C# Exit finish");
					return true;
				}

				try
				{
					Debug.Write("C#@2");
					p.CancelErrorRead();
					Debug.Write("C#@3");
					p.CancelOutputRead();
					Debug.Write("C#@4");
					p.CloseMainWindow();
					Debug.Write("C#@5");
										//p.Kill();
					//p.WaitForExit(1000);
				}
				catch (Exception)
				{
					//pass
				}
				Debug.Write("C#@5.5");
				try
				{
					p.Close();
					Debug.Write("C#@6");
				}
				catch (Exception)
				{
					//pass
				}
				Debug.Write("C#@7");
				return true;
			}
			catch (Exception)
			{
				//pass
				Debug.Write("C#@8");
			}
			Debug.Write("C#@9");
			return false;
		}


		static int _next_handle = 1;
		static Dictionary<int, ProcessInstance> _process = new Dictionary<int, ProcessInstance>();

	}
}
