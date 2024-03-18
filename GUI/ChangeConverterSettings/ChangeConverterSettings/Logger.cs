using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.Collections.Generic;

public class Logger
{
    private static Logger? instance;
    private static readonly object lockObject = new object();
    string logPath;         // Path to log file
    string docPath;         // Path to documentation file
                            // Configure JSON serializer options for pretty-printing
    JsonSerializerOptions options = new JsonSerializerOptions
    {
        WriteIndented = true,
    };
    

    private Logger()
    {
        string path = "./logs/";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("yyyy-MM-dd HHmmss");
        path += "/";
        logPath = path + "log " + formattedDateTime + ".txt";
        // Write the specified text asynchronously to a new file.
        using (StreamWriter outputFile = new StreamWriter(logPath))
        {
            outputFile.WriteAsync("Type: | (Error) Message | Pronom Code | Mime Type | Filename\n");
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
        lock (lockObject)
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
        if (error) { errorM = "Error: "; }
        string formattedMessage = errorM + " | " + message + " | " + pronom + " | " + mime + " | " + filename + "\n";
        WriteLog(formattedMessage, logPath);
    }

    
}
