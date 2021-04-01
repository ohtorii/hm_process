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
			try
			{
				var newProcess=ProcessInstance.Spawn(filename, arguments);
				var current_handle = _next_handle;
				_holder[current_handle] = newProcess;
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
			if (_holder == null)
			{
				return INVALID_HANDLE;
			}
			try
			{
				var newProcess = ProcessInstance.SpawnWithRedirect(filename, arguments, redirect_stndard_output, redirect_standard_error, redirect_standard_input);
				var current_handle = _next_handle;
				_holder[current_handle] = newProcess;
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
				_holder[handle].SetArguments(argments);
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
				_holder[handle].SetCreateNoWindow(value);
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
				_holder[handle].SetWorkingDirectory(value);
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
				_holder[handle].Start();
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
			try
			{
				return _holder[handle].ReadStandardOutput();
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
			try
			{
				return _holder[handle].ReadStandardError();
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
				_holder[handle].WaitForExit();
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
				return _holder[handle].WaitForExit(timeout);
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
				return _holder[handle].HasExited();
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
				return _holder[handle].ExitCode();
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
				ProcessInstance process;

				lock (_holder)
				{
					process = _holder[handle];
					_holder.Remove(handle);
				}
				return process.Destroy();
			}
			catch (Exception)
			{
				//pass
			}
			return false;
		}

		static int _next_handle = FIRST_HANDLE;
		static Dictionary<int, ProcessInstance> _holder = new Dictionary<int, ProcessInstance>();
	}
}
