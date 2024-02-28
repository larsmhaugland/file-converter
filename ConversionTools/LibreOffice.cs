using iText.Kernel.Pdf;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class LibreOfficeConverter : Converter
{
    Logger log = Logger.Instance;
    private static readonly object locker = new object();
    public LibreOfficeConverter()
    {
        Name = "Libreoffice";
        Version = "7.6.4";
        SupportedConversions = getListOfSupportedConvesions();
    }

    /// <summary>
    /// Convert a file to a new format
    /// </summary>
    /// <param name="filePath">The file to be converted</param>
    /// <param name="pronom">The file format to convert to</param>
    public override void ConvertFile(string filePath, string pronom)
    {
        string outputDir = Directory.GetParent(filePath.Replace("input", "output")).ToString();
        string inputDirectory = Directory.GetParent(filePath).ToString();
        string inputFilePath = Path.Combine(inputDirectory, Path.GetFileName(filePath));
        string executableName = "soffice.exe";
        bool sofficePathWindows = checkSofficePathWindows(executableName);
        bool sofficePathLinux = checkSofficePathLinux("soffice");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            lock (locker)
            {
                RunOfficeToPdfConversion(inputFilePath, outputDir, pronom, sofficePathWindows);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            lock (locker)
            {
                RunOfficeToPdfConversion(inputFilePath, outputDir, pronom, sofficePathLinux);
            }
        }
        else
        {
            log.SetUpRunTimeLogMessage("Operating system not supported for office conversion", true, filePath);
        }

    }

    /// <summary>
    /// Reference list stating supported conversions containing key value pairs with string input pronom and string output pronom
    /// </summary>
    /// <returns>List of all supported conversions</returns>
    public override Dictionary<string, List<string>> getListOfSupportedConvesions()
    {
        var supportedConversions = new Dictionary<string, List<string>>();

        // EXcel to all others
        foreach (string excelPronom in EXCELPronoms)
        {
            if (!supportedConversions.ContainsKey(excelPronom))
            {
                supportedConversions[excelPronom] = new List<string>();
            }
            supportedConversions[excelPronom].AddRange(WORDPronoms);
            supportedConversions[excelPronom].AddRange(PowerPointPronoms);
            supportedConversions[excelPronom].AddRange(OpenDocumentPronoms);
            supportedConversions[excelPronom].AddRange(PDFPronoms);
        }
        // Word to all other
        foreach (string wordPronom in WORDPronoms)
        {
            if (!supportedConversions.ContainsKey(wordPronom))
            {
                supportedConversions[wordPronom] = new List<string>();
            }
            supportedConversions[wordPronom].AddRange(PowerPointPronoms);
            supportedConversions[wordPronom].AddRange(OpenDocumentPronoms);
            supportedConversions[wordPronom].AddRange(EXCELPronoms);
            supportedConversions[wordPronom].AddRange(PDFPronoms);
        }
        // PowerPoint to all other
        foreach (string pptPronom in PowerPointPronoms)
        {
            if (!supportedConversions.ContainsKey(pptPronom))
            {
                supportedConversions[pptPronom] = new List<string>();
            }
            supportedConversions[pptPronom].AddRange(EXCELPronoms);
            supportedConversions[pptPronom].AddRange(OpenDocumentPronoms);
            supportedConversions[pptPronom].AddRange(WORDPronoms);
            supportedConversions[pptPronom].AddRange(PDFPronoms);
        }

        // OpenDocument to PDF
        foreach (string odtPronom in OpenDocumentPronoms)
        {
            supportedConversions.Add(odtPronom, PDFPronoms);
        }
        // RTF to PDF
        foreach (string rtfPronom in RTFPronoms)
        {
            supportedConversions.Add(rtfPronom, PDFPronoms);
        }

        return supportedConversions;
    }

    /// <summary>
    /// Converts and office file 
    /// </summary>
    /// <param name="sourceDoc"></param>
    /// <param name="destinationPdf"></param>
    /// <param name="pronom"></param>
    /// <param name="sofficePath"></param>
    void RunOfficeToPdfConversion(string sourceDoc, string destinationPdf, string pronom, bool sofficePath)
    {
        try
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = GetPlatformCommand();
                process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();

                string sofficeCommand = GetSofficeCommand(sofficePath);
                string arguments = GetArguments(destinationPdf, sourceDoc, sofficeCommand);
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string standardOutput = process.StandardOutput.ReadToEnd();
                string standardError = process.StandardError.ReadToEnd();

                process.WaitForExit();
                int exitCode = process.ExitCode;

                if (exitCode != 0)
                {
                    Console.WriteLine($"\n Filepath: {sourceDoc} :  Exit Code: {exitCode}\n");
                    Console.WriteLine("Standard Output:\n" + standardOutput);
                    Console.WriteLine("Standard Error:\n" + standardError);
                }

                string newFileName = Path.Combine(destinationPdf, Path.GetFileNameWithoutExtension(sourceDoc) + ".pdf");
                bool converted = CheckConversionStatus(sourceDoc, newFileName, pronom);
                if (!converted)
                {
                    throw new Exception("File was not converted");
                }
                else
                {
                    deleteOriginalFileFromOutputDirectory(sourceDoc);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Instance.SetUpRunTimeLogMessage("Error converting file to PDF. File is not converted: " + e.Message, true, filename: sourceDoc);
            throw;
        }
    }

    string GetPlatformCommand()
    {
        return Environment.OSVersion.Platform == PlatformID.Unix ? "bash" : "cmd.exe";
    }

    string GetSofficeCommand(bool sofficePath)
    {
        return sofficePath ? "soffice" : Environment.OSVersion.Platform == PlatformID.Unix ? "/usr/lib/libreoffice/program/soffice" : "C:\\Program Files\\LibreOffice\\program\\soffice.exe";
    }
    string GetArguments(string destinationPDF, string sourceDoc, string sofficeCommand)
    {
        return Environment.OSVersion.Platform == PlatformID.Unix ? $@"-c ""{sofficeCommand} --headless --convert-to pdf --outdir '{destinationPDF}' '{sourceDoc}'""" : $@"/C {sofficeCommand} --headless --convert-to pdf --outdir ""{destinationPDF}"" ""{sourceDoc}""";
    }




    /// <summary>
    /// Checks if the folder with the soffice.exe executable exists in the PATH.
    /// </summary>
    /// <param name="executableName">Name of the executable to have its folder in the PATH</param>
    /// <returns>Bool indicating if the directory containing the executable was found </returns>
    static bool checkSofficePathWindows(string executableName)
    {
        
        string pathVariable = Environment.GetEnvironmentVariable("PATH"); // Get the environment variables as a string
        string[] paths = pathVariable.Split(Path.PathSeparator);          // Split them into individual entries

        foreach (string path in paths)                                    // Go through and check if found  
        {
            string fullPath = Path.Combine(path, executableName);
            if (File.Exists(fullPath))
            {
                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// Same function as for windows, but with small changes to facilitate for linux users
    /// </summary>
    /// <param name="executableName">Name of the executable to have its folder in the PATH</param>
    /// <returns></returns>
    static bool checkSofficePathLinux(string executableName)
    {
        string pathVariable = Environment.GetEnvironmentVariable("PATH");
        char pathSeparator = Path.PathSeparator;

        // Use : as the separator on Linux
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            pathSeparator = ':';
        }

        string[] paths = pathVariable.Split(pathSeparator);

        foreach (string path in paths)
        {
            string fullPath = Path.Combine(path, executableName);

            // Linux is case-sensitive, so check for case-insensitive existence
            if (Directory.Exists(fullPath))
            {
                return true;
            }
        }

        return false;
    }

    List<string> PDFPronoms =
    [
        "fmt/14",
        "fmt/15",
        "fmt/16",
        "fmt/17",
        "fmt/18",
        "fmt/19",
        "fmt/20",
        "fmt/276",
        "fmt/1129",
        "fmt/479", // PDFA
    ];
    List<string> WORDPronoms =
    [
        // DOC
        "x-fmt/329",
        "fmt/609",
        "fmt/39",
        "x-fmt/274",
        "x-fmt/275",
        "x-fmt/276",
        "fmt/1688",
        "fmt/37",
        "fmt/38",
        "fmt/1282",
        "fmt/1283",
        "x-fmt/131",
        "x-fmt/42",
        "x-fmt/43",
        "fmt/40",
        "x-fmt/44",
        "x-fmt/393",
        "x-fmt/394",
        "fmt/892",
        // DOCX
        "fmt/473",
        "fmt/1827",
        "fmt/412",
        "fmt/523", // DOCM
        "fmt/597", // DOTX
        "fmt/599", // DOTM
        // DOT
        "x-fmt/45",
        "fmt/755",

    ];
    List<string> EXCELPronoms =
    [
        //XLS
        "fmt/55",
        "fmt/56",
        "fmt/57",
        "fmt/61",
        "fmt/62",
        "fmt/59",
        //XLSX
        "fmt/214",
        "fmt/1828",
        "fmt/445", //XLSM
        "fmt/595", //XLSB
        "fmt/598", //XLTX
        "fmt/627", //XLTM
        "x-fmt/18", //CSV
    ];

    List<string> PowerPointPronoms =
    [
        // PPT
        "fmt/1537",
        "fmt/1866",
        "fmt/181",
        "fmt/1867",
        "fmt/179",
        "fmt/1747",
        "fmt/1748",
        "x-fmt/88",
        "fmt/125",
        "fmt/126",
        // PPTX
        "fmt/215",
        "fmt/1829",
        "fmt/494",
        // PPTM
        "fmt/487",
        // PPS
        "x-fmt/87",
        // PPSM
        "fmt/630",
        // PPSX
        "fmt/629",
        // POT
        "x-fmt/84",
        //POTX
        "fmt/631",
        // POTM
        "fmt/632",
    ];

    List<string> OpenDocumentPronoms =
    [
        // ODT
        "x-fmt/3",
        "fmt/1756",
        "fmt/136",
        "fmt/290",
        "fmt/291",
        // ODP
        "fmt/1754",
        "fmt/138",
        "fmt/292",
        "fmt/293",
        // ODS
        "fmt/1755",
        "fmt/137",
        "fmt/294",
        "fmt/295",
    ];
    List<string> RTFPronoms =
    [
        "fmt/969",
        "fmt/45",
        "fmt/50",
        "fmt/52",
        "fmt/53",
        "fmt/355",
    ];
}



/*

 /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceDoc"></param>
    /// <param name="destinationPdf"></param>
    /// <param name="pronom"></param>
    /// <param name="sofficePath"></param>
    void RunOfficeToPdfConversionWindows(string sourceDoc, string destinationPdf, string pronom, bool sofficePath)
    {
        try
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                string sofficeCommand = sofficePath ? "soffice" : "C:\\Program Files\\LibreOffice\\program\\soffice";
                //string sofficeCommand = "soffice";
                process.StartInfo.Arguments = $@"/C {sofficeCommand} --headless --convert-to pdf --outdir ""{destinationPdf}"" ""{sourceDoc}""";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                

                process.Start();

                string standardOutput = process.StandardOutput.ReadToEnd();
                string standardError = process.StandardError.ReadToEnd();

                process.WaitForExit();
                int exitCode = process.ExitCode;
                if (exitCode != 0)
                {
                    Console.WriteLine($"\n Filepath: {sourceDoc} :  Exit Code: {exitCode}\n");
                    Console.WriteLine("Standard Output:\n" + standardOutput);
                    Console.WriteLine("Standard Error:\n" + standardError);
                }
                
                string newFileName = Path.Combine(destinationPdf, Path.GetFileNameWithoutExtension(sourceDoc) + ".pdf");
                bool converted = CheckConversionStatus(sourceDoc, newFileName, pronom);
                if (!converted)
                {
                    throw new Exception("File was not converted");
                }
                else
                {
                    deleteOriginalFileFromOutputDirectory(sourceDoc);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Instance.SetUpRunTimeLogMessage("Error converting file to PDF. File is not converted: " + e.Message, true, filename: sourceDoc);
            throw;
        }
    }

    void RunOfficeToPdfConversionLinux(string sourceDoc, string destinationPdf, string pronom, bool sofficePath)
    {
        try
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "bash";
                process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();

                // Use the full path to soffice if sofficePath is false
                string sofficeCommand = sofficePath ? "soffice" : "/usr/lib/libreoffice/program/soffice";

                process.StartInfo.Arguments = $@"-c ""{sofficeCommand} --headless --convert-to pdf --outdir '{destinationPdf}' '{sourceDoc}'""";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string standardOutput = process.StandardOutput.ReadToEnd();
                string standardError = process.StandardError.ReadToEnd();

                process.WaitForExit();
                int exitCode = process.ExitCode;

                if (exitCode != 0)
                {
                    Console.WriteLine($"\n Filepath: {sourceDoc} :  Exit Code: {exitCode}\n");
                    Console.WriteLine("Standard Output:\n" + standardOutput);
                    Console.WriteLine("Standard Error:\n" + standardError);
                }

                string newFileName = Path.Combine(destinationPdf, Path.GetFileNameWithoutExtension(sourceDoc) + ".pdf");
                bool converted = CheckConversionStatus(sourceDoc, newFileName, pronom);
                if (!converted)
                {
                    throw new Exception("File was not converted");
                }
                else
                {
                    deleteOriginalFileFromOutputDirectory(sourceDoc);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Instance.SetUpRunTimeLogMessage("Error converting file to PDF. File is not converted: " + e.Message, true, filename: sourceDoc);
            throw;
        }
    }

*/