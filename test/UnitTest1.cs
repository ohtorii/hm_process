using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using hm_process;
namespace test
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void SpawnWithRedirect()
		{
			var has_output = false;
			{
				var handle = ProcessHolder.SpawnWithRedirect("cmd.exe", "/c dir c:\\", true, true);
				Assert.AreNotEqual(ProcessHolder.INVALID_HANDLE, handle);

				ProcessHolder.SetCreateNoWindow(handle, true);
				ProcessHolder.Start(handle);
				var exit = false;
				while (exit == false)
				{
					exit = ProcessHolder.HasExited(handle);

					//
					//標準出力と標準エラーを取得します。
					//
					if (ProcessHolder.ReadStandardOutputAsString(handle) != "")
					{
						has_output = true;
					}
					if (ProcessHolder.ReadStandardErrorAsString(handle) != "")
					{
						has_output = true;
					}
				}

				//
				//残りの標準出力とエラーを取得します。
				//
				var text = ProcessHolder.ReadStandardOutputAsString(handle);
				while (text != "")
				{
					text = ProcessHolder.ReadStandardOutputAsString(handle);
					has_output = true;
				}
				text = ProcessHolder.ReadStandardErrorAsString(handle);
				while (text != "")
				{
					text = ProcessHolder.ReadStandardErrorAsString(handle);
					has_output = true;
				}
				//
				//終了処理
				//
				ProcessHolder.Destroy(handle);
			}

			//出力があるはず
			Assert.AreEqual(true, has_output);
		}

		[TestMethod]
		public void StandardInput()
		{
			var handle = ProcessHolder.SpawnWithRedirect("sort.exe", "", true, true, true);
			Assert.AreNotEqual(ProcessHolder.INVALID_HANDLE, handle);

			ProcessHolder.SetCreateNoWindow(handle, true);
			ProcessHolder.Start(handle);

			//ソート対象の文字列
		    ProcessHolder.WriteLineStandardInput(handle, "xyz");
            ProcessHolder.WriteLineStandardInput(handle,"abc");
            ProcessHolder.WriteLineStandardInput(handle,"3");
			
			//Ctl-c 相当
			ProcessHolder.CloseStandardInput(handle);

			ProcessHolder.WaitForExit(handle);

			{
				var textOutput = ProcessHolder.ReadStandardOutputAllAsString();
				Assert.AreEqual("3\nabc\nxyz", textOutput);
			}
			{
				var textError = ProcessHolder.ReadStandardErrorAllAsString();
				Assert.AreEqual("", textError);
			}

			ProcessHolder.Destroy(handle);
		}

		[TestMethod]
		public void ProcessKill()
		{
			var handle = ProcessHolder.SpawnWithRedirect("notepad.exe", "", false, false);
			Assert.AreNotEqual(ProcessHolder.INVALID_HANDLE, handle);

		    ProcessHolder.Start(handle);
			
			//少し待ってからプロセスをキルする
			System.Threading.Thread.Sleep(1000);

			ProcessHolder.Kill(handle);
			ProcessHolder.Destroy(handle);
		}
	}
}
