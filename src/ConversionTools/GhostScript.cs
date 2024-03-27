using System.Diagnostics;
using Ghostscript.NET.Rasterizer;
using System.Drawing.Imaging;
using Ghostscript.NET;

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
		SetNameAndVersion();
		SupportedConversions = getListOfSupportedConvesions();
		SupportedOperatingSystems = getSupportedOS();
	  
	}

    public override void GetVersion()
    {
		//TODO: Actually fetch version
		Version = "1.23.1";
    }

    public string gsExecutable = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GhostscriptBinaryFiles", "gs10.02.1", "bin", "gswin64c.exe");
    public string gsLibrary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GhostscriptBinaryFiles", "gs10.02.1", "bin", "gsdll64.dll");

    List<string> ImagePronoms = 
		[
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
	List<string> PDFPronoms = 
		[
		"fmt/15",
		"fmt/16",
		"fmt/17",
		"fmt/18",
		"fmt/19",
		"fmt/20",
		"fmt/276",
		"fmt/1129"
		];
	List<string> PostScriptPronoms = 
		[
		"fmt/124",
		"x-fmt/91",
		"x-fmt/406",
		"x-fmt/407",
		"x-fmt/408",
		"fmt/501"
		];

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

	Dictionary<List<string>, Tuple<string, string>> keyValuePairs = new Dictionary<List<string>, Tuple<string, string>>() 
	{
		{new List<string> { "fmt/11", "fmt/12", "fmt/13", "fmt/935" }, new Tuple<string, string>("png16m", ".png")},
		{new List<string> { "fmt/41", "fmt/42", "fmt/43", "fmt/44", "x-fmt/398", "x-fmt/390", "x-fmt/391", "fmt/645", "fmt/1507", "fmt/112" }, new Tuple<string, string>("jpeg", ".jpg")},
		{new List<string> { "fmt/1917", "x-fmt/399", "x-fmt/388", "x-fmt/387", "fmt/155", "fmt/353", "fmt/154", "fmt/153", "fmt/156" }, new Tuple<string, string>("tiff24nc", ".tiff")},
		{new List<string> { "x-fmt/270", "fmt/115", "fmt/118", "fmt/119", "fmt/114", "fmt/116", "fmt/117" }, new Tuple<string, string>("bmp16m", ".bmp")},
		{new List<string> { "fmt/15", "fmt/16", "fmt/17", "fmt/18", "fmt/19", "fmt/20", "fmt/276", "fmt/1129" }, new Tuple<string, string>("pdfwrite", ".pdf")}
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
		//Add more supported OS here
		return supportedOS;
	}

	/// <summary>
	/// Convert a file to a new format
	/// </summary>
	/// <param name="filePath">The file to be converted</param>
	/// <param name="pronom">The file format to convert to</param>
	public override void ConvertFile(FileToConvert file, string pronom)
	{
		string outputFileName = Path.GetFileNameWithoutExtension(file.FilePath);
		string extension;
		string sDevice;

		try
		{
			if (keyValuePairs.Any(kv => kv.Key.Contains(pronom)))
			{
				extension = keyValuePairs.First(kv => kv.Key.Contains(pronom)).Value.Item2;
				sDevice = keyValuePairs.First(kv => kv.Key.Contains(pronom)).Value.Item1;

				if (extension == ".pdf")
				{
					string pdfVersion = pdfVersionMap[pronom].ToString();
					convertToPDF(file, outputFileName, sDevice, extension, pdfVersion, pronom);
				}
				else if (extension == ".png" || extension == ".jpg" || extension == ".tiff" || extension == ".bmp")
				{
					if(OperatingSystem.IsWindows())
					{
                        convertToImagesWindows(file, outputFileName, sDevice, extension, pronom);
                    }
					else 
					{
						convertToImagesLinux(file, outputFileName, sDevice, extension, pronom);
					}
				}
			}
		}catch(Exception e)
		{
			Logger.Instance.SetUpRunTimeLogMessage(pronom + " is not supported by GhostScript. File is not converted.", true, file.FilePath);
		}
	}

	/// <summary>
	/// Convert a file using GhostScript command line
	/// </summary>
	/// <param name="filePath">The file to be converted</param>
	/// <param name="outputFileName">The name of the new file</param>
	/// <param name="sDevice">What format GhostScript will convert to</param>
	/// <param name="extension">Extension type for after the conversion</param>
	void convertToImagesWindows(FileToConvert file, string outputFileName, string sDevice, string extension, string pronom)
	{
		Logger log = Logger.Instance;
		if (!System.OperatingSystem.IsWindowsVersionAtLeast(6,1) ) 
		{
			log.SetUpRunTimeLogMessage("GhostScript is not supported on this version of Windows (6.1). File is not converted.", true, filename: file.FilePath);
			return; 
		}
		try
		{
            int count = 0;
			bool converted = true;
			List<FileInfo> files;
			var originalFileInfo = FileManager.Instance.GetFile(file.Id);
			if(originalFileInfo == null)
			{
				file.Failed = true;
				return;
			}
			do {
				files = new List<FileInfo>(); //Clear list of files

                //Create folder for images with original name
                string filename = Path.GetFileNameWithoutExtension(file.FilePath);
				string relPath = Path.GetRelativePath(Directory.GetCurrentDirectory(),file.FilePath);
				 
                string folderPath = relPath.Substring(0, relPath.LastIndexOf('.'));
                if (Directory.Exists(folderPath))
				{
                    //Clear folder if it already exists
                    Directory.Delete(folderPath, true);
                }
				Directory.CreateDirectory(folderPath);

                using (var rasterizer = new GhostscriptRasterizer())
				{
					GhostscriptVersionInfo versionInfo = new GhostscriptVersionInfo(new Version(), gsLibrary, string.Empty, GhostscriptLicense.GPL);
					using (var stream = new FileStream(file.FilePath, FileMode.Open, FileAccess.Read))
					{
						rasterizer.Open(stream, versionInfo, false);
						ImageFormat? imageFormat = GetImageFormat(extension);
						if (imageFormat != null)
						{
							for (int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
							{
								string pageOutputFileName = String.Format("{0}{1}{2}_{3}{4}", folderPath, Path.DirectorySeparatorChar, outputFileName, pageNumber.ToString(), extension);
								using (var image = rasterizer.GetPage(300, pageNumber))
								{
									image.Save(pageOutputFileName, imageFormat);
								}

								var newFile = new FileInfo(pageOutputFileName, originalFileInfo);
								newFile.AddConversionTool(NameAndVersion);
								files.Add(newFile);
							}
						}
					}
				}
				foreach (var newFile in files)
				{
					converted = CheckConversionStatus(newFile.FilePath, pronom);
					//It is only relevant to check if at least one file is not converted, rest will be checked at the end of conversion
					if (!converted)
					{
						break;
					}
				}
			} while (!converted && ++count < GlobalVariables.MAX_RETRIES);
            FileManager.Instance.AddFiles(files);
			if (converted)
			{
				deleteOriginalFileFromOutputDirectory(file.FilePath);
				originalFileInfo.Display = false;
			}
			file.Failed = !converted;
        }
		catch (Exception e)
		{
			log.SetUpRunTimeLogMessage("Error when converting file with GhostScript. Error message: " + e.Message, true, filename: file.FilePath);
		}
	}

	private ImageFormat ?GetImageFormat(string extension)
	{
        if (!System.OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            Logger.Instance.SetUpRunTimeLogMessage("GhostScript is not supported on this version of Windows (6.1).", true);
            return null;
        }
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
    /// Convert a file to images using GhostScript command line
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="outputFileName"></param>
    /// <param name="sDevice"></param>
    /// <param name="extension"></param>
    /// <param name="pronom"></param>
    void convertToImagesLinux(FileToConvert file, string outputFileName, string sDevice, string extension, string pronom)
    {
        try
        {
            int count = 0;
            bool converted = true;
			List<FileInfo> files;
            var originalFileInfo = FileManager.Instance.GetFile(file.Id);

			do
			{
				files = new List<FileInfo>(); //Clear list of files
				//Create folder for images with original name
				string folder = Path.GetFileNameWithoutExtension(file.FilePath);
				string folderPath = Path.Combine(GlobalVariables.parsedOptions.Output, folder);
				if (Directory.Exists(folderPath))
				{
					//Clear folder if it already exists
					Directory.Delete(folderPath, true);
				}
				Directory.CreateDirectory(folderPath);

				string command = $"gs -sDEVICE={sDevice} -o {folderPath}{outputFileName}%d{extension} {file.FilePath}";  // %d adds page number to filename, i.e outputFileName1.png outputFileName2.png

				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.FileName = "/bin/bash";
				startInfo.Arguments = $"-c \"{command}\"";
				startInfo.RedirectStandardOutput = true;
				startInfo.UseShellExecute = false;
				startInfo.CreateNoWindow = true;
				startInfo.RedirectStandardError = true;

				using (Process? process = Process.Start(startInfo))
				{
					string? output = process?.StandardOutput.ReadToEnd();
					string? error = process?.StandardError.ReadToEnd();

					process?.WaitForExit();
				}

				var filesInFolder = Directory.GetFiles(folderPath);
				foreach (var fileInFolder in filesInFolder)
				{
					var newFile = new FileInfo(fileInFolder, originalFileInfo);
					files.Add(newFile);
                    //Only checking as long as a file hasn't been converted, rest will be checked at the end of conversion
                    converted = converted && CheckConversionStatus(newFile.FilePath, pronom);
                }
            } while (!converted && ++count < GlobalVariables.MAX_RETRIES);
			FileManager.Instance.AddFiles(files);
            file.Failed = !converted;
        }
        catch (Exception e)
        {
            Logger.Instance.SetUpRunTimeLogMessage("Error when converting file with GhostScript. Error message: " + e.Message, true, filename: file.FilePath);
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
	void convertToPDF(FileToConvert file, string outputFileName, string sDevice, string extension, string pdfVersion, string pronom)
    {
        string? outputFolder = Path.GetDirectoryName(file.FilePath);

        string outputFilePath = Path.Combine(outputFolder, outputFileName + extension);
        string arguments = "-dCompatibilityLevel=" + pdfVersion + " -sDEVICE=pdfwrite -o " + outputFilePath + " " + file.FilePath;
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
                string linuxCommand = $"gs " + arguments;
                command = $"-c \"{linuxCommand}\"";
            }
            startInfo.Arguments = command;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            using (Process? exeProcess = Process.Start(startInfo))	//TODO: System.ComponentModel.Win32Exception: 'An error occurred trying to start process 'C:\Users\larsm\source\repos\file-converter\bin\Debug\net8.0\GhostscriptBinaryFiles\gs10.02.1\bin\gsdll64.dll' with working directory 'C:\Users\larsm\source\repos\file-converter'. The specified executable is not a valid application for this OS platform.'

            {
                exeProcess?.WaitForExit();
            }

            int count = 1;
            bool converted = false;
            do
            {
                converted = CheckConversionStatus( outputFilePath, pronom, file);
                count++;
                if (!converted)
                {
                    convertToPDF(file, outputFileName, sDevice, extension, pdfVersion, pronom);
                }
            } while (!converted && count < 4);
            if (!converted)
            {
                file.Failed = true;
            }
        }
        catch (Exception e)
        {
            Logger.Instance.SetUpRunTimeLogMessage("Error when converting file with GhostScript. Error message: " + e.Message, true, filename: file.FilePath);
        }
    }
}


