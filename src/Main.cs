using CommandLine;
using iText.Layout.Splitting;
using System.Diagnostics;

public static class GlobalVariables
{
    public static Options parsedOptions = new Options();
	//Map with all specified conversion formats, to and from
    public static Dictionary<string, string> FileSettings = new Dictionary<string, string>(); // the key is pronom code 
	// Map with info about what folders have overrides for specific formats
    public static Dictionary<string, SettingsData> FolderOverride = new Dictionary<string, SettingsData>(); // the key is a foldername
    public static HashAlgorithms checksumHash;
	public static int maxThreads = Environment.ProcessorCount*2;
	public static int timeout = 30;
}
public class Options
{
	[Option('i', "input", Required = false, HelpText = "Specify input directory", Default = "input")]
	public string Input { get; set; } = "";
	[Option('o', "output", Required = false, HelpText = "Specify output directory", Default = "output")]
	public string Output { get; set; } = "";

}
class Program
{ 
	static void Main(string[] args)
	{
		Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
		{
			GlobalVariables.parsedOptions = options;
			if (options.Input != null)
			{
				Console.WriteLine("Input: " + options.Input);
			}
			if (options.Output != null)
			{
				Console.WriteLine("Output: " + options.Output);
			}
		});

		if (GlobalVariables.parsedOptions == null)
			return;
		//Only maximize and center the console window if the OS is Windows
		
		MaximizeAndCenterConsoleWindow();
		if (!OperatingSystem.IsLinux())
		{
            Directory.SetCurrentDirectory("../../../");
        }
		else
		{
			LinuxSetup.Setup();
		}
		Settings settings = Settings.Instance;
		Console.WriteLine("Reading settings...");
		settings.ReadSettings("./Settings.xml");
        settings.SetUpFolderOverride("./Settings.xml");
        Logger logger = Logger.Instance;

		FileManager fileManager = FileManager.Instance;
        Siegfried sf = Siegfried.Instance;
		
        //TODO: Check for malicous input files
        try
		{
			//Check if user wants to use files from previous run
			sf.AskReadFiles();
			//Check if files were added from previous run
			if (!sf.Files.IsEmpty)
			{
				//Import files from previous run
				Console.WriteLine("Checking files from previous run...");
				fileManager.ImportFiles(sf.Files.ToList());
				var compressedFiles = sf.IdentifyCompressedFilesJSON(GlobalVariables.parsedOptions.Input);
				fileManager.ImportCompressedFiles(compressedFiles);
			}
			else
			{
				Console.WriteLine("Copying files from {0} to {1}...",GlobalVariables.parsedOptions.Input, GlobalVariables.parsedOptions.Output);
				//Copy files
				sf.CopyFiles(GlobalVariables.parsedOptions.Input, GlobalVariables.parsedOptions.Output);
				Console.WriteLine("Identifying files...");
				//Identify and unpack files
				fileManager.IdentifyFiles();
			}
		} catch (Exception e)
		{
			Console.WriteLine("[FATAL] Could not identify files: " + e.Message);
			logger.SetUpRunTimeLogMessage("Main: Error when copying/unpacking/identifying files: " + e.Message, true);
			return;
		}
		ConversionManager cm = ConversionManager.Instance;
		

		if (fileManager.Files.Count > 0)
		{			
			string input;
			do
			{
                logger.AskAboutReqAndConv();
				//settings.AskAboutEmptyDefaults();
                fileManager.DisplayFileList();
				var oldColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine("Requester: {0}",Logger.JsonRoot.requester);
				Console.WriteLine("Converter: {0}",Logger.JsonRoot.converter);
				Console.WriteLine("MaxThreads: {0}", GlobalVariables.maxThreads);
				Console.WriteLine("Timeout in minutes: {0}", GlobalVariables.timeout);
				Console.ForegroundColor = oldColor;
                Console.Write("Do you want to proceed with these settings (Y (Yes) / A (Abort) / R (Reload) / G (Change in GUI): ");
                string ?r = Console.ReadLine();
				r = r?.ToUpper() ?? " ";
				input = r;
                if (input == "R")
                {
                    Console.WriteLine("Change settings file and hit enter when finished (Remember to save file)");
                    Console.ReadLine();
                    settings.ReadSettings("./Settings.xml");
                    settings.SetUpFolderOverride("./Settings.xml");
                }
				if (input == "G")
				{
					//TODO: Start GUI
					Console.WriteLine("Not implemented yet...");
                    settings.ReadSettings("./Settings.xml");
                    settings.SetUpFolderOverride("./Settings.xml");
                }
            } while (input != "Y" && input != "A");
			if (input == "A")
			{
                return;
            }
            Console.WriteLine("Converting files...");

            
            try
			{
                cm.ConvertFiles();
				//Delete siegfrieds json files
				sf.ClearOutputFolder();
			} catch (Exception e)
			{
                Console.WriteLine("Error while converting " + e.Message);
                logger.SetUpRunTimeLogMessage("Main: Error when converting files: " + e.Message, true);
			}
			finally
			{
				Console.WriteLine("Conversion finished:");
				fileManager.DisplayFileList();
                Console.WriteLine("Documenting conversion...");
                fileManager.DocumentFiles();
			}
			Console.WriteLine("Compressing folders...");
			sf.CompressFolders();
		}
	}
    static void MaximizeAndCenterConsoleWindow()
    {
		//Only maximize and center the console window if the OS is Windows
		if (Environment.OSVersion.Platform != PlatformID.Win32NT)
		{
			return;
		}
        int screenWidth = Console.LargestWindowWidth;
        int screenHeight = Console.LargestWindowHeight;

        int windowWidth = screenWidth ;  // You can adjust this as needed
        int windowHeight = screenHeight; // You can adjust this as needed

        Console.SetWindowSize(windowWidth, windowHeight);
        Console.BufferHeight = windowHeight;
        Console.BufferWidth = windowWidth;

        int left = Math.Max((screenWidth - windowWidth) / 2, 0);
        int top = Math.Max((screenHeight - windowHeight) / 2, 0);

        //Console.SetWindowPosition(left, top);
    }
}
