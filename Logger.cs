using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Text.Json;

public class Logger
{
	private static Logger instance;
	private static readonly object lockObject = new object();
	string fileName;        // Path to log file

    private Logger()
	{
        string docPath = "output/logs/";

        // Write the specified text asynchronously to a new file.
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "log.txt")))
        {
            outputFile.WriteAsync("Type: | (Error) Message | Format | Filetype | Filename\n");
        }

		docPath = "output/";
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "documentation.json")))
        {
            outputFile.WriteAsync("\n");
        }

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
	/// <param name="message"> The message to be logged </param>
	/// <param name="filepath"> The filepath to the logfile </param>
	private void WriteLog(string message, string filepath)
	{
		lock(lockObject)
		{
			// https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-write-text-to-a-file    

			// Write the specified text asynchronously to a new file.
			using (StreamWriter outputFile = new StreamWriter(filepath, true))
			{
				outputFile.WriteAsync(message);
			}
        }
    }

    /// <summary>
    /// sets up status and error messages to the correct format.
    /// </summary>
	/// <param name="message"> the message to be sent </param>
    /// <param name="error"> true if it is an error </param>
    /// <param name="format"> Optional: the fileformat </param>
    /// <param name="filetype"> Optional: the filetype </param>
    /// <param name="filename"> Optional: the filename </param>
    /// <returns> returns the message in the correct format </returns>
    public void SetUpRunTimeLogMessage(string message, bool error, string format = "N/A", string filetype = "N/A", string filename = "N/A")
    {
		string errorM = "Message: ";
		if (error) { errorM = "Error: "; }
        string formattedMessage =  errorM + " | " + format + " | " + filetype + " | " + filename + " | " + message+"\n";
		WriteLog(formattedMessage, "output/logs/log.txt");
    }

	public void SetUpDocumentation(FileInfo fileinfo)
	{
        // JSON data
        var jsonData = new
        {
            FileName = fileinfo.FileName,
            OriginalPronom = fileinfo.OriginalPronom,
            OriginalChecksum = fileinfo.OriginalChecksum,
            OriginalSize = fileinfo.OriginalSize,
            NewPronom = fileinfo.NewPronom,
            NewChecksum = fileinfo.NewChecksum,
            NewSize = fileinfo.NewSize,
            Converter = "converter",
            IsConverted = fileinfo.IsConverted
        };

        // Convert the object to JSON with indentation
        string jsonString = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        // Specify the path to your output JSON file
        string filePath = "output/documentation.json";

        // Send it to writelog to print it out there
        WriteLog( jsonString, filePath);
    }
    /*
	private void writeErrorLog(string message)
	{
        lock (lockObject)
        {

        }
    }*/
}
