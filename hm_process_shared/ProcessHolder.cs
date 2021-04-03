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
	public class ProcessHolder
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
			if (_holder != null)
			{
				lock (_holder)
				{
					foreach (var item in _holder)
					{
						item.Value.Destroy();
					}
					_holder.Clear();
				}
			}

			_next_handle = FIRST_HANDLE;

			FreeStaticStdoutString();
			FreeStaticStderrString();
		}

		public static int Spawn(string filename, string arguments)
		{
			if (_holder == null)
			{
				return INVALID_HANDLE;
			}
			
			var newProcess=ProcessInstance.Spawn(filename, arguments);
			var current_handle = _next_handle;
			_holder[current_handle] = newProcess;
			++_next_handle;
			return current_handle;
		}

		public static int SpawnWithRedirect(string filename, string arguments, bool redirect_stndard_output, bool redirect_standard_error)
		{
			return SpawnWithRedirect(filename, arguments, redirect_stndard_output, redirect_standard_error, false);
		}
		public static int SpawnWithRedirect(string filename, string arguments, bool redirect_stndard_output, bool redirect_standard_error, bool redirect_standard_input)
		{
			if (_holder == null)
			{
				return INVALID_HANDLE;
			}
			var newProcess = ProcessInstance.SpawnWithRedirect(filename, arguments, redirect_stndard_output, redirect_standard_error, redirect_standard_input);
			var current_handle = _next_handle;
			_holder[current_handle] = newProcess;
			++_next_handle;
			return current_handle;
		}

		public static void SetArguments(int handle, string argments)
		{
		    _holder[handle].SetArguments(argments);
		}

		public static void SetCreateNoWindow(int handle, bool value)
		{
			_holder[handle].SetCreateNoWindow(value);
		}

		public static void SetWorkingDirectory(int handle, string value)
		{
			_holder[handle].SetWorkingDirectory(value);
		}

		public static void Start(int handle)
		{
			_holder[handle].Start();
		}

		public static string ReadStandardOutputAsString(int handle)
		{
			try
			{
				return _holder[handle].ReadStandardOutputAsString();
			}
			catch (Exception)
			{
				//pass
			}
			return "";
		}

		public static IntPtr ReadStandardOutput(int handle)
		{
			return _holder[handle].ReadStandardOutput();
		}

		public static string ReadStandardErrorAsString(int handle)
		{
			try
			{
				return _holder[handle].ReadStandardErrorAsString();
			}
			catch (Exception)
			{
				//pass
			}
			return "";
		}

		public static IntPtr ReadStandardError(int handle)
		{
			return _holder[handle].ReadStandardError();
		}
		public static void WriteLineStandardInput(int handle, string line)
		{
			_holder[handle].WriteLineStandardInput(line);
		}
		public static void WriteStandardInput(int handle, string str)
		{
			_holder[handle].WriteStandardInput(str);
		}
		/// <summary>
		/// 標準入力を閉じる（Ctrl-Cに相当する）
		/// </summary>
		/// <param name="handle"></param>
		public static void CloseStandardInput(int handle)
		{
			_holder[handle].CloseStandardInput();
		}
		/// <summary>
		/// 起動した全プロセスの標準出力を取得する
		/// </summary>
		/// <returns></returns>
		public static string ReadStandardOutputAllAsString()
		{
			try
			{
				string all = "";
				foreach (var item in _holder)
				{
					all += item.Value.ReadStandardOutputAsString();
				}
				return all;
			}
			catch (Exception)
			{
				//pass
			}
			return "";
		}
		/// <summary>
		/// 起動した全プロセスの標準出力を取得する
		/// </summary>
		/// <returns></returns>
		public static IntPtr ReadStandardOutputAll()
		{

			FreeStaticStdoutString();
			var all = ReadStandardOutputAllAsString();
			_static_stdout_string = Marshal.StringToHGlobalUni(all);
			return _static_stdout_string;
		}
		/// <summary>
		/// 起動した全プロセスの標準エラーを取得する
		/// </summary>
		/// <returns></returns>
		public static string ReadStandardErrorAllAsString()
		{
			try
			{
				string all = "";
				foreach (var item in _holder)
				{
					all += item.Value.ReadStandardErrorAsString();
				}
				return all;
			}
			catch (Exception)
			{
				//pass
			}
			return "";
		}
		/// <summary>
		/// 起動した全プロセスの標準エラーを取得する
		/// </summary>
		/// <returns></returns>
		public static IntPtr ReadStandardErrorAll()
		{
			FreeStaticStderrString();
			var all = ReadStandardErrorAllAsString();
			_static_stderr_string = Marshal.StringToHGlobalUni(all);
			return _static_stderr_string;
		}

		public static void Kill(int handle)
		{
			_holder[handle].Kill();
		}
		public static void WaitForExit(int handle)
		{
			_holder[handle].WaitForExit();
		}

		public static bool WaitForExit(int handle, int timeout)
		{
			return _holder[handle].WaitForExit(timeout);
		}

		public static bool HasExited(int handle)
		{
			return _holder[handle].HasExited();
		}

		public static int ExitCode(int handle)
		{
			return _holder[handle].ExitCode();
		}

		public static void Destroy(int handle)
		{
			ProcessInstance process;
			lock (_holder)
			{
				process = _holder[handle];
				_holder.Remove(handle);
			}
			process.Destroy();
		}

		static int _next_handle = FIRST_HANDLE;
		static Dictionary<int, ProcessInstance> _holder = new Dictionary<int, ProcessInstance>();
	}
}
