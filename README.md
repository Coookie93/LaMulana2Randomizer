# La Mulana 2 Randomizer
Before playing a run it is best to head over to https://github.com/Coookie93/LaMulana2Randomizer/wiki for more information about the randomizer.

## Installing:
1. Get the lastest version from https://github.com/Coookie93/LaMulana2Randomizer/releases
2. Place the `LaMulana2Randomizer` folder in the La-Mulana 2 root directory, the one with `lamaulana2.exe`
3. Copy all the files from the `LaMulana2Randomizer/Monomod` folder to the `LaMulana2_Data/Managed` folder
4. Now in the `LaMulana2_Data/Managed` folder, drag the `Assembly-CSharp.dll` onto `monomod.exe`
5. Make a backup of `Assembly-CSharp.dll` eg. create an `Original` folder inside `LaMulana2_Data/Managed` and place the file in there.
6. Rename the `MONOMODDED_Assembly-CSharp.dll `file to `Assembly-CSharp.dll`
7. Now open the `LaMulana2Randomizer.exe` program, create a seed and you're good to go

## Updating:
1. Get the lastest version from https://github.com/Coookie93/LaMulana2Randomizer/releases
2. If new version supports older seeds copy current seed if you want it.
3. Delete the `Assembly-CSharp.dll` in the `LaMulana2_Data/Managed` folder
4. Copy `LaMulana2_Data/Managed/Original/Assembly-CSharp.dll` back to `LaMulana2_Data/Managed/Assembly-CSharp.dll`.
5. Replace current `LaMulana2Randomizer` folder with the newer version.
6. Refer to steps 3-6 on the [Installing](#installing) section.
7. Copy old seeds into the seed folder if that is applicable.

## Unistall:
1. Delete the `Assembly-CSharp.dll` in the `LaMulana2_Data/Managed` folder
2. Choose ONE of the following:
    * Copy `LaMulana2_Data/Managed/Original/Assembly-CSharp.dll` back to `LaMulana2_Data/Managed/Assembly-CSharp.dll`.
    * Use Steam's file verification and it will redownload the original `Assembly-CSharp.dll`
3. Remove `Assembley-CSharp.mm.dll` and all monomod related files from the `LaMulana2_Data/Managed` folder

## Compiling:
The randomisation application should compile fine.

The patch library will require both installing La-Mulana 2 and setting the system environment variable `LAMULANA2PATH` before building. Set the variable to where La-Mulana 2 is installed. Refer to [How to change Environment Variables on Windows 10](https://www.architectryan.com/2018/08/31/how-to-change-environment-variables-on-windows-10/) for additional information. Make sure the environment variable is set before launching Visual Studio.
