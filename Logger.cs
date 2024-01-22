using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;

public class Logger
{
	private static Logger? instance;
	private static readonly object lockObject = new object();
	string fileName;        // Path to log file

    // Configure JSON serializer options for pretty-printing
    JsonSerializerOptions options = new JsonSerializerOptions
    {
        WriteIndented = true,
    };
    public class JsonData
    {
        public string? Filename { get; set; }
        public string? OriginalPronom { get; set; }
        public string? OriginalChecksum { get; set; }
        public long OriginalSize { get; set; }
        public string? NewPronom { get; set; }
        public string? NewChecksum { get; set; }
        public long NewSize { get; set; }
        public string[]? Converter { get; set; }
        public bool IsConverted { get; set; }
    }
    List<JsonData> data = new List<JsonData>();

    private Logger()
	{
        string docPath = "output/logs";

        if (!Directory.Exists(docPath))
        {
            Directory.CreateDirectory(docPath);
        }
        docPath += "/"; 

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
				outputFile.Write(message);
                outputFile.Flush();
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

    /// <summary>
    /// Sets up the layout for how a file should be written to the final documentation file
    /// </summary>
    /// <param name="fileinfo"> info about the file </param>
	public void SetUpDocumentation(List<FileInfo> files)
	{        
        foreach (FileInfo file in files)
        {
            JsonData jsondata = new JsonData();
            jsondata.Filename = file.FileName;
            jsondata.OriginalPronom = file.OriginalPronom;
            jsondata.OriginalChecksum = file.OriginalChecksum;
            jsondata.OriginalSize = file.OriginalSize;
            jsondata.NewPronom = file.NewPronom;
            jsondata.NewChecksum = file.NewChecksum;
            jsondata.NewSize = file.NewSize;
            jsondata.Converter = ["converter"];
            jsondata.IsConverted = file.IsConverted;
            data.Add(jsondata);
        }

        string json = JsonSerializer.Serialize(data, options);

        // Specify the path to your output JSON file
        string filePath = "output/documentation.json";

        // Send it to writelog to print it out there
        WriteLog(json, filePath);
    }
}
