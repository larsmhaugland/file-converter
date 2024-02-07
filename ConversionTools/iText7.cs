using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using System.Reflection;
using iText.Kernel.Font;
using iText.Pdfa;
using iText.Html2pdf;
using iText.Commons;
using System.Runtime.CompilerServices;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Tagutils;
using iText.Html2pdf.Attach.Impl.Layout;

/// <summary>
/// iText7 is a subclass of the Converter class.                                                     <br></br>
///                                                                                                  <br></br>
/// iText7 supports the following conversions:                                                       <br></br>
/// - Image (jpg, png, gif, tiff, bmp) to PDF 1.0-2.0                                                <br></br>
/// - Image (jpg, png, gif, tiff, bmp) to PDF-A 1A-3B                                                <br></br>
///                                                                                                  <br></br>
/// iText7 can also combine the following file formats into one PDF (1.0-2.0) or PDF-A (1A-3B):      <br></br>
/// - Image (jpg, png, gif, tiff, bmp)                                                               <br></br>
///                                                                                                  <br></br>
/// Conversions not added:                                                                           <br></br>
/// - HTML to PDF                                                                                    <br></br>
/// </summary>
public class iText7 : Converter
{
    public iText7()
    {
        Name = "iText7";
        Version = "8.0.2";
        SupportedConversions = listOfSupportedConversions();
    }

    /// <summary>
    /// Convert a file to a new format
    /// </summary>
    /// <param name="fileinfo">The file to be converted</param>
    /// <param name="pronom">The file format to convert to</param>
    public override void ConvertFile(string fileinfo, string pronom)
    {
        Logger log = Logger.Instance;
        string extension = Path.GetExtension(fileinfo).ToLower();

        if (extension == ".html" || extension == ".htm")
        {
            switch (pronom)
            {
                //PDF-A
                case "fmt/95":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_2, true, PdfAConformanceLevel.PDF_A_1A);
                    break;
                case "fmt/354":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_2, true, PdfAConformanceLevel.PDF_A_1B);
                    break;
                case "fmt/476":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_2, true, PdfAConformanceLevel.PDF_A_2A);
                    break;
                case "fmt/477":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_2, true, PdfAConformanceLevel.PDF_A_2B);
                    break;
                case "fmt/478":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_2, true, PdfAConformanceLevel.PDF_A_2U);
                    break;
                case "fmt/479":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_2, true, PdfAConformanceLevel.PDF_A_3A);
                    break;
                case "fmt/480":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_2, true, PdfAConformanceLevel.PDF_A_3B);
                    break;
                //PDF 1.x
                case "fmt/14":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_0, false);
                    break;
                case "fmt/15":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_1, false);
                    break;
                case "fmt/16":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_2, false);
                    break;
                case "fmt/17":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_3, false);
                    break;
                case "fmt/18":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_4, false);
                    break;
                case "fmt/19":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_5, false);
                    break;
                case "fmt/20":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_6, false);
                    break;
                case "fmt/276":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_1_7, false);
                    break;
                //PDF 2.x
                case "fmt/1129":
                    convertFromHTMLToPDF(fileinfo, PdfVersion.PDF_2_0, false);
                    break;
                //Logger error-message
                default:
                    log.SetUpRunTimeLogMessage(pronom + " is not supported by iText7. File is not converted.", true, filename: fileinfo);
                    break;
            }

        }
        else if(extension == ".pdf")
        {
            switch (pronom)
            {
                //PDF-A
                case "fmt/95":
                    convertFromPDFToPDFA(fileinfo, PdfAConformanceLevel.PDF_A_1A);
                    break;
                case "fmt/354":
                    convertFromPDFToPDFA(fileinfo, PdfAConformanceLevel.PDF_A_1B);
                    break;
                case "fmt/476":
                    convertFromPDFToPDFA(fileinfo, PdfAConformanceLevel.PDF_A_2A);
                    break;
                case "fmt/477":
                    convertFromPDFToPDFA(fileinfo, PdfAConformanceLevel.PDF_A_2B);
                    break;
                case "fmt/478":
                    convertFromPDFToPDFA(fileinfo, PdfAConformanceLevel.PDF_A_2U);
                    break;
                case "fmt/479":
                    convertFromPDFToPDFA(fileinfo, PdfAConformanceLevel.PDF_A_3A);
                    break;
                case "fmt/480":
                    convertFromPDFToPDFA(fileinfo, PdfAConformanceLevel.PDF_A_3B);
                    break;
                default:
                log.SetUpRunTimeLogMessage(pronom + " is not supported by iText7. File is not converted.", true, filename: fileinfo);
                break;
            }
        }
        else
        {
            switch (pronom)
            {
                //PDF-A
                case "fmt/95":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_2_0, PdfAConformanceLevel.PDF_A_1A);
                    break;
                case "fmt/354":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_2_0, PdfAConformanceLevel.PDF_A_1B);
                    break;
                case "fmt/476":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_2_0, PdfAConformanceLevel.PDF_A_2A);
                    break;
                case "fmt/477":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_2_0, PdfAConformanceLevel.PDF_A_2B);
                    break;
                case "fmt/478":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_2_0, PdfAConformanceLevel.PDF_A_2U);
                    break;
                case "fmt/479":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_2_0, PdfAConformanceLevel.PDF_A_3A);
                    break;
                case "fmt/480":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_2_0, PdfAConformanceLevel.PDF_A_3B);
                    break;
                //PDF 1.x
                case "fmt/14":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_1_0);
                    break;
                case "fmt/15":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_1_1);
                    break;
                case "fmt/16":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_1_2);
                    break;
                case "fmt/17":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_1_3);
                    break;
                case "fmt/18":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_1_4);
                    break;
                case "fmt/19":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_1_5);
                    break;
                case "fmt/20":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_1_6);
                    break;
                case "fmt/276":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_1_7);
                    break;
                //PDF 2.x
                case "fmt/1129":
                    convertFromImageToPDF(fileinfo, PdfVersion.PDF_2_0);
                    break;
                //Logger error-message
                default:
                    log.SetUpRunTimeLogMessage(pronom + " is not supported by iText7. File is not converted.", true, filename: fileinfo);
                    break;
            }
        }
    }

    /// <summary>
    /// Convert from any image file to pdf version 1.0-2.0
    /// </summary>
    /// <param name="fileinfo">The file being converted</param>
    /// <param name="pdfVersion">What pdf version it is being converted to</param>
    void convertFromImageToPDF(string fileinfo, PdfVersion pdfVersion, PdfAConformanceLevel? conformanceLevel = null) {

        string dir = Path.GetDirectoryName(fileinfo)?.ToString() ?? "";
        string filePathWithoutExtension = Path.Combine(dir, Path.GetFileNameWithoutExtension(fileinfo));
        string output = Path.Combine(filePathWithoutExtension + ".pdf");
        try
        {
            using (var pdfWriter = new PdfWriter(output, new WriterProperties().SetPdfVersion(pdfVersion)))
            using (var pdfDocument = new PdfDocument(pdfWriter))
            using (var document = new Document(pdfDocument))
            {
                pdfDocument.SetTagged();
                PdfDocumentInfo info = pdfDocument.GetDocumentInfo();
                iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory.Create(fileinfo));
                document.Add(image);
            }
            //TODO: Check if file is converted correctly, only delete file if yes
            replaceFileInList(fileinfo, output);
            if(conformanceLevel != null)
            {
                convertFromPDFToPDFA(output, conformanceLevel, fileinfo);
            }
            else
            {
                deleteOriginalFileFromOutputDirectory(fileinfo);
            }
        }
        catch (Exception e)
        {
            Logger.Instance.SetUpRunTimeLogMessage("Error converting file to PDF. File is not converted: " + e.Message, true, filename: fileinfo);
        }
    }

    /// <summary>
    /// Convert from any html file to pdf 1.0-2.0
    /// </summary>
    /// <param name="fileinfo">Name of the file to be converted</param>
    /// <param name="pdfVersion">Specific pdf version to be converted to</param>
    void convertFromHTMLToPDF(string fileinfo, PdfVersion pdfVersion, bool pdfA, PdfAConformanceLevel? conformanceLevel = null)
    {
        string dir = Path.GetDirectoryName(fileinfo)?.ToString() ?? "";
        string filePathWithoutExtension = Path.Combine(dir, Path.GetFileNameWithoutExtension(fileinfo));
        string output = Path.Combine(filePathWithoutExtension + ".pdf");

        try
        {
            using (var pdfWriter = new PdfWriter(output, new WriterProperties().SetPdfVersion(pdfVersion)))
            using (var pdfDocument = new PdfDocument(pdfWriter))
            using (var document = new Document(pdfDocument))
            {
                pdfDocument.SetTagged();
                PdfDocumentInfo info = pdfDocument.GetDocumentInfo();
                using(var htmlSource = new FileStream(fileinfo, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    HtmlConverter.ConvertToPdf(htmlSource, pdfDocument);
                    document.Close();
            
                }
                pdfDocument.Close();
                pdfWriter.Close();
                pdfWriter.Dispose();
            }
            
            if (conformanceLevel != null)
            {
                convertFromPDFToPDFA(output, conformanceLevel, fileinfo);
            }
            else
            {
                deleteOriginalFileFromOutputDirectory(fileinfo);
            }
        }
        catch (Exception e)
        {
            Logger.Instance.SetUpRunTimeLogMessage("Error converting file to PDF. File is not converted: " + e.Message, true, filename: fileinfo);
        }

    }

    /// <summary>
    /// Convert from any pdf file to pdf-A version 1A-3B
    /// </summary>
    /// <param name="fileinfo">The filename to convert</param>
    /// <param name="conformanceLevel">The type of PDF-A to convert to</param>
    /// <param name="originalFile">Original file that should be deleted</param>
    void convertFromPDFToPDFA(string fileinfo, PdfAConformanceLevel conformanceLevel, string? originalFile = null)
    {
        try
        {
            string newFileName = Path.Combine(Path.GetDirectoryName(fileinfo) ?? "", Path.GetFileNameWithoutExtension(fileinfo) + "_PDFA.pdf");
            using (FileStream iccFilestream = new FileStream("ConversionTools/sRGB2014.icc", FileMode.Open))
            {
                PdfOutputIntent outputIntent = new PdfOutputIntent("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", iccFilestream);

                using (PdfReader reader = new PdfReader(fileinfo))
                using (PdfWriter writer = new PdfWriter(newFileName))
                {

                    PdfADocument pdfADocument = new PdfADocument(writer, conformanceLevel, outputIntent);
                    PdfDocument pdfDocument = new PdfDocument(reader);

                    for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
                    {

                        PdfPage page = pdfADocument.AddNewPage();
                        PdfFormXObject pageCopy = pdfDocument.GetPage(pageNum).CopyAsFormXObject(pdfADocument);
                        PdfCanvas canvas = new PdfCanvas(page);
                        canvas.AddXObject(pageCopy);
                    }


                    pdfDocument.Close();
                    pdfADocument.Close();
                }
            }
            //deleteOriginalFileFromOutputDirectory(fileinfo);
            File.Delete(fileinfo);
            File.Move(newFileName, fileinfo);
            if (originalFile != null)
            {
                deleteOriginalFileFromOutputDirectory(originalFile);
            }
        }
        catch(Exception e)
        {
            Logger.Instance.SetUpRunTimeLogMessage("Error converting file to PDF-A. File is not converted: " + e.Message, true, filename: fileinfo);
        }
    }


    /// <summary>
    /// Update the fileinfo object with new information after conversion
    /// </summary>
    /// <param name="fileinfo">The file that gets updated information</param>
    /// 	/// <param name="pronom">The file format to convert to</param>
    public override void CombineFiles(string[] files, string pronom)
    {
        if (files == null || files.Length == 0)
        {
            Logger.Instance.SetUpRunTimeLogMessage("Files sent to iText7 to be combined, but no files found.", true);
            return;
        }

        Logger log = Logger.Instance;

        string outputFolder = GlobalVariables.parsedOptions.Output;
        //TODO: Check with archive how they want to name the combined pdfs
        string outputFileName = Path.GetFileNameWithoutExtension(files[0]) + ".pdf";

        switch (pronom)
        {
            //PDF-A
            case "fmt/95":
                MergeFilesToPDFA(files, outputFileName, outputFolder, PdfAConformanceLevel.PDF_A_1A);
                break;
            case "fmt/354":
                MergeFilesToPDFA(files, outputFileName, outputFolder, PdfAConformanceLevel.PDF_A_1B);
                break;
            case "fmt/476":
                MergeFilesToPDFA(files, outputFileName, outputFolder, PdfAConformanceLevel.PDF_A_2A);
                break;
            case "fmt/477":
                MergeFilesToPDFA(files, outputFileName, outputFolder, PdfAConformanceLevel.PDF_A_2B);
                break;
            case "fmt/478":
                MergeFilesToPDFA(files, outputFileName, outputFolder, PdfAConformanceLevel.PDF_A_2U);
                break;
            case "fmt/479":
                MergeFilesToPDFA(files, outputFileName, outputFolder, PdfAConformanceLevel.PDF_A_3A);
                break;
            case "fmt/480":
                MergeFilesToPDFA(files, outputFileName, outputFolder, PdfAConformanceLevel.PDF_A_3B);
                break;
            //PDF 1.x
            case "fmt/14":
                MergeFilesToPDF(files, outputFileName, outputFolder, PdfVersion.PDF_1_0);
                break;
            case "fmt/15":
                MergeFilesToPDF(files, outputFileName, outputFolder, PdfVersion.PDF_1_1);
                break;
            case "fmt/16":
                MergeFilesToPDF(files, outputFileName, outputFolder, PdfVersion.PDF_1_2);
                break;
            case "fmt/17":
                MergeFilesToPDF(files, outputFileName, outputFolder, PdfVersion.PDF_1_3);
                break;
            case "fmt/18":
                MergeFilesToPDF(files, outputFileName, outputFolder, PdfVersion.PDF_1_4);
                break;
            case "fmt/19":
                MergeFilesToPDF(files, outputFileName, outputFolder, PdfVersion.PDF_1_5);
                break;
            case "fmt/20":
                MergeFilesToPDF(files, outputFileName, outputFolder, PdfVersion.PDF_1_6);
                break;
            case "fmt/276":
                MergeFilesToPDF(files, outputFileName, outputFolder, PdfVersion.PDF_1_7);
                break;
            //PDF 2.x
            case "fmt/1129":
                MergeFilesToPDF(files, outputFileName, outputFolder, PdfVersion.PDF_2_0);
                break;
            //Logger error-message
            default:
                //TODO: Check how the archive wants to write the error message, should all files be displayed?
                log.SetUpRunTimeLogMessage(pronom + " is not supported by iText7. Files have not been combined.", true, files[0]);
                break;
        
        }
    }

    /// <summary>
    /// Merge several image files into one pdf
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputFileName"></param>
    /// <param name="outputFolder"></param>
    /// <param name="pdfVersion"></param>
    void MergeFilesToPDF(string[] files, string outputFileName, string outputFolder, PdfVersion pdfVersion)
    {
        string output = Path.Combine(outputFolder, outputFileName);

        using (var pdfWriter = new PdfWriter(output, new WriterProperties().SetPdfVersion(pdfVersion)))
        using (var pdfDocument = new PdfDocument(pdfWriter))
        using (var document = new Document(pdfDocument))
        {
            pdfDocument.SetTagged();
            PdfDocumentInfo info = pdfDocument.GetDocumentInfo();
            foreach (string file in files)
            {
                iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory.Create(file));
                document.Add(image);
            }
        }
        foreach (string file in files) { 
            deleteOriginalFileFromOutputDirectory(file);
        }
    }

    /// <summary>
    /// Merge several image files into one pdf-a
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputFileName"></param>
    /// <param name="outputFolder"></param>
    /// <param name="conformanceLevel"></param>
    void MergeFilesToPDFA(string[] files, string outputFileName, string outputFolder, PdfAConformanceLevel conformanceLevel)
    {
        string output = Path.Combine(outputFolder, outputFileName);

        PdfOutputIntent? outputIntent = null;
        using (var pdfWriter = new PdfWriter(output, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0)))
        using (var pdfDocument = new PdfADocument(pdfWriter, conformanceLevel, outputIntent))
        using (var document = new Document(pdfDocument))
        {
            pdfDocument.SetTagged();
            PdfDocumentInfo info = pdfDocument.GetDocumentInfo();
            foreach (string file in files)
            {
                iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory.Create(file));
                document.Add(image);
            }
        }
        foreach (string file in files)
        {
            deleteOriginalFileFromOutputDirectory(file);
        }
    }

    /// <summary>
    /// Reference list stating supported conversions containing key value pairs with string input pronom and string output pronom
    /// </summary>
    /// <returns>List of all conversions</returns>
    public override Dictionary<string, List<string>> listOfSupportedConversions()
    {
        var supportedConversions = new Dictionary<string, List<string>>();
        foreach (string imagePronom in ImagePronoms)
        {
            supportedConversions.Add(imagePronom, PDFPronoms);
        }
        foreach(string htmlPronom in HTMLPronoms)
        {
            supportedConversions.Add(htmlPronom, PDFPronoms);
        }
        foreach(string pdfPronom in PDFPronoms)
        {
            supportedConversions.Add(pdfPronom, PDFPronoms);
        }

        return supportedConversions;
    }


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