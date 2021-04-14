using System.IO;
using System.Linq;
using System.Reflection;
using Derpy.Result;

namespace Derpy.Tips
{
    public class Service
    {
        readonly string[] _tips;

        public static readonly string STEPS_TIP = string.Join("\n", new string[] {
            "Step 1. copy a work.",
            "Step 2. change the angle/pose of the work",
            "Step 3. With only step 2 as a reference make another example of the work",
            "Step 4. Make a piece without reference of the work.",
            "If unsuccessful or unhappy with the results return to step 1 and repeat."
        });

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

        public IResult GetSteps() => new Reply(STEPS_TIP);
    }
}
