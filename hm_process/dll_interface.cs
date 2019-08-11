using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace hm_process
{
    public class dll_interface
    {
        [DllExport]
        public static int Spawn(IntPtr filename, IntPtr arguments)
        {
			return ProcessHolder.Spawn(Marshal.PtrToStringAuto(filename), Marshal.PtrToStringAuto(arguments));
		}

		[DllExport]
		public static int SpawnWithRedirect(IntPtr filename, IntPtr arguments, IntPtr redirect_stndard_output, IntPtr redirect_standard_error)
		{
			return ProcessHolder.SpawnWithRedirect(
					Marshal.PtrToStringAuto(filename), 
					Marshal.PtrToStringAuto(arguments), 
					redirect_stndard_output.ToInt32()==0?false:true,
					redirect_standard_error.ToInt32()==0?false:true);
		}

		[DllExport]
		public static IntPtr Start(IntPtr handle)
		{
			if (ProcessHolder.Start(handle.ToInt32()))
			{
				return new IntPtr(1);	//true
			}
			return new IntPtr(0);		//false
		}

		[DllExport]
		public static IntPtr ReadStandardOutput(IntPtr handle)
		{
			return ProcessHolder.ReadStandardOutput(handle.ToInt32());
		}

		[DllExport]
		public static IntPtr ReadStandardError(IntPtr handle)
		{
			return ProcessHolder.ReadStandardError(handle.ToInt32());
		}

		[DllExport]
		public static IntPtr WaitForExit(IntPtr handle)
		{
			if (ProcessHolder.WaitForExit(handle.ToInt32()))
			{
				return new IntPtr(1);   //true
			}
			return new IntPtr(0);   //false
		}

		[DllExport]
		public static IntPtr WaitForExitWithTimeOut(IntPtr handle,IntPtr timeout_msec)
		{
			if (ProcessHolder.WaitForExit(handle.ToInt32(),timeout_msec.ToInt32()))
			{
				return new IntPtr(1);   //true
			}
			return new IntPtr(0);   //false
		}

		[DllExport]
		public static IntPtr HasExited(IntPtr handle)
		{
			if (ProcessHolder.HasExited(handle.ToInt32()))
			{
				return new IntPtr(1);   //true
			}
			return new IntPtr(0);   //false
		}
		[DllExport]
		public static IntPtr ExitCode(IntPtr handle)
		{
			return new IntPtr(ProcessHolder.ExitCode(handle.ToInt32()));
		}

		[DllExport]
		public static IntPtr Destroy(IntPtr handle)
		{
			if (ProcessHolder.Destroy(handle.ToInt32()))
			{
				return new IntPtr(1);   //true
			}
			return new IntPtr(0);   //false
		}


	}
}
