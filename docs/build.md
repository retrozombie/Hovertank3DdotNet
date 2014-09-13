Build Instructions
==================

Contents

1. [Windows](#Windows)
2. [Linux](#Linux)

Windows
-------

The batch files are configured to use the .NET Framework v2.0 and expect to find the framework in the folder: *C:\Windows\Microsoft.NET\Framework\v2.0.50727*

Using explorer navigate to the windows make folder: *\make\windows*

###WinForms

1. Double-click the batch file: ```make_winforms.bat```

2. Copy the *.exe* file to the hovertank game folder.

3. Copy the default configuration file *\resources\config\CONFIG.HOV* to the hovertank game folder.

###OpenTK

1. Install OpenTK: http://www.opentk.com/ and make a note of the intallation path.

2. It may be necessary to edit *make_opentk.bat* and change the path. The path expected by the batch file is: *C:\Users\%USERNAME%\Documents\OpenTK\1.1\Binaries\OpenTK\Release\OpenTK.dll*

3. Double-click the batch file: ```make_opentk.bat```

4. Copy the *.exe* file to the hovertank game folder.

5. Copy the default configuration file *\resources\config\CONFIG.HOV* to the hovertank game folder.

6. Copy the files *OpenTK.dll* and *OpenTK.dll.config* from the *\OpenTK\1.1\Binaries\OpenTK\Release* folder to the hovertank folder.

###SlimDX

1. Install SlimDX January 2012 for .NET2.0: http://slimdx.org/download.php

2. Double-click the batch file: ```make_slimdx.bat```

3. Copy the *.exe* file to the hovertank game folder.

4. Copy the default configuration file *\resources\config\CONFIG.HOV* to the hovertank game folder.

Linux
-----

Install Mono if necessary.

###OpenTK

1. Get the OpenTK binaries from SourceForge: http://sourceforge.net/projects/opentk/files/opentk/opentk-1.1/.

2. Unzip and copy the files *OpenTK.dll* and *OpenTK.dll.config* from */Binaries/OpenTK/Release/* to the ```/make/linux``` folder.

3. In a terminal, run the shell script: ```./make_opentk.sh```

4. Copy the *.exe*, *OpenTK.dll* and *OpenTK.dll.config* files to the hovertank game folder.

5. Copy the default configuration file *\resources\config\CONFIG.HOV* to the hovertank game folder.

6. To run the game, change to the hovertank game folder and enter: ```mono Hovertank3DdotNet_OTK.exe```
