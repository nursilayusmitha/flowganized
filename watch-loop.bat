@echo off
setlocal enabledelayedexpansion

:MAINLOOP
cls
echo Flowganized Dev Helper
echo --------------------------------------
echo [1] Jalankan update + dotnet run
echo [2] Jalankan update + dotnet watch run
echo [Q] Keluar
echo --------------------------------------
set /p input="Pilih aksi (1/2/Q): "

if /i "%input%"=="1" (
    call :RUN_APP normal
    goto MAINLOOP
) else if /i "%input%"=="2" (
    call :RUN_APP watch
    goto MAINLOOP
) else if /i "%input%"=="q" (
    echo Keluar...
    exit /b
) else (
    echo Input tidak valid.
    timeout /t 2 >nul
    goto MAINLOOP
)

exit /b

:RUN_APP
set "mode=%1"
echo Menutup proses Flowganized.exe jika ada...
taskkill /f /im Flowganized.exe >nul 2>&1

:: Generate nama migration berdasarkan timestamp
set timestamp=%date:~-4%%date:~3,2%%date:~0,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set timestamp=%timestamp: =0%
set migrationName=AutoUpdate_%timestamp%

echo Menambahkan migration: %migrationName%...
dotnet ef migrations add %migrationName%
IF %ERRORLEVEL% NEQ 0 (
    echo Gagal membuat migration. Periksa error-nya.
    pause
    exit /b
)

echo Menjalankan migrasi database...
dotnet ef database update
IF %ERRORLEVEL% NEQ 0 (
    echo Gagal melakukan migrasi. Periksa error-nya.
    pause
    exit /b
)

echo Migrasi berhasil.

echo Menjalankan aplikasi...
if /i "!mode!"=="watch" (
    echo Menjalankan dotnet watch run...
    dotnet watch run
) else (
    echo Menjalankan dotnet run...
    dotnet run
)

goto :eof
