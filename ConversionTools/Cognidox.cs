using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class CogniddoxConverter : Converter
{
    Logger log = Logger.Instance;
    public CogniddoxConverter()
    {
        Name = "";
        Version = "";
        SupportedConversions = listOfSupportedConversions();
    }

    /// <summary>
    /// Convert a file to a new format
    /// </summary>
    /// <param name="filePath">The file to be converted</param>
    /// <param name="pronom">The file format to convert to</param>
    public override void ConvertFile(string filePath, string pronom)
    {
        string covnersionExePath = "ConversionTools/OfficeToPDF.exe";
        string parentDirectory = Directory.GetParent(filePath).ToString();
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string targetFileExtension = "";
        // Logic here for getting the correct file extenstion based on the pronom (fmt format) sent as parameter
        string filePathWithNewExtension = Path.Combine(parentDirectory, fileName + targetFileExtension);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            RunOfficeToPdfConversionWindows(covnersionExePath, filePath, filePathWithNewExtension);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            RunOfficeConversionLinuxMacOS(filePath, filePathWithNewExtension);
        }
        else
        {
            log.SetUpRunTimeLogMessage("Operating system not supported for office conversion", true, filePath);
        }

    }

    /// <summary>
    /// Reference list stating supported conversions containing key value pairs with string input pronom and string output pronom
    /// </summary>
    /// <returns>List of all conversions</returns>
    public override Dictionary<string, List<string>> listOfSupportedConversions()
    {
        var supportedConversions = new Dictionary<string, List<string>>();

        // WORD to PDF
        foreach (string wordPronom in WORDPronoms)
        {
            supportedConversions.Add(wordPronom, PDFPronoms);
        }
        // EXCEL to PDF
        foreach (string excelPronom in EXCELPronoms)
        {
            supportedConversions.Add(excelPronom, PDFPronoms);
        }
        // PPT to PDF
        foreach (string pptPronom in PowerPointPronoms)
        {
            supportedConversions.Add(pptPronom, PDFPronoms);
        }
        // OpenDocument to PDF
        foreach (string odtPronom in OpenDocumentPronoms)
        {
            supportedConversions.Add(odtPronom, PDFPronoms);
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            foreach (string excelPronom in EXCELPronoms)
            {
                if (!supportedConversions.ContainsKey(excelPronom))
                {
                    supportedConversions[excelPronom] = new List<string>();
                }
                supportedConversions[excelPronom].AddRange(WORDPronoms);
                supportedConversions[excelPronom].AddRange(PowerPointPronoms);
                supportedConversions[excelPronom].AddRange(OpenDocumentPronoms);
            }

            foreach (string wordPronom in WORDPronoms)
            {
                if (!supportedConversions.ContainsKey(wordPronom))
                {
                    supportedConversions[wordPronom] = new List<string>();
                }
                supportedConversions[wordPronom].AddRange(PowerPointPronoms);
                supportedConversions[wordPronom].AddRange(OpenDocumentPronoms);
                supportedConversions[wordPronom].AddRange(EXCELPronoms);
            }

            foreach (string pptPronom in PowerPointPronoms)
            {
                if (!supportedConversions.ContainsKey(pptPronom))
                {
                    supportedConversions[pptPronom] = new List<string>();
                }
                supportedConversions[pptPronom].AddRange(EXCELPronoms);
                supportedConversions[pptPronom].AddRange(OpenDocumentPronoms);
                supportedConversions[pptPronom].AddRange(WORDPronoms);
            }
        }

        return supportedConversions;
    }

    static void RunOfficeToPdfConversionWindows(string exePath, string sourceDoc, string destinationPdf)
    {
        Process process = new Process();
        process.StartInfo.FileName = exePath;
        process.StartInfo.Arguments = $"{sourceDoc} {destinationPdf}";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        process.WaitForExit();

        // Capture standard output and standard error
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        // Print standard output and standard error to the console or log it
        Console.WriteLine("Conversion Output:");
        Console.WriteLine(output);

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("Conversion Error:");
            Console.WriteLine(error);
        }

        process.Close();
    }

    static void RunOfficeConversionLinuxMacOS(string filePath, string outputdir)
    {
        // Build the soffice command
        string sofficeCommand = $"soffice --convert-to pdf {filePath}";

        // Start the process
        Process process = new Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"{sofficeCommand}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        process.WaitForExit();

        // Capture standard output and standard error
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        // Print standard output and standard error to the console or log it
        Console.WriteLine("Conversion Output:");
        Console.WriteLine(output);

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("Conversion Error:");
            Console.WriteLine(error);
        }

        process.Close();
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
