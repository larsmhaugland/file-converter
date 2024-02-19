using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
class AddConverters
{

    public List<Converter> GetConverters()
    {
        List<Converter> converters = new List<Converter>();
        converters.Add(new iText7());
        converters.Add(new GhostscriptConverter());
        converters.Add(new CognidoxConverter());
        return converters;
    }
    private static AddConverters? instance;
    private static readonly object lockObject = new object();
    public static AddConverters Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new AddConverters();
                    }
                }
            }
            return instance;
        }
    }
}


