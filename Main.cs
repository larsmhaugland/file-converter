using CommandLine;
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

		Directory.SetCurrentDirectory("../../../");
		Settings settings = Settings.Instance;
		Console.WriteLine("Reading settings...");
		settings.ReadSettings("./Settings.xml");
		Logger logger = Logger.Instance;

		FileManager fileManager = FileManager.Instance;
		Siegfried sf = Siegfried.Instance;

		//TODO: Check for malicous input files
		try
		{
			Console.WriteLine("Copying and unpacking files...");
			//Copy files
			sf.CopyFiles(GlobalVariables.parsedOptions.Input, GlobalVariables.parsedOptions.Output);
			settings.SetUpFolderOverride("./Settings.xml");
			Console.WriteLine("Identifying files...");
			//Identify and unpack files
			fileManager.IdentifyFiles();
		} catch (Exception e)
		{
			Console.WriteLine("Could not identify files: " + e.Message);
			logger.SetUpRunTimeLogMessage("Error when copying/unpacking/identifying files: " + e.Message, true);
			return;
		}

		ConversionManager cm = ConversionManager.Instance;
		logger.AskAboutReqAndConv();

		if (fileManager.Files.Count > 0)
		{
			fileManager.DisplayFileList();
			Console.Write("Proceed? (Y/N): ");
			string input = Console.ReadLine();
			if (input.ToLower() != "y")
			{
                return;
            }
			Console.WriteLine("Converting files...");
			try
			{
				cm.ConvertFiles().Wait();
			} catch (Exception e)
			{
				logger.SetUpDocumentation(fileManager.Files);
				logger.SetUpRunTimeLogMessage("Error when converting files: " + e.Message, true);
			}
			Console.WriteLine("Compressing folders...");
			sf.CompressFolders();
			Console.WriteLine("Documenting conversion...");
			logger.SetUpDocumentation(fileManager.Files);
		}
	}
}
