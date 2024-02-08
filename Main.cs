using CommandLine;
using iText.IO.Font.Constants;
using System.Diagnostics;

public static class GlobalVariables
{
    public static Options parsedOptions = new Options();
    public static Dictionary<string, string> FileSettings = new Dictionary<string, string>();
	public static HashAlgorithms checksumHash;
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
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
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
		Console.WriteLine("Done!");
        Logger logger = Logger.Instance;

		FileManager fileManager = FileManager.Instance;
		Siegfried sf = Siegfried.Instance;

		//TODO: Check for malicous input files
		try
		{
			Console.WriteLine("Copying and unpacking files...");
			//Copy and unpack files
			sf.CopyFiles(GlobalVariables.parsedOptions.Input, GlobalVariables.parsedOptions.Output);
			Console.WriteLine("Done! Elapsed: {0}", stopwatch.Elapsed);
			Console.WriteLine("Identifying files...");
			//Identify files
			fileManager.IdentifyFiles();
			Console.WriteLine("Done! Elapsed: {0}", stopwatch.Elapsed);
		} catch (Exception e)
		{
			Console.WriteLine("Could not identify files: " + e.Message);
			logger.SetUpRunTimeLogMessage("Error when copying/unpacking/identifying files: " + e.Message, true);
			return;
		}

        ConversionManager cm = new ConversionManager();
        logger.AskAboutReqAndConv();

        if (fileManager.Files.Count > 0)
        {
			Console.WriteLine("Files identified: " + fileManager.Files.Count);
			Console.WriteLine("Converting files...");
            cm.ConvertFiles().Wait();
			Console.WriteLine("Done!");
			Console.WriteLine("Compressing folders...");
			sf.CompressFolders();
			Console.WriteLine("Done!");
			Console.WriteLine("Documenting conversion...");
            logger.SetUpDocumentation(fileManager.Files);
			Console.WriteLine("Done!");
        }
		stopwatch.Stop();
		Console.WriteLine("Time elapsed: " + stopwatch.Elapsed);
    }
}
