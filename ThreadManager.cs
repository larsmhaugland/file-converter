using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

class ThreadManager
{
	private static ThreadManager ?instance = null;
	private static List<Thread> threads = new List<Thread>();
	private static readonly object padlock = new object();
	private static SemaphoreSlim semaphore = new SemaphoreSlim(0, int.MaxValue);
	private static Queue<Action> taskQueue = new Queue<Action>();
	private static bool ExitThreadWorkers = false;

	private ThreadManager()
	{
		int threadCount = Environment.ProcessorCount * 2;

        // Initialize threads
        for (int i = 0; i < threadCount; i++)
		{
			Thread t = new Thread(ThreadWorker);
			t.Start();
			threads.Add(t);
		}
	}

	public static ThreadManager Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new ThreadManager();
				}
				return instance;
			}
		}
	}

	private void ThreadWorker()
	{
		while (!ExitThreadWorkers)
		{
			semaphore.Wait(); // Wait until a task is available
			Action task;
			lock (taskQueue)
			{
				if (taskQueue.Count == 0 && ExitThreadWorkers)
				{
					// Exit the loop if there are no more tasks and the exitThreadWorkers flag is set
					break;
				}
				task = taskQueue.Dequeue();
			}
			// Perform task
			task.Invoke();
		}
	}

	private void StartTaskInternal(Action function)
	{
		// Perform initialization or other setup if needed

		// Execute the task
		function();

		// Perform cleanup if needed
	}

	public void WaitAll()
	{
		ExitThreadWorkers = true;
		semaphore.Release(threads.Count);
		foreach (Thread t in threads)
		{
			t.Join();
		}
	}

	public void StartAll()
	{
		ExitThreadWorkers = false;
		for (int i = 0; i < threads.Count; i++)
		{
			threads[i] = new Thread(ThreadWorker);
			threads[i].Start();
		}
	}

	public async Task StartTask(Action function)
	{
		await Task.Run(() =>
		{
			lock (taskQueue)
			{
				taskQueue.Enqueue(() => StartTaskInternal(function));
			}
			semaphore.Release();
		});
	}
}
