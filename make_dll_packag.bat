@echo off
setlocal

set ROOT=%~dp0


call :Main
exit /b %errorlevel%

:Main
	mkdir "%ROOT%\dll_package\" >nul 2>&1
	if not exist "%ROOT%\dll_package\" (
		echo �f�B���N�g���쐬�Ɏ��s
		exit /b 1
	)

	copy /B /Y "%ROOT%\hm_process\bin\Release\x86\hm_process.dll" "%ROOT%\dll_package\hm_process_x86.dll"
	if %errorlevel% NEQ 0 (
        exit /b %errorlevel%
    )

	copy /B /Y "%ROOT%\hm_process\bin\Release\x64\hm_process.dll" "%ROOT%\dll_package\hm_process_x64.dll"
	if %errorlevel% NEQ 0 (
        exit /b %errorlevel%
    )
    echo dll_package �f�B���N�g���ȉ���DLL���R�s�[���܂���
	exit /b 0
