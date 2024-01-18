using System;

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

	private void writeLog(string message)
	{
    }

	private void writeErrorLog(string message)
	{
    }
}
