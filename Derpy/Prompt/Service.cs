using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Derpy.Result;

namespace Derpy.Prompt
{
    public class Service : IListProvider
    {
        private readonly Dictionary<string, Item[]> _lists;

        public Service()
        {
            _lists = new Dictionary<string, Item[]> { };

            var assembly = Assembly.GetEntryAssembly();
            var stream = assembly.GetManifestResourceStream("Derpy.Prompt.Prompts.xml");
            var document = XElement.Load(stream);

            foreach (var listElement in document.Elements())
            {
                _lists.Add(
                    listElement.Attribute("name").Value,
                    (from el in listElement.Elements() select new Item(el)).ToArray()
                );
            }
        }

        public Item[] GetItems(string list) => _lists[list];

        public IResult GetRandomPrompt()
        {
            return new Reply(GetItems("message").PickRandom().ToString(this) + "\n\n> " + GetItems("prompt").PickRandom().ToString(this));
        }
    }
}
