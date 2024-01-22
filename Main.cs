using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Runtime.InteropServices;
using CommandLine;

class Program
{ 
	class Options
{
	[Option('i',"input", Required = false, HelpText = "Specify input directory",Default = "Input")]
	public string Input { get; set; }

	[Option('o',"output", Required = false, HelpText = "Specify output directory",Default ="Output")]
	public string Output { get; set; }

}
	static void Main(string[] args)
	{
		Options parsedOptions = null;

		Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
		{
			parsedOptions = options;
			if (options.Input != null)
			{
				Console.WriteLine("Input: " + options.Input);
			}
			if (options.Output != null)
			{
				Console.WriteLine("Output: " + options.Output);
			}
		});

		if (parsedOptions == null)
			return;

		Directory.SetCurrentDirectory("../../../");
		ConversionManager cm = new ConversionManager();

		Logger logger = Logger.Instance;

		FileManager fileManager = new FileManager(parsedOptions.Input,parsedOptions.Output);

		fileManager.IdentifyFiles();
        if (fileManager.Files.Count > 0)
        {
			Console.WriteLine("Files identified: " + fileManager.Files.Count);
			logger.SetUpDocumentation(fileManager.Files);
        }
    }
}
