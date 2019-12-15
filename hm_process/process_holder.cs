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
		public static readonly int INVALID_HANDLE = 0;
		public static readonly int FIRST_HANDLE = 1;
		static IntPtr _static_stdout_string = IntPtr.Zero;
		static IntPtr _static_stderr_string = IntPtr.Zero;


		~ProcessHolder()
		{
			Destroy();
		}

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
				IntPtr _ptr=IntPtr.Zero;

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
			
			public bool Destroy()
			{
				try {
					var p = _process;

					_process = null;
					_stderr = null;
					_stdout = null;

					try
					{
						p.Kill();
					}
					catch (Exception) {
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
			//public bool _auto_destroy=false;///プロセス終了時に自動削除するかどうか。
		}

		static void FreeStaticStdoutString()
		{
			if (_static_stdout_string == IntPtr.Zero)
			{
				return;
			}
			Marshal.FreeHGlobal(_static_stdout_string);
			_static_stdout_string = IntPtr.Zero;
		}

		static void FreeStaticStderrString()
		{
			if (_static_stderr_string == IntPtr.Zero)
			{
				return;
			}
			Marshal.FreeHGlobal(_static_stderr_string);
			_static_stderr_string = IntPtr.Zero;
		}

		public static void Destroy()
		{ 
			if (_process != null)
			{
				lock (_process)
				{
					foreach (var item in _process)
					{
						item.Value.Destroy();
					}
                    _process.Clear();
                }
			}

			_next_handle = FIRST_HANDLE;

			FreeStaticStdoutString();
			FreeStaticStderrString();
		}

		public static int Spawn(string filename, string arguments)
		{
			if (_process == null)
			{
				return INVALID_HANDLE;
			}
			try
			{
				var item = new ProcessInstance();
				var start = item._process.StartInfo;

				start.FileName = filename;
				start.Arguments = arguments;

				var current_handle = _next_handle;
				_process[current_handle] = item;
				++_next_handle;
				return current_handle;
			}
			catch (Exception)
			{
				//pass
			}
			return INVALID_HANDLE;
		}

		public static int SpawnWithRedirect(string filename, string arguments, bool redirect_stndard_output, bool redirect_standard_error)
		{
            if (_process == null)
			{
                return INVALID_HANDLE;
			}

			try
			{
                var item    = new ProcessInstance();
                var start   = item._process.StartInfo;

                start.FileName  = filename;
				start.Arguments = arguments;
				start.RedirectStandardOutput    = redirect_stndard_output;
				start.RedirectStandardError     = redirect_standard_error;

				start.UseShellExecute = false;
                if (redirect_stndard_output){
                    item._process.OutputDataReceived += item._stdout.Received;
				}

                if (redirect_standard_error){
                    item._process.ErrorDataReceived += item._stderr.Received;
				}

                var current_handle = _next_handle;
                _process[current_handle] = item;

                ++_next_handle;
                return current_handle;
			}
			catch (Exception)
			{
				//pass
			}
            return INVALID_HANDLE;
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
				/*if(_inspector==null){
					_inspector = Task.Run(() => InspectAsync(_inspector_token.Token));
				}
				*/

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
				return _process[handle]._stdout.ReadAsIntPtr();
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
				return _process[handle]._stderr.ReadAsIntPtr();
			}
			catch (Exception)
			{
				//pass
			}
			return new IntPtr();
		}

		public static IntPtr ReadStandardOutputAll()
		{
			try
			{
				FreeStaticStdoutString();
				string all="";
				foreach(var item in _process)
				{
					all += item.Value._stdout.ReadAsString();
				}
				_static_stdout_string = Marshal.StringToHGlobalUni(all);
				return _static_stdout_string;
			}
			catch (Exception)
			{
				//pass
			}
			return new IntPtr();
		}
		
		public static IntPtr ReadStandardErrorAll()
		{
			try
			{
				FreeStaticStderrString();
				string all = "";
				foreach (var item in _process)
				{
					all += item.Value._stderr.ReadAsString();
				}
				_static_stderr_string = Marshal.StringToHGlobalUni(all);
				return _static_stderr_string;
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
				ProcessInstance p;

				lock (_process)
				{
					p = _process[handle];
					_process.Remove(handle);
				}
				return p.Destroy();
			}
			catch (Exception)
			{
				//pass
			}
			return false;
		}

		/*
		static void InspectAsync(CancellationToken cancelToken)
		{
			var remove_handles = new List<int>();

			while (! cancelToken.IsCancellationRequested){
				lock (_process)
				{
					foreach (var item in _process)
					{
						if (!item.Value._auto_destroy)
						{
							continue;
						}
						if (! item.Value._process.HasExited)
						{
							continue;
						}
						remove_handles.Add(item.Key);
					}
				}

				foreach(var handle in remove_handles)
				{
					Destroy(handle);
				}
				remove_handles.Clear();
				
				Thread.Sleep(5000);

			}
		}
		*/

		static int _next_handle = FIRST_HANDLE;
		static Dictionary<int, ProcessInstance> _process = new Dictionary<int, ProcessInstance>();

		//static Task	_inspector=null;
		//static CancellationTokenSource _inspector_token=new CancellationTokenSource();
	}
}
