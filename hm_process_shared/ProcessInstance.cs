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
		public static ProcessInstance Spawn(string filename, string arguments)
		{
			var newProcess = new ProcessInstance();
			var start = newProcess._process.StartInfo;

			start.FileName = filename;
			start.Arguments = arguments;
			return newProcess;
		}
		public static ProcessInstance SpawnWithRedirect(string filename, string arguments, bool redirect_stndard_output, bool redirect_standard_error, bool redirect_standard_input)
		{
			var newProcess = new ProcessInstance();
			var start = newProcess._process.StartInfo;

			start.FileName = filename;
			start.Arguments = arguments;

			start.RedirectStandardOutput = redirect_stndard_output;
			start.RedirectStandardError = redirect_standard_error;
			start.RedirectStandardInput = redirect_standard_input;
			newProcess._process.Exited += newProcess.ExitedHandler;

			start.UseShellExecute = false;
			if (redirect_stndard_output)
			{
				newProcess._process.OutputDataReceived += newProcess._stdout.Received;
			}
			if (redirect_standard_error)
			{
				newProcess._process.ErrorDataReceived += newProcess._stderr.Received;
			}
			return newProcess;
		}

		public void SetArguments(string argments)
		{
			_process.StartInfo.Arguments = argments;
		}
		public void SetCreateNoWindow(bool value){
    		_process.StartInfo.CreateNoWindow = value;
		}
		public void SetWorkingDirectory(string value){
    		_process.StartInfo.WorkingDirectory = value;
		}
		public void Start(){
    		_process.Start();
			if (_process.StartInfo.RedirectStandardOutput)
			{
				_process.BeginOutputReadLine();
			}
			if (_process.StartInfo.RedirectStandardError)
			{
				_process.BeginErrorReadLine();
			}
		}
        public void WaitForExit(){
            _process.WaitForExit();
        }
        public bool WaitForExit(int timeout){
            return _process.WaitForExit(timeout);
        }
        public bool HasExited(){
            return _process.HasExited;
        }
        public int ExitCode(){
            return _process.ExitCode;
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
		
		Process  _process = new Process();
		public Redirect _stdout = new Redirect();
		public Redirect _stderr = new Redirect();

		CancellationTokenSource _cancellationTokenSource =new CancellationTokenSource();
		//public bool _auto_destroy=false;///プロセス終了時に自動削除するかどうか。
	}

	

}
