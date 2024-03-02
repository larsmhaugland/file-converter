using iText.Html2pdf.Attach.Impl.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class TimeoutHelper : IDisposable
{
    private readonly CancellationTokenSource cts;
    private readonly Task timeoutTask;

    public TimeoutHelper(TimeSpan timeout)
    {
        cts = new CancellationTokenSource(timeout);
        timeoutTask = Task.Run(() => TimeoutCounter(cts.Token));
    }

    public void Start()
    {
        // Nothing specific to start in this example
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }

    private void TimeoutCounter(CancellationToken cancellationToken)
    {
        DateTime startTime = DateTime.Now;

        while (!cancellationToken.IsCancellationRequested)
        {
            // Periodic checks or work during the timeout period
            Thread.Sleep(1000);

            // Custom checks for cancellation
            // For example, check some condition related to your RunOfficeToPdfConversion method
            if (CheckCancellationCondition())
            {
                cts.Cancel();
            }
        }
    }

    private bool CheckCancellationCondition()
    {
        // Replace this with your specific cancellation condition logic
        // For example, if the conversion is complete or another condition is met
        return false;
    }
}