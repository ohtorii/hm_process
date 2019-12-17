# hm_process
 
# はじめに

秀丸エディタから複数の外部プロセスを扱うためのDLLです。


# 使い方

使い方はC#のSystem.Diagnostics.Processクラスと同等です。

利用頻度の高いプロセスの起動とリダイレクトに関するメソッドの一部を移植しました、通常の使い方であれば機能に過不足は無いと思います。

## プロセスの終了を待つ
	/*プロセス（メモ帳）を起動し終了するまで待つ
	*/
	debuginfo 1;

	#dll=loaddll(currentmacrodirectory+"\\..\\hm_process\\bin\\Release\\x64\\hm_process.dll");
	#handle=dllfuncw(#dll,"Spawn","notepad","");
	#success=dllfuncw(#dll,"Start",#handle);

	#counter=0;
	#exit=0;
	while(#exit==0){
		#exit=dllfuncw(#dll,"WaitForExitWithTimeOut",#handle,1000);

		/*ここで独自の処理（バックグランドタスク）を動かすことが出来ます。
		*/
		debuginfo"#counter="+str(#counter);
		#counter = #counter + 1;
	}

	#success=dllfuncw(#dll,"Destroy",#handle);
	freedll(#dll);
	debuginfo"Finish";


## 標準出力と標準エラーを非同期に取得する

	/*標準出力と標準エラーを取得する
	*/
	newfile;
	debuginfo 1;
	#dll=loaddll(currentmacrodirectory+"\\..\\hm_process\\bin\\Release\\x64\\hm_process.dll");
	#handle=dllfuncw(#dll,"SpawnWithRedirect","cmd.exe","",true,true);

	//メモ：約5000個のファイルを列挙します。（環境依存ですが・・・）
	#success=dllfuncw(#dll,"SetArguments",#handle,"/cdir c:\\Windows\\System32");
	//黒いウインドウを非表示にする
	#success=dllfuncw(#dll,"SetCreateNoWindow",#handle,true);
	//プロセスを開始する
	#success=dllfuncw(#dll,"Start",#handle);

	#exit=false;
	while(#exit==false){
		#exit=dllfuncw(#dll,"HasExited",#handle);

		//
		//標準出力と標準エラーを取得します。
		//
		insert dllfuncstrw(#dll,"ReadStandardOutput",#handle);
		insert dllfuncstrw(#dll,"ReadStandardError",#handle);

		//
		//何か別の処理（バックグランドタスク）をここで行います。
		//
	}
	//
	//残りの標準出力とエラーを取得します。
	//
	$text=dllfuncstrw(#dll,"ReadStandardOutput",#handle);
	while($text!=""){
		insert $text;
		$text=dllfuncstrw(#dll,"ReadStandardOutput",#handle);
	}
	$text=dllfuncstrw(#dll,"ReadStandardError",#handle);
	while($text!=""){
		insert $text;
		$text=dllfuncstrw(#dll,"ReadStandardError",#handle);
	}
	//
	//終了処理
	//
	#success=dllfuncw(#dll,"Destroy",#handle);
	freedll(#dll);
	debuginfo "@finish";


## 複数のプロセスを起動し終了を待つ

準備中


# 秀丸エディタへの導入方法

後で書く

# DLLのビルド方法

後で書く

|ファイル名|説明|
|--|--|
|DllExport.bat|C#のDLLをC++から利用出来るようにするバッチファイル|
|make_dll_packag.bat|ビルド済みDLLをdll_packageフォルダへコピーする|

# 導入プロジェクト

以下プロジェクトで利用しています。

[Unity](https://github.com/ohtorii/unity)


# 謝辞

C#でDLLを作るにあたり以下の方に感謝します。

- [DllExport](https://github.com/3F/DllExport)
- [C++からC# DLL 超超超入門](https://qiita.com/Midoliy/items/58d56e202f104ebf867a)

# 連絡先

- <https://ohtorii.hatenadiary.jp>
- <https://twitter.com/ohtorii>
- <https://github.com/ohtorii>
