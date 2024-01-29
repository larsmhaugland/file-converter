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

public class iText7 : Converter
{
    public iText7()
    {
        Name = "iText";
        Version = "8.0.2";
    }

    /**
     * Convert file to pdf
     * 
     * @param fileinfo File to convert
     */
    public override void ConvertFile(FileInfo fileinfo)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileinfo.FileName);
        var pdfVersion = PdfVersion.PDF_2_0;
        string output = Path.Combine(GlobalVariables.parsedOptions.Output, fileNameWithoutExtension + ".pdf");

        using (var pdfWriter = new PdfWriter(output, new WriterProperties().SetPdfVersion(pdfVersion)))
        using (var pdfDocument = new PdfDocument(pdfWriter))
        using (var document = new Document(pdfDocument))
        {
            pdfDocument.SetTagged();
            PdfDocumentInfo info = pdfDocument.GetDocumentInfo();
            iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory.Create(fileinfo.FileName));
            document.Add(image);
        }
        deleteOriginalFileFromOutputDirectory(fileinfo);
    }

    /** TO-DO:
     * Combine multiple files into one pdf
     * 
     */
    public override void CombineFiles()
    {
    }
}
