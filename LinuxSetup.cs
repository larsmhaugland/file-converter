using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

class LinuxSetup
{
   public static void Setup()
    {
        Directory.SetCurrentDirectory("../../../");
        checkInstallGhostScript();
    }

    /// <summary>
    /// Builds GhostScript from source.
    /// </summary>
    private static void BuildGhostScript() {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        string ghostScriptPath = "ghostpdl";
        string ghostScriptBuildCommand = "./autogen.sh && ./configure --prefix=$HOME/local && make && make install";

        //open terminal
        startInfo = new ProcessStartInfo();
        startInfo.FileName = "/bin/bash";
        startInfo.Arguments = ghostScriptBuildCommand;
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
    }

    /// <summary>
    /// Checks if GhostScript is installed, if not, asks the user if they want to install it
    /// </summary>
    private static void checkInstallGhostScript()
    {
        //Check if GhostScript is installed
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "/bin/bash";
        startInfo.Arguments = "-c \" " + "gs --version" + " \"";
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardError = true;
        startInfo.RedirectStandardOutput = true;

        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
        string output = process.StandardOutput.ReadToEnd();
        if (!output.Contains("GPL Ghostscript"))
        {
            Console.WriteLine("GhostScript is not installed, would you like to install it? (y/n)");
            Console.WriteLine("Note: autoconf and automake is needed for the installation, see https://ghostscript.readthedocs.io/en/latest/Make.html#macos-or-linux-openbsd for how to install it using brew.");
            Console.WriteLine("Or download it directly from GNU: https://www.gnu.org/software/autoconf/");
            string? r = Console.ReadLine();
            r = r?.ToUpper() ?? " ";
            if (r == "Y")
            {
                Console.WriteLine("Note: The installation will take a while, please be patient.");
                Console.WriteLine("Installing GhostScript...");
                //Call function to build GhostScript
                BuildGhostScript();

                //Check if GhostScript is installed
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                output = process.StandardOutput.ReadToEnd();
                if (!output.Contains("GPL Ghostscript"))
                {
                    Console.WriteLine("GhostScript installation failed, please install it manually.");
                    Console.WriteLine("For more information, see https://ghostscript.readthedocs.io/en/latest/index.html");
                }
                else
                {
                    Console.WriteLine("GhostScript installed successfully.");
                }
            }
        }
    }
}