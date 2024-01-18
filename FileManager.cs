using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class FileManager
{
	string InputFolder;		// Path to input folder
	string OutputFolder;    // Path to output folder
	List<FileInfo> Files;	// List of files to be converted

	private FileManager()
	{
	}

	public FileManager(string input, string output)
	{
		InputFolder = input;
		OutputFolder = output;
	}

    public void IdentifyFiles()
    {
		string[] filePaths = Directory.GetFiles(InputFolder, "*.*", SearchOption.AllDirectories);
        Parallel.ForEach(filePaths, filePath =>
        {
            FileInfo file = GetFileInfo(filePath);
            if (file != null)
                Files.Add(file);
        });
    }

    static FileInfo GetFileInfo(string filePath)
    {
        // Wrap the file path in quotes
        filePath = "\"" + filePath + "\"";
        // Define the process start info
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = @"ConversionTools/sf.exe", // or any other command you want to run
            Arguments = filePath,
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        string output = "";
        string error = "";
        // Create the process
        using (Process process = new Process { StartInfo = psi })
        {
            // Start the process
            process.Start();

            // Read the output
            output = process.StandardOutput.ReadToEnd();
            error = process.StandardError.ReadToEnd();

            // Wait for the process to exit
            process.WaitForExit();                     
        }
        if (error.Length > 0)
        {
            Logger.Instance.SetUpRunTimeLogMessage("FileManager SF " + error, true);
            return null;
        }
        return new FileInfo(output);
    }
}
