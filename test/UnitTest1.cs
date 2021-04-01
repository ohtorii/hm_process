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
				Assert.AreNotSame(ProcessHolder.INVALID_HANDLE, handle);

				bool success = ProcessHolder.SetCreateNoWindow(handle, true);
				Assert.AreEqual(true, success);

				success = ProcessHolder.Start(handle);
				Assert.AreEqual(true, success);

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
				success = ProcessHolder.Destroy(handle);
				Assert.AreEqual(true, success);
			}

			//出力があるはず
			Assert.AreEqual(true, has_output);
		}
	}
}
