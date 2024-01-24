using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Runtime.InteropServices;
using CommandLine;

public static class GlobalVariables
{
    public static Options parsedOptions = null;
}
public class Options
{
    [Option('i', "input", Required = false, HelpText = "Specify input directory", Default = "Input")]
    public string Input { get; set; }

    [Option('o', "output", Required = false, HelpText = "Specify output directory", Default = "Output")]
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
		ConversionManager cm = new ConversionManager();

		Logger logger = Logger.Instance;

        FileManager fileManager = new FileManager(GlobalVariables.parsedOptions.Input, GlobalVariables.parsedOptions.Output);

        logger.AskAboutReqAndConv();
        fileManager.IdentifyFiles();
		fileManager.ReadSettings("./Settings.xml");
        if (fileManager.Files.Count > 0)
        {
			Console.WriteLine("Files identified: " + fileManager.Files.Count);
            logger.SetUpDocumentation(fileManager.Files);
        }
    }
}
