using System.Diagnostics;
using Ghostscript.NET.Rasterizer;
using System.Drawing.Imaging;
using Ghostscript.NET;
using System.Drawing;

//TODO: Put all images in a folder with original name and delete original file

/// <summary>
/// GhostScript is a subclass of the Converter class.   <br></br>
/// 
/// GhostScript supports the following conversions:     <br></br>
/// - PDF to Image (png, jpg, tif, bmp)                 <br></br>
/// - PostScript to PDF                                 <br></br>
///                                                     <br></br>
/// Conversions not added:                              <br></br>
/// - Image to PDF  (see iText7)                        <br></br>
/// </summary>
public class GhostscriptConverter : Converter
{
	public GhostscriptConverter()
	{
		Name = "Ghostscript";
		Version = "1.23.1";
		SupportedConversions = getListOfSupportedConvesions();
        SupportedOperatingSystems = getSupportedOS();
      
	}

    // Ghostscript executable path (if on Windows)
    public string gsExecutable = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GhostscriptBinaryFiles", "gs10.02.1", "bin", "gswin64c.exe");

    /// <summary>
    /// List of pronoms for images
    /// </summary>
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

    /// <summary>
    /// List of pronoms for PDF
    /// </summary>
    List<string> PDFPronoms = [
        "fmt/15",
        "fmt/16",
        "fmt/17",
        "fmt/18",
        "fmt/19",
        "fmt/20",
        "fmt/276",
        "fmt/1129"
    ];

    /// <summary>
    /// List of pronoms for PostScript
    /// </summary>
    List<string> PostScriptPronoms = [
        "fmt/124",
        "x-fmt/91",
        "x-fmt/406",
        "x-fmt/407",
        "x-fmt/408",
        "fmt/501"
        ];

    /// <summary>
    /// Maps the pronom to the pdf version
    /// </summary>
    Dictionary<string, double> pdfVersionMap = new Dictionary<string, double>()
    {
        {"fmt/15", 1.1},
        {"fmt/16", 1.2},
        {"fmt/17", 1.3},
        {"fmt/18", 1.4},
        {"fmt/19", 1.5},
        {"fmt/20", 1.6},
        {"fmt/276", 1.7},
        {"fmt/1129", 2 }
};

    /// <summary>
    /// Dictionary containing key value pairs with a list of output pronoms and a tuple containing the sDevice needed for ghostscript and the extension
    /// </summary>
	Dictionary<List<string>, Tuple<string, string>> keyValuePairs = new Dictionary<List<string>, Tuple<string, string>>() 
	{
		{new List<string> { "fmt/11", "fmt/12", "fmt/13", "fmt/935" }, new Tuple<string, string>("png16m", ".png")},    //PNG
		{new List<string> { "fmt/41", "fmt/42", "fmt/43", "fmt/44", "x-fmt/398", "x-fmt/390", "x-fmt/391", "fmt/645", "fmt/1507", "fmt/112" }, new Tuple<string, string>("jpeg", ".jpg")},  //JPG
		{new List<string> { "fmt/1917", "x-fmt/399", "x-fmt/388", "x-fmt/387", "fmt/155", "fmt/353", "fmt/154", "fmt/153", "fmt/156" }, new Tuple<string, string>("tiff24nc", ".tiff")},    //TIFF
		{new List<string> { "x-fmt/270", "fmt/115", "fmt/118", "fmt/119", "fmt/114", "fmt/116", "fmt/117" }, new Tuple<string, string>("bmp16m", ".bmp")},  //BMP
		{new List<string> { "fmt/15", "fmt/16", "fmt/17", "fmt/18", "fmt/19", "fmt/20", "fmt/276", "fmt/1129" }, new Tuple<string, string>("pdfwrite", ".pdf")} //PDF
	};

    /// <summary>
    /// Reference list stating supported conversions containing key value pairs with string input pronom and string output pronom
    /// </summary>
    /// <returns>List of all conversions</returns>
    public override Dictionary<string, List<string>> getListOfSupportedConvesions()
    {
        var supportedConversions = new Dictionary<string, List<string>>();
        //PDF to Image
        foreach (string pdfPronom in PDFPronoms)
        {
            supportedConversions.Add(pdfPronom, ImagePronoms);
        }
        //PostScript to PDF
        foreach (string postScriptPronom in PostScriptPronoms)
        {
            supportedConversions.Add(postScriptPronom, PDFPronoms);
        }

        return supportedConversions;
    }

    public override List<string> getSupportedOS()
    {
        var supportedOS = new List<string>();
        supportedOS.Add(PlatformID.Win32NT.ToString());
        supportedOS.Add(PlatformID.Unix.ToString());
        return supportedOS;
    }

    /// <summary>
    /// Convert a file to a new format
    /// </summary>
    /// <param name="fileinfo">The file to be converted</param>
    /// <param name="pronom">The file format to convert to</param>
	public override void ConvertFile(string fileinfo, string pronom)
    {
        string outputFileName = Path.GetFileNameWithoutExtension(fileinfo);
		string extension;
		string sDevice; //Needed for GhostScript CLI commands

		try
		{
			if (keyValuePairs.Any(kv => kv.Key.Contains(pronom)))                           //Sees if the output pronom is supported by GhostScript and sets sDevice and extension based on the pronom file format
			{
				extension = keyValuePairs.First(kv => kv.Key.Contains(pronom)).Value.Item2;
				sDevice = keyValuePairs.First(kv => kv.Key.Contains(pronom)).Value.Item1;

                switch (extension)
                {
                    case ".pdf":
                        string pdfVersion = pdfVersionMap[pronom].ToString();
                        convertToPDF(fileinfo, outputFileName, sDevice, extension, pdfVersion, pronom);
                        break;
                    case ".png":
                    case ".jpg":
                    case ".tiff": 
                    case ".bmp":
                        if (OperatingSystem.IsWindows())
                        {
                            convertToImageWindows(fileinfo, outputFileName, sDevice, extension, pronom);
                        }
                        else
                        {
                            convertToImagesLinux(fileinfo, outputFileName, sDevice, extension, pronom);
                        }
                        break;
                }
			}
		}catch(Exception e)
		{
			Logger.Instance.SetUpRunTimeLogMessage(pronom + " is not supported by GhostScript. File is not converted." + e.Message, true, fileinfo);
		}
		
    }

    /// <summary>
    /// Convert a file using GhostScript command line
    /// </summary>
    /// <param name="filePath">The file to be converted</param>
    /// <param name="outputFileName">The name of the new file</param>
    /// <param name="sDevice">What format GhostScript will convert to</param>
    /// <param name="extension">Extension type for after the conversion</param>
    void convertToImageWindows(string filePath, string outputFileName, string sDevice, string extension, string pronom)
	{
		try
		{
			using (var rasterizer = new GhostscriptRasterizer())
			{
				GhostscriptVersionInfo versionInfo = new GhostscriptVersionInfo(new Version(0, 0, 0), gsExecutable, string.Empty, GhostscriptLicense.GPL);
				using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					rasterizer.Open(stream, versionInfo, false);

					ImageFormat? imageFormat = GetImageFormat(extension);

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

                        int count = 1;
                        bool converted = false;
                        do
                        {
                            converted = CheckConversionStatus(filePath, outputFileName, pronom);
                            count++;
                            if (!converted)
                            {
                                convertToImageWindows(filePath, outputFileName, sDevice, extension, pronom);
                            }
                        } while (!converted && count < 4);
                        if (!converted)
                        {
                            throw new Exception("File was not converted");
                        }

						//Create folder for images with original name
						string folder = Path.GetFileNameWithoutExtension(filePath);
						string folderPath = Path.Combine(GlobalVariables.parsedOptions.Output, folder);
						Directory.CreateDirectory(folderPath);
						
						for(int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
						{
							string pageOutputFileName = outputFileName + "_" + pageNumber.ToString() + extension;
							string pageOutputFilePath = Path.Combine(GlobalVariables.parsedOptions.Output, pageOutputFileName);
							string pageOutputFilePathInFolder = Path.Combine(folderPath, pageOutputFileName);
							File.Move(pageOutputFilePath, pageOutputFilePathInFolder);
						}
		
                    }
				}
			}
		}
		catch (Exception e)
		{
			Logger.Instance.SetUpRunTimeLogMessage("Error when converting file with GhostScript. Error message: " + e.Message, true, filename: filePath);
		}
	}
    
    /// <summary>
    /// Convert a file to images using GhostScript command line
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="outputFileName"></param>
    /// <param name="sDevice"></param>
    /// <param name="extension"></param>
    /// <param name="pronom"></param>
    void convertToImagesLinux(string filePath, string outputFileName, string sDevice, string extension, string pronom)
    {
        try
        {
            string? outputFolder = Path.GetDirectoryName(filePath);
            string fullPath = Path.GetFullPath(filePath);
            string outputName = Path.Combine(fullPath, outputFileName);
            string formattedOutputName = $"\"{outputName}\"";       //To handle spaces in file names
            string formattedInputName = $"\"{filePath}\"";
            string command = $"gs -sDEVICE={sDevice} -o {formattedOutputName}%d{extension} {formattedInputName}";  // %d adds page number to filename, i.e outputFileName1.png outputFileName2.png

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "/bin/bash";
            startInfo.Arguments = $"-c \"{command}\"";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardError = true;

            using (Process? process = Process.Start(startInfo))
            {
                string? output = process?.StandardOutput.ReadToEnd();
                string? error = process?.StandardError.ReadToEnd();

                process?.WaitForExit();

                if (process?.ExitCode != 0 || process == null)
                {
                    Logger.Instance.SetUpRunTimeLogMessage($"Error when converting file with GhostScript. Exit code: {process?.ExitCode}, Error message: {error}", true, filename: filePath);
                }
            }

            int count = 1;
            bool converted = false;
            do
            {
                converted = CheckConversionStatus(filePath, outputFileName, pronom);
                count++;
                if (!converted)
                {
                    convertToImageWindows(filePath, outputFileName, sDevice, extension, pronom);
                }
            } while (!converted && count < 4);
            if (!converted)
            {
                throw new Exception("File was not converted");
            }

        }
        catch(Exception e)
        {
            Logger.Instance.SetUpRunTimeLogMessage("Error when converting file with GhostScript. Error message: " + e.Message, true, filename: filePath);
        }

    }

    //TODO: Make reachable only on Windows (which it already should be, but find out why it's still complaining)
    /// <summary>
    ///     Get the ImageFormat class for the given extension
    /// </summary>
    /// <param name="extension"> A string containing a file format extension </param>
    /// <returns> An ImageFormat class </returns>
	private ImageFormat ?GetImageFormat(string extension)
	{
		switch (extension)
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
    ///     Convert to PDF using GhostScript
    /// </summary>
    /// <param name="filePath"> Name and path of original file </param>
    /// <param name="outputFileName"> Filename of the converted file </param>
    /// <param name="sDevice"> Ghostscript variable that determines what conversion it should do </param>
    /// <param name="extension"> Extension for the new file </param>
    /// <param name="pdfVersion"> The PDF version to covnert to </param>
    /// <param name="pronom"> The output pronom </param>
	void convertToPDF(string filePath, string outputFileName, string sDevice, string extension, string pdfVersion, string pronom)
	{
		string? outputFolder = Path.GetDirectoryName(filePath);
        string fullPath = Path.GetFullPath(filePath);
        string path;
        //Remove everything from fullpath that is before outputFolder
        int index = fullPath.IndexOf(outputFolder);
        if (index > 0)
        {
            string relativePath = fullPath.Substring(index + outputFolder.Length);
            path = Path.Combine(GlobalVariables.parsedOptions.Output, relativePath);
        }
        else { throw new Exception("Error when converting file with GhostScript. Could not find output folder.");}

        string outputFilePath = Path.Combine(path, outputFileName + extension);
		string arguments = "-dCompatibilityLevel=" + pdfVersion + " -sDEVICE=pdfwrite -o " + outputFilePath + " " + filePath;
        string command;

		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo();
            if (OperatingSystem.IsWindows())
            {
                startInfo.FileName = gsExecutable;
                command = arguments;
            }
            else
            {
                startInfo.FileName = "/bin/bash";
                string linuxCommand = $"gs " +arguments;
                command = $"-c \"{linuxCommand}\"";
            }
			startInfo.Arguments = command;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.RedirectStandardOutput = true;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			using (Process? exeProcess = Process.Start(startInfo))
			{
				exeProcess?.WaitForExit();
			}

            int count = 1;
            bool converted = false;
            do
            {
                converted = CheckConversionStatus(filePath,outputFilePath, pronom);
                count++;
                if (!converted)
                {
                    convertToPDF(filePath, outputFileName, sDevice, extension, pdfVersion, pronom);
                }
            } while (!converted && count < 4);
            if (!converted)
            {
                throw new Exception("File was not converted");
            }

		}
		catch (Exception e)
		{
			Logger.Instance.SetUpRunTimeLogMessage("Error when converting file with GhostScript. Error message: " + e.Message, true, filename: filePath);
		}
	}

}


