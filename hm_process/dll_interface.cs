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
			return process_holder.Spawn(Marshal.PtrToStringAuto(filename), Marshal.PtrToStringAuto(arguments));
		}

		[DllExport]
		public static IntPtr Start(IntPtr handle)
		{
			if (process_holder.Start(handle.ToInt32()))
			{
				return new IntPtr(1);	//true
			}
			return new IntPtr(0);		//false
		}

		[DllExport]
		public static IntPtr WaitForExit(IntPtr handle)
		{
			if (process_holder.WaitForExit(handle.ToInt32()))
			{
				return new IntPtr(1);   //true
			}
			return new IntPtr(0);   //false
		}

		[DllExport]
		public static IntPtr WaitForExitWithTimeOut(IntPtr handle,IntPtr timeout_msec)
		{
			if (process_holder.WaitForExit(handle.ToInt32(),timeout_msec.ToInt32()))
			{
				return new IntPtr(1);   //true
			}
			return new IntPtr(0);   //false
		}

		[DllExport]
		public static IntPtr HasExited(IntPtr handle)
		{
			if (process_holder.HasExited(handle.ToInt32()))
			{
				return new IntPtr(1);   //true
			}
			return new IntPtr(0);   //false
		}

		[DllExport]
		public static IntPtr ExitCode(IntPtr handle)
		{
			return new IntPtr(process_holder.ExitCode(handle.ToInt32()));
		}
	}
}
