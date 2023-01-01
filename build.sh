for value in linux-x64 linux-arm linux-arm64 win-x64 win-x86 osx-x64 osx-arm64
do
    echo $value
    mkdir -p out/$value
    dotnet publish -c Release -r $value -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true  
    cp bin/Release/net7.0/$value/publish/* out/$value
done;