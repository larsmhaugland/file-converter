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

	/// <summary>
	/// writes a log to a file
	/// </summary>
	/// <param name="message">The message to be logged</param>
	public void writeLog(string message)
	{
		lock(lockObject)
		{
            // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-write-text-to-a-file
            // Set a variable to the Documents path.
            string docPath = "output/logs/";

            // Write the specified text asynchronously to a new file.
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "log.txt")))
            {
				outputFile.WriteAsync("Message: " + message);
            }
        }
    }

	/// <summary>
	/// sets up status and error messages to the correct format.
	/// </summary>
	/// <param name="format"> the fileformat </param>
	/// <param name="filetype"> the filetype </param>
	/// <param name="filename"> the filename </param>
	/// <param name="message"> the message to be sent </param>
	/// <param name="error"> true if it is an error </param>
	/// <returns> returns the message in the correct format </returns>
    string setUpMessage(string message, bool error, string format = "N/A", string filetype = "N/A", string filename = "N/A")
    {
		string errorM = "Message: ";
		if (error) { errorM = "Error: "; }
        return errorM + " | " + format + " | " + filetype + " | " + filename + " | " + message;
    }
    /*
	private void writeErrorLog(string message)
	{
        lock (lockObject)
        {

        }
    }*/
}
