@echo off
setlocal

set ROOT=%~dp0


call :Main
exit /b %errorlevel%

:Main
	mkdir "%ROOT%\dll_package\"
	if not exist "%ROOT%\dll_package\" (
		echo �f�B���N�g���쐬�Ɏ��s
		exit /b 1
	)

	copy /B /Y "%ROOT%\hm_process\bin\Release\x86\hm_process.dll" "%ROOT%\dll_package\hm_process_x86.dll"
	
	copy /B /Y "%ROOT%\hm_process\bin\Release\x64\hm_process.dll" "%ROOT%\dll_package\hm_process_x64.dll"
	
	exit /b 0
	