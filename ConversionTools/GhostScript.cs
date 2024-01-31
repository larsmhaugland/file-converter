using iText.IO.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO: Check resolution settings when converting to image
//TODO: Error check - only delete original file if conversion is completed successfully

/// <summary>
/// GhostScript is a subclass of the Converter class.   <br></br>
/// 
/// GhostScript supports the following conversions:     <br></br>
/// - Image (png, jpg, tif, bmp) to PDF                 <br></br>
/// - PDF to Image (png, jpg, tif, bmp)                 <br></br>
/// - HTML to PDF                                       <br></br>
/// </summary>
public class Ghostscript : Converter
{
    public Ghostscript()
    {
        Name = "Ghostscript";
        Version = "1.23.1";
    }

    /// <summary>
    /// Convert a file to a new format
    /// </summary>
    /// <param name="fileinfo">The file to be converted</param>
    /// <param name="pronom">The file format to convert to</param>
    public override void ConvertFile(string fileinfo, string pronom)
    {
        string outputDirectory = GlobalVariables.parsedOptions.Output;
        string outputFileName;
        string sDevice;

        Logger log = Logger.Instance;

        switch (pronom)
        {
            #region png
            // PNG
            case "fmt/11":
            case "fmt/12":
            case "fmt/13":
            case "fmt/935":
                outputFileName = Path.GetFileNameWithoutExtension(fileinfo) + ".png";
                sDevice = "png16m";
                convert(fileinfo, outputDirectory, outputFileName, sDevice);
                break;
            #endregion
            #region jpg
            // JPG/JPEG
            case "fmt/41":
            case "fmt/42":
            case "fmt/43":
            case "fmt/44":
            case "x-fmt/398":
            case "x-fmt/390":
            case "x-fmt/391":
            case "fmt/645":
            case "fmt/1507":
            case "fmt/112":
            case "fmt/367":
                outputFileName = Path.GetFileNameWithoutExtension(fileinfo) + ".jpg";
                sDevice = "jpeg";
                convert(fileinfo, outputDirectory, outputFileName, sDevice);
                 break;
            #endregion
            #region tif
            // TIF
            case "fmt/1917":
            case "x-fmt/399":
            case "x-fmt/388":
            case "x-fmt/387":
            case "fmt/155":
            case "fmt/353":
            case "fmt/154":
            case "fmt/153":
            case "fmt/156":
                outputFileName = Path.GetFileNameWithoutExtension(fileinfo) + ".tif";
                sDevice = "tiff24nc";
                convert(fileinfo, outputDirectory, outputFileName, sDevice);
                break;
            #endregion
            #region bmp
            // BMP
            case "x-fmt/270":
            case "fmt/115":
            case "fmt/118":
            case "fmt/119":
            case "fmt/114":
            case "fmt/116":
            case "fmt/117":
                outputFileName = Path.GetFileNameWithoutExtension(fileinfo) + ".bmp";
                sDevice = "bmp16m";
                convert(fileinfo, outputDirectory, outputFileName, sDevice);
                break;
            #endregion
            #region pdf
            case "fmt/559":
            case "fmt/560":
            case "fmt/561":
            case "fmt/562":
            case "fmt/563":
            case "fmt/564":
            case "fmt/565":
            case "fmt/558":
            case "fmt/14":
            case "fmt/15":
            case "fmt/16":
            case "fmt/17":
            case "fmt/18":
            case "fmt/19":
            case "fmt/20":
            case "fmt/276":
            case "fmt/95":
            case "fmt/354":
            case "fmt/476":
            case "fmt/477":
            case "fmt/478":
            case "fmt/479":
            case "fmt/480":
            case "fmt/481":
            case "fmt/1910":
            case "fmt/1911":
            case "fmt/1912":
            case "fmt/493":
            case "fmt/144":
            case "fmt/145":
            case "fmt/157":
            case "fmt/146":
            case "fmt/147":
            case "fmt/158":
            case "fmt/148":
            case "fmt/488":
            case "fmt/489":
            case "fmt/490":
            case "fmt/492":
            case "fmt/491":
            case "fmt/1129":
            case "fmt/1451":
                outputFileName = Path.GetFileNameWithoutExtension(fileinfo) + ".pdf";
                sDevice = "pdfwrite";
                convert(fileinfo, outputDirectory, outputFileName, sDevice);
                break;
            #endregion
            default:
                log.SetUpRunTimeLogMessage(pronom + " is not supported by GhostScript. File is not converted.", true, fileinfo);
                break;
        }
    }

    /// <summary>
    /// Convert a file using GhostScript command line
    /// </summary>
    /// <param name="fileinfo">The file to be converted</param>
    /// <param name="output">The specified output directory</param>
    /// <param name="outputFileName">The name of the new file</param>
    /// <param name="sDevice">What format GhostScript will convert to</param>
    void convert(string fileinfo, string output, string outputFileName, string sDevice)
    {
        string input = fileinfo;
        string outputFilePath = Path.Combine(output, outputFileName);
        Logger log = Logger.Instance;

        string gsArguments = "-dNOPAUSE -dBATCH -sDEVICE=" + sDevice + " -sOutputFile=" + outputFilePath + " " + input;
        Process gsProcess = new Process();
        gsProcess.StartInfo.FileName = "gswin64c.exe";
        gsProcess.StartInfo.Arguments = gsArguments;
        gsProcess.StartInfo.UseShellExecute = false;
        gsProcess.StartInfo.RedirectStandardOutput = true;
        gsProcess.StartInfo.RedirectStandardError = true;
        gsProcess.Start();

        //TODO: Check if standard output is necessary (either w/ archive or by test running the program)
        log.SetUpRunTimeLogMessage(gsProcess.StandardOutput.ReadToEnd(), false, fileinfo);
        log.SetUpRunTimeLogMessage(gsProcess.StandardError.ReadToEnd(), true, fileinfo);

        gsProcess.WaitForExit();
        gsProcess.Close();

        deleteOriginalFileFromOutputDirectory(fileinfo);
    }

    /// <summary>
    /// Reference list stating supported conversions containing key value pairs with string input pronom and string output pronom
    /// </summary>
    /// <returns>List of all conversions</returns>
    public override Dictionary<string, List<string>> listOfSupportedConversions()
    {
        var supportedConversions = new Dictionary<string, List<string>>();
        //Image to PDF
        foreach (string imagePronom in ImagePronoms)
        {
            supportedConversions.Add(imagePronom, PDFPronoms);
        }
        //PDF to Image
        foreach(string pdfPronom in PDFPronoms)
        {
            supportedConversions.Add(pdfPronom, ImagePronoms);
        }
        //HTML to PDF
        foreach (string htmlPronom in HTMLPronoms)
        {
            supportedConversions.Add(htmlPronom, PDFPronoms);
        }

        return supportedConversions;
    }

    //TODO: Clean up PRONOM list, not all of these are supported by GhostScript
    List<string> ImagePronoms = [
    "fmt/3",
        "fmt/4",
        "fmt/11",
        "fmt/12",
        "fmt/13",
        "fmt/935",
        "fmt/41",
        "fmt/42",
        "fmt/43",
        "fmt/44",
        "x-fmt/398",
        "x-fmt/390",
        "x-fmt/391",
        "fmt/645",
        "fmt/1507",
        "fmt/112",
        "fmt/367",
        "fmt/1917",
        "x-fmt/399",
        "x-fmt/388",
        "x-fmt/387",
        "fmt/155",
        "fmt/353",
        "fmt/154",
        "fmt/153",
        "fmt/156",
        "x-fmt/270",
        "fmt/115",
        "fmt/118",
        "fmt/119",
        "fmt/114",
        "fmt/116",
        "fmt/117"
];
    List<string> HTMLPronoms = [
        "fmt/103",
        "fmt/96",
        "fmt/97",
        "fmt/98",
        "fmt/99",
        "fmt/100",
        "fmt/471",
        "fmt/1132",
        "fmt/102",
        "fmt/583"
    ];
    List<string> PDFPronoms = [
        "fmt/95",
        "fmt/354",
        "fmt/476",
        "fmt/477",
        "fmt/478",
        "fmt/479",
        "fmt/480",
        "fmt/14",
        "fmt/15",
        "fmt/16",
        "fmt/17",
        "fmt/18",
        "fmt/19",
        "fmt/20",
        "fmt/276",
        "fmt/1129"
    ];
}

