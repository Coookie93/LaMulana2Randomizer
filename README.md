# La Mulana 2 Randomizer
Before playing a run it is best to head over to https://github.com/Coookie93/LaMulana2Randomizer/wiki for more information about the randomizer.

## Installing:
0. Get the lastest version from https://github.com/Coookie93/LaMulana2Randomizer/releases
1. Place the LaMulana2Randomizer folder in the La Mulana 2 root directory, the one with lamaulana2.exe
2. Copy all the files from the LaMulana2Randomizer/Monomod folder to the LaMulana2_Data/Managed folder
3. Now in the LaMulana2_Data/Managed folde, drag the Assembly-CSharp.dll onto monomod.exe
4. Make a backup of Assembly-CSharp.ddl eg. rename to Assembly-CSharp.ddl.backup
5. Rename the MONOMODDED_Assembly-CSharp.dll file to Assembly-CSharp.dll
6. Now open the LaMulana2Randomizer.exe create a seed and you're good to go

## Updating:
0. Get the lastest version from https://github.com/Coookie93/LaMulana2Randomizer/releases
1. If new version supports older seeds copy current seed if you want it.
2. Delete the Assembly-CSharp.dll in the LaMulana2_Data/Managed folder
3. Rename your backed up version of Assembly-CSharp.dll back too Assembly-CSharp.dll
4. Replace current LaMulana2Randomizer folder with the newer version.
5. Copy all the files from the LaMulana2Randomizer/Monomod folder to the LaMulana2_Data/Managed folder
6. Now in the LaMulana2_Data/Managed folde, drag the Assembly-CSharp.dll onto monomod.exe
7. Make a backup of Assembly-CSharp.ddl eg. create an `Original` folder inside `LaMulana2_Data/Managed` and place the file in there.
8. Rename the MONOMODDED_Assembly-CSharp.dll file to Assembly-CSharp.dll
9. Copy old seeds into the seed folder if that is applicable.

## Unistall:
0. Delete the Assembly-CSharp.dll in the LaMulana2_Data/Managed folder
1. Rename your backed up version of Assembly-CSharp.dll back too Assembly-CSharp.dll or
Use Steam's file verification and it will redownload the original Assembly-CSharp.dll
2. Remove Assembley-CSharp.mm.dll and all monomod related files from the LaMulana2_Data/Managed folder

## Compiling:
The randomisation application should compile fine.

The patch library will require both installing La-Mulana 2 and setting the system environment variable `LAMULANA2PATH` before building. Set the variable to where La-Mulana 2 is installed. Refer to [How to change Environment Variables on Windows 10](https://www.architectryan.com/2018/08/31/how-to-change-environment-variables-on-windows-10/) for additional information. Make sure the environment variable is set before launching Visual Studio.
