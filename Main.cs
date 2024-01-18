using Microsoft.VisualBasic.FileIO;
using System;

class Program
{
	static void Main()
	{

		ConversionManager cm = new ConversionManager();

		Logger logger = Logger.Instance;
		logger.writeLog("sendt fra main", false);


	}
}
