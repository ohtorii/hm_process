using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;


namespace hm_process
{
    public class DllInterface
    {
        const int FALSE=1;
        const int TRUE=1;
        
		[DllExport]
		public static IntPtr Finish()
		{
			ProcessHolder.Destroy();
			return new IntPtr(TRUE);
		}

		[DllExport]
        public static IntPtr Spawn(IntPtr filename, IntPtr arguments)
        {
			try
			{
				return new IntPtr(
							ProcessHolder.Spawn(
								Marshal.PtrToStringAuto(filename),
								Marshal.PtrToStringAuto(arguments)));
			}
			catch (Exception)
			{
				//pass
			}
			return new IntPtr(ProcessHolder.INVALID_HANDLE);
		}

		[DllExport]
		public static IntPtr SpawnWithRedirect(IntPtr filename, IntPtr arguments, IntPtr redirect_stndard_output, IntPtr redirect_standard_error)
		{
            try{
    			return new IntPtr(
    						ProcessHolder.SpawnWithRedirect(
    							Marshal.PtrToStringAuto(filename), 
    							Marshal.PtrToStringAuto(arguments), 
    							redirect_stndard_output.ToInt32()==0?false:true,
    							redirect_standard_error.ToInt32()==0?false:true));
            }catch (Exception){
                //pass
            }
            return new IntPtr(ProcessHolder.INVALID_HANDLE);
		}

		[DllExport]
		public static IntPtr SpawnWithRedirectEx(IntPtr filename, IntPtr arguments, IntPtr redirect_stndard_output, IntPtr redirect_standard_error, IntPtr redirect_standard_input)
		{
            try{
    			return new IntPtr(
    						ProcessHolder.SpawnWithRedirect(
    							Marshal.PtrToStringAuto(filename),
    							Marshal.PtrToStringAuto(arguments),
    							redirect_stndard_output.ToInt32() == 0 ? false : true,
    							redirect_standard_error.ToInt32() == 0 ? false : true,
    							redirect_standard_input.ToInt32() == 0 ? false : true));
            }catch (Exception){
                //pass
            }
            return new IntPtr(ProcessHolder.INVALID_HANDLE);
		}

		[DllExport]
		public static IntPtr SetArguments(IntPtr handle, IntPtr arguments)
		{
            try{
    			ProcessHolder.SetArguments(handle.ToInt32(),Marshal.PtrToStringAuto(arguments));
    			return new IntPtr(TRUE);
			}catch (Exception){
               	//pass
            }
			return new IntPtr(FALSE);
		}

		[DllExport]
		public static IntPtr SetCreateNoWindow(IntPtr handle, IntPtr value)
		{
            try{
    			ProcessHolder.SetCreateNoWindow(handle.ToInt32(), value.ToInt32()==0?false:true);
    			return new IntPtr(TRUE);
			}catch (Exception){
               	//pass
            }
			return new IntPtr(FALSE);
		}

		[DllExport]
		public static IntPtr SetWorkingDirectory(IntPtr handle, IntPtr value)
		{
            try{
			    ProcessHolder.SetWorkingDirectory(handle.ToInt32(), Marshal.PtrToStringAuto(value));
			    return new IntPtr(TRUE);
			}catch (Exception){
               	//pass
            }
			return new IntPtr(FALSE);
		}

		[DllExport]
		public static IntPtr Start(IntPtr handle)
		{
            try{
			    ProcessHolder.Start(handle.ToInt32());
			    return new IntPtr(TRUE); 
			}catch (Exception){
               	//pass
            }			
			return new IntPtr(FALSE);
		}

		[DllExport]
		public static IntPtr ReadStandardOutput(IntPtr handle)
		{
            try{
			    return ProcessHolder.ReadStandardOutput(handle.ToInt32());
			}catch (Exception){
               	//pass
            }
            return new IntPtr(0);
		}

		[DllExport]
		public static IntPtr ReadStandardError(IntPtr handle)
		{
            try{
			    return ProcessHolder.ReadStandardError(handle.ToInt32());
			}catch (Exception){
                //pass
            }
            return new IntPtr(0);
		}

		[DllExport]
		public static IntPtr ReadStandardOutputAll()
		{
            try{
    			return ProcessHolder.ReadStandardOutputAll();
			}catch (Exception){
                //pass
            }
            return new IntPtr(0);
		}

		[DllExport]
		public static IntPtr ReadStandardErrorAll()
		{
            try{
			    return ProcessHolder.ReadStandardErrorAll();
			}catch (Exception){
                //pass
            }
            return new IntPtr(0);
		}

		[DllExport]
		public static IntPtr WaitForExit(IntPtr handle)
		{
            try{
    			ProcessHolder.WaitForExit(handle.ToInt32());
    			return new IntPtr(TRUE);
			}catch (Exception){
                //pass
            }
			return new IntPtr(FALSE);
		}

		[DllExport]
		public static IntPtr WaitForExitWithTimeOut(IntPtr handle,IntPtr timeout_msec)
		{
            try{
    			ProcessHolder.WaitForExit(handle.ToInt32(),timeout_msec.ToInt32());
    			return new IntPtr(TRUE);
			}catch (Exception){
                //pass
            }
			return new IntPtr(FALSE);
		}

		[DllExport]
		public static IntPtr HasExited(IntPtr handle)
		{
            try{
    			ProcessHolder.HasExited(handle.ToInt32());
    			return new IntPtr(TRUE);
			}catch (Exception){
                //pass
            }
			return new IntPtr(FALSE); 
		}
		[DllExport]
		public static IntPtr ExitCode(IntPtr handle)
		{
            try{
    			return new IntPtr(ProcessHolder.ExitCode(handle.ToInt32()));
			}catch (Exception){
                //pass
            }
            return new IntPtr(0);
		}

		[DllExport]
		public static IntPtr Destroy(IntPtr handle)
		{
            try{
    			ProcessHolder.Destroy(handle.ToInt32());
    			return new IntPtr(TRUE);
			}catch (Exception){
                //pass
            }
			return new IntPtr(FALSE);  
		}
		

		[DllExport]
		public static IntPtr DllDetachFunc_After_Hm866(IntPtr n)
		{
			ProcessHolder.Destroy();
			return new IntPtr(0);
		}
	}
}
