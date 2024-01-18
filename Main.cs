using Microsoft.VisualBasic.FileIO;
using System;

class Program
{
	static void Main()
	{

		ConversionManager cm = new ConversionManager();

		Logger logger = Logger.Instance;
		FileManager fileManager = new FileManager("input/","output/");
		fileManager.IdentifyFiles();
	}
}
