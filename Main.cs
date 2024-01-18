using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;

class Program
{
	static void Main()
	{
		Directory.SetCurrentDirectory("../../../");
		ConversionManager cm = new ConversionManager();

		//Logger logger = Logger.Instance;
		FileManager fileManager = new FileManager("input/","output/");
		fileManager.IdentifyFiles();
        if (fileManager.Files.Count > 0)
        {
			logger.SetUpDocumentation(fileManager.Files[0]);
        }
    }
}
