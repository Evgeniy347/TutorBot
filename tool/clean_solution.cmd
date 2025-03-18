for /d /r ..\src\. %%d in (bin,obj) do @if exist "%%d" echo "%%d" & rd /s/q "%%d" 
  