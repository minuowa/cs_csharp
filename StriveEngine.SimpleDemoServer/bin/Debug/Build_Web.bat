: tiny build batch by chinagamedev@gmail.com

:---------------------------------------------------------------------------
: config!
set VCVAR="C:\Program Files (x86)\Microsoft Visual Studio 8\VC\bin\vcvars32.bat"
set CLIENT_PATH=E:\newwork\client\TKE_Client\Client
set RUNTIME_PATH=E:\newwork\client\TKE_Runtime\
set	RESOURCE_PATH=E:\newwork\_Resource\ClientRes130514
:---------------------------------------------------------------------------


call %VCVAR%

devenv %CLIENT_PATH%\Client.sln /rebuild ShippingPlugin
devenv %RUNTIME_PATH%\PluginRuntime\PluginRuntime.sln /rebuild Release

copy %RUNTIME_PATH%\output\PluginRuntime.dll %RESOURCE_PATH%\PluginRuntime.dll

