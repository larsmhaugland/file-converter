using System;
using System.IO;

public class Logger
{
	private static Logger instance;
	private static readonly object lockObject = new object();
	string fileName;		// Path to log file

	private Logger()
	{
	}
	public static Logger Instance
	{
		get
		{
			if (instance == null)
			{
				lock (lockObject)
				{
					if (instance == null)
					{
                        instance = new Logger();
                    }
				}
			}
			return instance;
		}
	}

	public void writeLog(string message, bool error)
	{
		lock(lockObject)
		{
            // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-write-text-to-a-file
            // Set a variable to the Documents path.
            string docPath = "output/logs/";

            // Write the specified text asynchronously to a new file.
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "log.txt")))
            {
                outputFile.WriteAsync(message);
            }
        }
    }

	/*
	private void writeErrorLog(string message)
	{
        lock (lockObject)
        {

        }
    }*/
}
