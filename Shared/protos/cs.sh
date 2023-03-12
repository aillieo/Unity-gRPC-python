#!/bin/bash

SOURCE_FOLDER=./protos

CS_COMPILER_PATH=./grpc-protoc_macos_x64-1.41.0-dev/protoc
GRPC_CS_PLUGIN_PATH=./grpc-protoc_macos_x64-1.41.0-dev/grpc_csharp_plugin
GRPC_PY_PLUGIN_PATH=./grpc-protoc_macos_x64-1.41.0-dev/grpc_python_plugin

CS_TARGET_PATH=../../CSClient/Assets/Scripts/Protos
# set PY_TARGET_PATH=../../PyServer\protos
PY_TARGET_PATH=../../PyServer

rm -f $CS_TARGET_PATH/
# del %PY_TARGET_PATH%\*.* /f /s /q
# rm -rf $PY_TARGET_PATH/protos

for i in `ls $SOURCE_FOLDER/*.proto`
do

    echo $CS_COMPILER_PATH $SOURCE_FOLDER/$i --csharp_out=$CS_TARGET_PATH
    # %CS_COMPILER_PATH% %SOURCE_FOLDER%\%%i --csharp_out=%CS_TARGET_PATH%
    # %CS_COMPILER_PATH% %SOURCE_FOLDER%\%%i --python_out=%PY_TARGET_PATH%
    
    # %CS_COMPILER_PATH% %SOURCE_FOLDER%\%%i -I=%SOURCE_FOLDER% --csharp_out=%CS_TARGET_PATH% --grpc_out=%CS_TARGET_PATH% --plugin=protoc-gen-grpc=%GRPC_CS_PLUGIN_PATH% --csharp_opt=internal_access
    $CS_COMPILER_PATH $i -I=$SOURCE_FOLDER --csharp_out=$CS_TARGET_PATH --grpc_out=$CS_TARGET_PATH --plugin=protoc-gen-grpc=$GRPC_CS_PLUGIN_PATH
    # %CS_COMPILER_PATH% %SOURCE_FOLDER%\%%i -I=%SOURCE_FOLDER% --python_out=%PY_TARGET_PATH% --grpc_out=%PY_TARGET_PATH% --plugin=protoc-gen-grpc=%GRPC_PY_PLUGIN_PATH%
    $CS_COMPILER_PATH $i -I=. --python_out=$PY_TARGET_PATH --grpc_out=$PY_TARGET_PATH --plugin=protoc-gen-grpc=$GRPC_PY_PLUGIN_PATH
done

read
