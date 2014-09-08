set path=%path%;C:\Windows\Microsoft.NET\Framework\v2.0.50727
cls
csc /noconfig /reference:C:\Users\%USERNAME%\Documents\OpenTK\1.1\Binaries\OpenTK\Release\OpenTK.dll @cs_opentk.txt @cs_hovertank.txt
pause
