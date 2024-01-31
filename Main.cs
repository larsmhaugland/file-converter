using CommandLine;
using System.Diagnostics;

public static class GlobalVariables
{
    public static Options parsedOptions = null;
    public static Dictionary<string, string> FileSettings = new Dictionary<string, string>();
}
public class Options
{
    [Option('i', "input", Required = false, HelpText = "Specify input directory", Default = "input")]
    public string Input { get; set; }
    [Option('o', "output", Required = false, HelpText = "Specify output directory", Default = "output")]
    public string Output { get; set; }

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
		
		Logger logger = Logger.Instance;

		FileManager fileManager = FileManager.Instance;
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		fileManager.IdentifyFiles();
        stopwatch.Stop();
        ConversionManager cm = new ConversionManager();
        Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		fileManager.ReadSettings("./Settings.xml");
        logger.AskAboutReqAndConv();
		
        if (fileManager.Files.Count > 0)
        {
			Console.WriteLine("Files identified: " + fileManager.Files.Count);
            logger.SetUpDocumentation(fileManager.Files);
            cm.ConvertFiles();
			Siegfried sf = Siegfried.Instance;
			sf.CompressFolders();
        }
    }
}
