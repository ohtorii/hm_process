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
			return SpawnWithRedirect(filename, arguments, redirect_stndard_output, redirect_standard_error, false);
		}
		public static int SpawnWithRedirect(string filename, string arguments, bool redirect_stndard_output, bool redirect_standard_error, bool redirect_standard_input)
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

				start.RedirectStandardOutput = redirect_stndard_output;
				start.RedirectStandardError = redirect_standard_error;
				start.RedirectStandardInput = redirect_standard_input;
				item._process.Exited += item.ExitedHandler;

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
			catch (Exception)
			{
				//pass
			}
			return INVALID_HANDLE;
		}

		public static bool SetArguments(int handle, string argments)
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

				if (item._process.StartInfo.RedirectStandardOutput)
				{
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

		public static string ReadStandardOutputAsString(int handle)
		{
			try
			{
				return _process[handle]._stdout.ReadAsString();
			}
			catch (Exception)
			{
				//pass
			}
			return "";
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

		public static string ReadStandardErrorAsString(int handle)
		{
			try
			{
				return _process[handle]._stderr.ReadAsString();
			}
			catch (Exception)
			{
				//pass
			}
			return "";
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

		public static string ReadStandardOutputAllAsString()
		{
			try
			{
				string all = "";
				foreach (var item in _process)
				{
					all += item.Value._stdout.ReadAsString();
				}
				return all;
			}
			catch (Exception)
			{
				//pass
			}
			return "";
		}

		public static IntPtr ReadStandardOutputAll()
		{
			try
			{
				FreeStaticStdoutString();
				var all = ReadStandardOutputAllAsString();
				_static_stdout_string = Marshal.StringToHGlobalUni(all);
				return _static_stdout_string;
			}
			catch (Exception)
			{
				//pass
			}
			return new IntPtr();
		}

		public static string ReadStandardErrorAllAsString()
		{
			try
			{
				string all = "";
				foreach (var item in _process)
				{
					all += item.Value._stderr.ReadAsString();
				}
				return all;
			}
			catch (Exception)
			{
				//pass
			}
			return "";
		}

		public static IntPtr ReadStandardErrorAll()
		{
			try
			{
				FreeStaticStderrString();
				var all = ReadStandardErrorAllAsString();
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
