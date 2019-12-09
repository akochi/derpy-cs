using System.Collections.Generic;
using System.IO;

static class StreamReaderExtensions
{
    public static IEnumerable<string> Lines(this StreamReader stream)
    {
        string line;
        while ((line = stream.ReadLine()) != null)
        {
            yield return line;
        }
    }
}
