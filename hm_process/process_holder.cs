using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;


namespace hm_process
{
	class process_holder
	{
		class process
		{
			public ProcessStartInfo _start_info;
			public Process _process;
		}

		public static int Spawn(string filename, string arguments)
		{
			var item = new process();
			item._start_info = new ProcessStartInfo(filename, arguments);
			item._start_info.UseShellExecute = true;

			var current_handle = _next_handle;
			_process[current_handle] = item;
			++_next_handle;
			return current_handle;
		}

		public static bool Start(int handle)
		{
			try
			{
				_process[handle]._process = Process.Start(_process[handle]._start_info);
				return true;
			}
			catch (Exception)
			{
				//pass
			}
			return false;
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


		static int _next_handle = 1;
		static Dictionary<int, process> _process = new Dictionary<int, process>();

	}
}
