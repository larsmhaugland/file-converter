using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using iText.Kernel.Geom;

public class Logger
{
	private static Logger? instance;
	private static readonly object lockObject = new object();
	string logPath;         // Path to log file
	string docPath;         // Path to documentation file
	public bool errorHappened { get; set; }             // True if an error message has been written
	// Configure JSON serializer options for pretty-printing
	JsonSerializerOptions options = new JsonSerializerOptions
	{
		WriteIndented = true,
	};

	public static class JsonRoot
	{
		public static string? requester { get; set; } // the person requesting the converting
		public static string? converter { get; set; } // the person that converts
	}
	/*
	JsonRoot root = new JsonRoot
	{
		requester = "requester",
		converter = "converter"
	};
	*/
	public class JsonData
	{
		public string? Filename { get; set; }
		public string? OriginalPronom { get; set; }
		public string? OriginalChecksum { get; set; }
		public long OriginalSize { get; set; }
		public string? TargetPronom { get; set; }
		public string? NewPronom { get; set; }
		public string? NewChecksum { get; set; }
		public long NewSize { get; set; }
		public List<string>? Converter { get; set; }
		public bool IsConverted { get; set; }
		public bool ShouldMerge { get; set; }
		public bool IsMerged { get; set; }
	}
	List<JsonData> data = [];

	private Logger()
	{
        string currentDirectory = Directory.GetCurrentDirectory();
        string path = System.IO.Path.Combine(currentDirectory, "logs");

        if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		DateTime currentDateTime = DateTime.Now;
		string formattedDateTime = currentDateTime.ToString("yyyy-MM-dd HHmmss");
		path += "/";
		logPath = path+ "log " + formattedDateTime + ".txt";
		// Write the specified text asynchronously to a new file.
		using (StreamWriter outputFile = new StreamWriter(logPath))
		{
			outputFile.WriteAsync("Type: | (Error) Message | Format | Filetype | Filename\n");
		}
		docPath = "";
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
	/// <param name="pronom"> Optional: the pronom of the file </param>
	/// <param name="mime"> Optional: mime of the file </param>
	/// <param name="filename"> Optional: the filename </param>
	/// <returns> returns the message in the correct format </returns>
	public void SetUpRunTimeLogMessage(string message, bool error, string pronom = "N/A", string mime = "N/A", string filename = "N/A")
	{
		string errorM = "Message: ";
		if (error) { errorM = "Error: "; errorHappened = true; }
		string formattedMessage =  errorM + " | " + pronom + " | " + mime + " | " + filename + " | " + message+"\n";
		WriteLog(formattedMessage, logPath);
	}

	/// <summary>
	/// Sets up how the final documentation file should be printed
	/// </summary>
	/// <param name="files"> list containing fileinfo about all files </param>
	public void SetUpDocumentation(List<FileInfo> files)
	{
		//TODO: Comment: Maybe find better place to put the file and set docPath earlier
		string path = GlobalVariables.parsedOptions.Output + "/";
		docPath = path + "documentation.json";
		using (StreamWriter outputFile = new StreamWriter(docPath))
		{
			outputFile.WriteAsync("\n");
		}
		foreach (FileInfo file in files)
		{
			JsonData jsonData;
			if (file.ShouldMerge)
			{
                jsonData = new JsonData
                {
                    Filename = file.FilePath,
                    OriginalPronom = file.OriginalPronom,
                    OriginalChecksum = file.OriginalChecksum,
                    OriginalSize = file.OriginalSize,
                    TargetPronom = Settings.GetTargetPronom(file),
                    NewPronom = file.NewPronom,
                    NewChecksum = file.NewChecksum,
                    NewSize = file.NewSize,
                    Converter = file.ConversionTools,
                    IsConverted = file.IsConverted
                };
            } else
			{
                jsonData = new JsonData
                {
                    Filename = file.FilePath,
                    OriginalPronom = file.OriginalPronom,
                    OriginalChecksum = file.OriginalChecksum,
                    OriginalSize = file.OriginalSize,
                    TargetPronom = Settings.GetTargetPronom(file),
                    NewPronom = file.NewPronom,
                    NewChecksum = file.NewChecksum,
                    NewSize = file.NewSize,
                    Converter = file.ConversionTools,
                    IsConverted = file.IsConverted
                };
            }
			
			data.Add(jsonData);
		}



		// Create an anonymous object with "requester" and "converter" properties
		var metadata = new
		{
			JsonRoot.requester,
			JsonRoot.converter
		};

		// Create an anonymous object with a "Files" property
		var jsonDataWrapper = new
		{
			Metadata = metadata,
			Files = data
		};

		// Serialize the wrapper object
		string json = JsonSerializer.Serialize(jsonDataWrapper, options);

		// Send it to writelog to print it out there
		WriteLog(json, docPath);
	}

	public void AskAboutReqAndConv()
	{
		if(JsonRoot.requester == null || JsonRoot.requester == "")
		{
			Console.WriteLine("Who is requesting the converting?");
			JsonRoot.requester = Console.ReadLine();
		}
		else
		{
			//Console.WriteLine("The requester " + JsonRoot.requester +" was found in the settings file.");
		}
		if(JsonRoot.converter == null || JsonRoot.converter == "")
		{
			Console.WriteLine("Who is converting?");
			JsonRoot.converter = Console.ReadLine();
		}
		else
		{
			//Console.WriteLine("The converter " + JsonRoot.converter + " was found in the settings file.");
		}
	}
}
