using iText.IO.Util;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ghostscript.NET.Rasterizer;
using System.Drawing.Imaging;
using Org.BouncyCastle.Bcpg;
using System.Reflection;
using iText.Kernel.Geom;
using Ghostscript.NET;
using iText.Layout.Splitting;
using System.Runtime.Intrinsics.X86;

//TODO: Check resolution settings when converting to image
//TODO: Error check - only delete original file if conversion is completed successfully
//TODO: Can add PDF to PDF conversion if needed

/// <summary>
/// GhostScript is a subclass of the Converter class.   <br></br>
/// 
/// GhostScript supports the following conversions:     <br></br>
/// - PDF to Image (png, jpg, tif, bmp)                 <br></br>
///                                                     <br></br>
/// Conversions not added:                              <br></br>
/// - PDF to PDF    (see iText7)                        <br></br>
/// - Image to PDF  (see iText7)                        <br></br>
/// </summary>
public class GhostscriptConverter : Converter
{
    public GhostscriptConverter()
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
        string outputFileName = System.IO.Path.Combine(outputDirectory,System.IO.Path.GetFileNameWithoutExtension(fileinfo));
        string extension;
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
                extension = ".png";
                sDevice = "png16m";
                convert(fileinfo, outputFileName, sDevice, extension);
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
                extension = ".jpg";
                sDevice = "jpeg";
                convert(fileinfo, outputFileName, sDevice, extension);
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
                extension = ".tiff";
                sDevice = "tiff24nc";
                convert(fileinfo, outputFileName, sDevice, extension);
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
                extension = ".bmp";
                sDevice = "bmp16m";
                convert(fileinfo, outputFileName, sDevice, extension);
                break;
            #endregion
                //Check how to make the pronom correct
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
                extension = ".pdf";
                sDevice = "pdfwrite";
                convert(fileinfo, outputFileName, sDevice, extension);
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
    /// <param name="outputFileName">The name of the new file</param>
    /// <param name="sDevice">What format GhostScript will convert to</param>
    /// <param name="extension">Extension type for after the conversion</param>
    void convert(string fileinfo, string outputFileName, string sDevice, string extension)
    {
        Logger log = Logger.Instance;
        string gsExecutable = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ghostscriptbinarywindows", "gs10.02.1", "bin", "gsdll64.dll");
        try
        {
            using (var rasterizer = new GhostscriptRasterizer())
            {
                GhostscriptVersionInfo versionInfo = new GhostscriptVersionInfo(new Version(0, 0, 0), gsExecutable, string.Empty, GhostscriptLicense.GPL);
                using (var stream = new FileStream(fileinfo, FileMode.Open, FileAccess.Read))
                {
                    rasterizer.Open(stream, versionInfo, false);

                    ImageFormat imageFormat = GetImageFormat(extension);

                    if (imageFormat != null)
                    {

                        for (int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
                        {
                            string pageOutputFileName = outputFileName + "_" + pageNumber.ToString() + extension;
                            using (var image = rasterizer.GetPage(300, pageNumber))
                            {
                                image.Save(pageOutputFileName, imageFormat);
                            }
                        }
                        // deleteOriginalFileFromOutputDirectory(fileinfo);
                    }
                    else
                    {
                        log.SetUpRunTimeLogMessage("Format not supported by GhostScript. File is not converted.", true, fileinfo);
                    }
                }
            }
        }
        catch (Exception e)
        {
            log.SetUpRunTimeLogMessage("Error when converting file with GhostScript. Error message: " + e.Message, true,filename:fileinfo);
        }
    }

    private ImageFormat GetImageFormat(string extension)
    {
        switch(extension)
        {
            case ".png":
                return ImageFormat.Png;
            case ".jpg":
                return ImageFormat.Jpeg;
            case ".tiff":
                return ImageFormat.Tiff;
            case ".bmp":
                return ImageFormat.Bmp;
            default:
                return null;
        }
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
        //PNG
        "fmt/11",
        "fmt/12",
        "fmt/13",
        "fmt/935",

        //JPG
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
        //TIFF
        "fmt/1917",
        "x-fmt/399",
        "x-fmt/388",
        "x-fmt/387",
        "fmt/155",
        "fmt/353",
        "fmt/154",
        "fmt/153",
        "fmt/156",
        //BMP
        "x-fmt/270",
        "fmt/115",
        "fmt/118",
        "fmt/119",
        "fmt/114",
        "fmt/116",
        "fmt/117",
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

