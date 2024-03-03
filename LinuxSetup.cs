using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using iText.Commons.Utils;

class LinuxSetup
{
   public static void Setup()
    {
        Console.WriteLine("Running on Linux");
        checkInstallGhostScript();
        checkInstallSiegfried();
    }

    /// <summary>
    /// Checks if Siegfried is installed, if not, asks the user if they want to install it
    /// </summary>
    private static void checkInstallSiegfried() 
    {
        //Check if Siegfried is installed
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "/bin/bash";
        startInfo.Arguments = "-c \" " + "sf -version" + " \""; 
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
        if (!output.Contains("Siegfried"))
        {
            Console.WriteLine("Siegfried is not installed. Do you want to install it? (Y/n)");
            string? r = Console.ReadLine();
            if(r == "Y")
            {
                ProcessStartInfo startInfo2 = new ProcessStartInfo();
                startInfo2.FileName = "/bin/bash";
                startInfo2.Arguments = $"-c \"curl -sL 'http://keyserver.ubuntu.com/pks/lookup?op=get&search=0x20F802FE798E6857' | gpg --dearmor | sudo tee /usr/share/keyrings/siegfried-archive-keyring.gpg && echo 'deb [signed-by=/usr/share/keyrings/siegfried-archive-keyring.gpg] https://www.itforarchivists.com/ buster main' | sudo tee -a /etc/apt/sources.list.d/siegfried.list && sudo apt-get update && sudo apt-get install siegfried\"";
                Process process2 = new Process();
                process2.StartInfo = startInfo2;
                process2.Start();
                process2.WaitForExit();
            }
        }
    }

    /// <summary>
    /// Checks if GhostScript is installed, if not, asks the user if they want to install it
    /// </summary>
    private static void checkInstallGhostScript()
    {
        //Check if GhostScript is installed
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "/bin/bash";
        startInfo.Arguments = "-c \" " + "gs -version" + " \"";
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
            Console.WriteLine("GhostScript is not installed.");
            Console.WriteLine("If you are on Ubuntu/Debian run: sudo apt install ghostscript");
            Console.WriteLine("For other Linux distros see https://www.ghostscript.com/ for installation instructions.");
        
        }
    }
}