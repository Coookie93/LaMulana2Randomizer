using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using LM2Randomiser.Logging;

namespace LM2Randomiser.Modding
{
    public abstract class Modding
    {
        public static bool ModDLL()
        {
            const string managed = "LaMulana2_Data\\Managed";
            const string dllName = "Assembly-CSharp.dll";
            const string moddeddllName = "MONOMODDED_Assembly-CSharp.dll";

            string currentDir = Directory.GetCurrentDirectory();
            string parentDir = Directory.GetParent(currentDir).FullName;

            string dllDir = Path.Combine(currentDir, "Dlls");
            string managedDir = Path.Combine(parentDir, managed);

            string dllPath = Path.Combine(managedDir, dllName);
            string moddeddllPath = Path.Combine(dllDir, moddeddllName);
            string backupdllPath = Path.Combine(currentDir, "Assembly-CSharp.dll.Backup");


            //Make a backup of Assembly-CSharp.dll
            if (File.Exists(dllPath) && !File.Exists(backupdllPath))
            {
                File.Copy(dllPath, backupdllPath);
            }

            //Copy all the dlls the game uses so monomod use them too
            if (Directory.Exists(managedDir)) { 
                foreach (var file in Directory.GetFiles(managedDir))
                {
                    string copiedFile = Path.Combine(dllDir, Path.GetFileName(file));
                    if (!File.Exists(copiedFile))
                    {
                        File.Copy(file, copiedFile);
                    }
                    
                }
            }

            try
            {
                //start a process so that monomod can create the modded Assembly-CSharp.dll
                const string commandString = "monomod.exe Assembly-CSharp.dll";

                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + commandString);
                procStartInfo.WorkingDirectory = dllDir;
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                using (Process process = new Process())
                {
                    process.StartInfo = procStartInfo;
                    process.Start();

                    process.WaitForExit();

                    string result = process.StandardOutput.ReadToEnd();
                    Logger.GetLogger.Log(result);
                    if (result.Contains("Exception"))
                    {
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger.Log(ex.Message);
                return false;
            }

            //replace vanilla Assembly-CSharp.dll with the modded one
            if(File.Exists(moddeddllPath)) { 
                File.Copy(moddeddllPath, dllPath, true);
            }

            return true;
        }
    }
}
