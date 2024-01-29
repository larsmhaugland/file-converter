using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Runtime.InteropServices;
using CommandLine;
using System.Diagnostics;

public static class GlobalVariables
{
    public static Options parsedOptions = null;
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
		ConversionManager cm = new ConversionManager();

		Logger logger = Logger.Instance;

		FileManager fileManager = FileManager.Instance;

		fileManager.IdentifyFiles();
		fileManager.ReadSettings("./Settings.xml");
        logger.AskAboutReqAndConv();
		
        if (fileManager.Files.Count > 0)
        {
			Console.WriteLine("Files identified: " + fileManager.Files.Count);
            logger.SetUpDocumentation(fileManager.Files);
            foreach (FileInfo fileInfo in fileManager.Files)
            {
				cm.ConvertFiles(fileInfo, fileInfo.OriginalPronom);
            }
        }
    }
}
