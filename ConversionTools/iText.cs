using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
public class iTextConverter : Converter
{
    public iTextConverter()
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
        string output = fileinfo.FileName + ".pdf";
        using (var pdfWriter = new PdfWriter(output))
        using (var pdfDocument = new PdfDocument(pdfWriter))
        using (var document = new Document(pdfDocument))
        {
            string fileContent = File.ReadAllText(fileinfo.FilePath);
            document.Add(new Paragraph(fileContent));
        }
        Console.WriteLine("Converted file: " + fileinfo.FileName + " to " + output);
    }

    /** TO-DO:
     * Combine multiple files into one pdf
     * 
     */
    public override void CombineFiles()
    {
    }
}
