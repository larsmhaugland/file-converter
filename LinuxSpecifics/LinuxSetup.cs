using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using iText.Commons.Utils;
using iText.Kernel.Pdf;

class LinuxSetup
{
    //Specific Linux distro
    public static string LinuxDistro = GetLinuxDistro();
    public static string PathRunningProgram = "/bin/bash";

    //Map for external converters to check if they are downloaded.
    static Dictionary<List<string>, string> converterArguments = new Dictionary<List<string>, string>()
    {
        {new List<string> { "\"-c \\\" \" + \"gs -version\" + \" \\\"\"", "GPL Ghostscript",  "LinuxSpecifics\\ghostscript.txt"}, "GhostScript"},
        {new List<string>{"\"-c \\\" \" + \"libreoffice --version\" + \" \\\"\"", "LibreOffice", "LinuxSpecifics\\libreoffice.txt"}, "LibreOffice" }
    };
    public static void Setup()
    {
        Console.WriteLine("Running on Linux");
        checkInstallSiegfried();
        foreach (var converter in converterArguments)
        {
            checkInstallConverter(converter.Key[0], converter.Key[1], converter.Key[2]);
        }
    }

    /// <summary>
    /// Runs a process with the given filename and arguments
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="arguments"></param>
    /// <param name="configure"></param>
    private static string RunProcess( Action<ProcessStartInfo> configure) 
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardError = true;
        configure(startInfo);

        try
        {
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            return process.StandardOutput.ReadToEnd();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return "";
        }
    }

    /// <summary>
    /// Checks if Siegfried is installed, if not, asks the user if they want to install it
    /// </summary>
    private static void checkInstallSiegfried() 
    {
        string output = RunProcess(startInfo =>
        { startInfo.FileName = PathRunningProgram;
           startInfo.Arguments = "-c \" " + "sf -version" + " \"";});

        if (!output.Contains("siegfried"))
        {
            Console.WriteLine("Siegfried is not installed. In order to install Siegfried your user must have sudo privileges.");
            Console.WriteLine("For more info on Siegfried see: https://www.itforarchivists.com/siegfried/");
            Console.WriteLine("Prompt for sudo password will appear after accepting the installation process.");
            Console.WriteLine("Do you want to install it? (Y/n)");
            string? r = Console.ReadLine();
            r = r?.ToUpper() ?? " ";
            if (r == "Y")
            {
                InstallSiegfried();
            }
            else 
            {
                Console.WriteLine("Siegfried is not installed. Without Siegfried the program cannot run properly. Exiting program.");
                Environment.Exit(0);
            }
        }
    }

    /// <summary>
    /// Install siegfried based on the linux distro
    /// </summary>
    private static void InstallSiegfried()
    {
        string checkDependencies;

            switch(LinuxDistro)
            {
            case "debian":
                  checkDependencies = RunProcess(startInfo =>
                  {
                      startInfo.FileName = PathRunningProgram;
                      startInfo.Arguments = "-c \" " + "curl" + " \"";
                  });

                if (checkDependencies.Contains(""))
                {
                   RunProcess(startInfo =>
                    {
                        startInfo.FileName = PathRunningProgram;
                        startInfo.Arguments = "-c \" " + "sf -version" + " \"";
                        startInfo.Arguments = $"-c \"curl -sL 'http://keyserver.ubuntu.com/pks/lookup?op=get&search=0x20F802FE798E6857' | gpg --dearmor | sudo tee /usr/share/keyrings/siegfried-archive-keyring.gpg && echo 'deb [signed-by=/usr/share/keyrings/siegfried-archive-keyring.gpg] https://www.itforarchivists.com/ buster main' | sudo tee -a /etc/apt/sources.list.d/siegfried.list && sudo apt-get update && sudo apt-get install siegfried\"";
                });}
                else
                {
                    Console.WriteLine("Siegfried needs curl to install properly. Please install curl and try again.");
                    Environment.Exit(0);
                }
               break;
            case "fedora":
                break;
            case "arch":
                checkDependencies = RunProcess(startInfo =>
                {
                    startInfo.FileName = PathRunningProgram;
                    startInfo.Arguments = "-c \" " + "brew help" + " \"";
                });
                if (checkDependencies.Contains("brew config"))
                {
                    RunProcess(startInfo =>
                    {
                        startInfo.FileName = PathRunningProgram;
                        startInfo.Arguments = $"-c \"brew install richardlehane/digipres/siegfried  \"";
                   });}
                else
                {
                    Console.WriteLine("Siegfried needs homebrew to install properly. Please install homebrew and try again.");
                    Environment.Exit(0);
                }
                break;
            }
    }
    

    /// <summary>
    /// Checks whether the given converter is installed
    /// </summary>
    /// <param name="arguments"> CLI arguments to be run</param>
    /// <param name="expectedOutput"> Expected output from CLI arguments </param>
    /// <param name="consoleMessage"> Message to write if converter is not installed </param>
    private static void checkInstallConverter(string arguments, string expectedOutput, string consoleMessage) {
       string output = RunProcess(startInfo =>
        {
            startInfo.FileName = PathRunningProgram;
            startInfo.Arguments = $"{arguments} | cat {consoleMessage}";
        });
        if (!output.Contains(expectedOutput))
        {
            Console.WriteLine(output);
            //TODO: Remove converter from converters and continue program
        }
    }

   private static string GetLinuxDistro() {
        string distro = "";
        //Check which distro the user is running
        string output = RunProcess(startInfo =>
        {
            startInfo.FileName = PathRunningProgram;
            startInfo.Arguments = "-c \" " + "cat /etc/*-release" + " \"";
        });

        switch (output)
        {
            case var o when o.Contains("Ubuntu") || o.Contains("Debian"):
                Console.WriteLine("Running on Debian based distro");
                distro = "debian";
                break;
            case var o when o.Contains("Fedora"):
                Console.WriteLine("Running on Fedora based distro");
                distro = "fedora";
                break;
            case var o when o.Contains("Arch"):
                Console.WriteLine("Running on Arch based distro");
                distro = "arch";
                break;
            default:
                Console.WriteLine("Distro not supported. Exiting program.");
                Environment.Exit(0);
                break;
        }

        return distro;
    }
}