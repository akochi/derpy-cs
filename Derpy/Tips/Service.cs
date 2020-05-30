using System.IO;
using System.Linq;
using System.Reflection;
using Derpy.Result;

namespace Derpy.Tips
{
    public class Service
    {
        readonly string[] _tips;

        public Service()
        {
            _tips = LoadTips();
        }

        private string[] LoadTips()
        {
            var assembly = Assembly.GetEntryAssembly();
            var stream = assembly.GetManifestResourceStream("Derpy.Tips.tips.txt");
            using var reader = new StreamReader(stream);

            return reader.Lines().ToArray();
        }

        public IResult GetTip() => new Reply(_tips.PickRandom());
    }
}
