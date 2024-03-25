using System;
using System.Collections.Generic;
using System.Text;

namespace FindMe_Application
{
    public interface ISmsReader
    {
        List<string> ReadSms();
    }
}
