@echo off

set SOURCE_FOLDER=.\protos

set CS_COMPILER_PATH=.\grpc-protoc_windows_x64-1.41.0-dev\protoc.exe
set GRPC_CS_PLUGIN_PATH=.\grpc-protoc_windows_x64-1.41.0-dev\grpc_csharp_plugin.exe
set GRPC_PY_PLUGIN_PATH=.\grpc-protoc_windows_x64-1.41.0-dev\grpc_python_plugin.exe

set CS_TARGET_PATH=..\..\CSClient\Assets\Scripts\Protos
REM set PY_TARGET_PATH=..\..\PyServer\protos
set PY_TARGET_PATH=..\..\PyServer

del %CS_TARGET_PATH%\*.* /f /s /q
REM del %PY_TARGET_PATH%\*.* /f /s /q
del %PY_TARGET_PATH%\protos\*.* /f /s /q

for /f "delims=" %%i in ('dir /b "%SOURCE_FOLDER%\*.proto"') do (

    echo %CS_COMPILER_PATH% %SOURCE_FOLDER%\%%i --csharp_out=%CS_TARGET_PATH%
    REM %CS_COMPILER_PATH% %SOURCE_FOLDER%\%%i --csharp_out=%CS_TARGET_PATH%
    REM %CS_COMPILER_PATH% %SOURCE_FOLDER%\%%i --python_out=%PY_TARGET_PATH%
    
    REM %CS_COMPILER_PATH% %SOURCE_FOLDER%\%%i -I=%SOURCE_FOLDER% --csharp_out=%CS_TARGET_PATH% --grpc_out=%CS_TARGET_PATH% --plugin=protoc-gen-grpc=%GRPC_CS_PLUGIN_PATH% --csharp_opt=internal_access
    %CS_COMPILER_PATH% %SOURCE_FOLDER%\%%i -I=%SOURCE_FOLDER% --csharp_out=%CS_TARGET_PATH% --grpc_out=%CS_TARGET_PATH% --plugin=protoc-gen-grpc=%GRPC_CS_PLUGIN_PATH%
    REM %CS_COMPILER_PATH% %SOURCE_FOLDER%\%%i -I=%SOURCE_FOLDER% --python_out=%PY_TARGET_PATH% --grpc_out=%PY_TARGET_PATH% --plugin=protoc-gen-grpc=%GRPC_PY_PLUGIN_PATH%
    %CS_COMPILER_PATH% %SOURCE_FOLDER%\%%i -I=. --python_out=%PY_TARGET_PATH% --grpc_out=%PY_TARGET_PATH% --plugin=protoc-gen-grpc=%GRPC_PY_PLUGIN_PATH%
)

pause
