using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Derpy.Prompt
{
    public interface IListProvider
    {
        public Item[] GetItems(string list);
    }

    public struct Item
    {
        private readonly IToken[] _tokens;

        public Item(IEnumerable<IToken> tokens) => _tokens = tokens.ToArray();
        public Item(XElement xml) : this(XmlToTokens(xml)) { }

        private static IEnumerable<IToken> XmlToTokens(XElement xml) =>
            from el in xml.Nodes()
            select (el is XText ? new StringToken(el as XText) as IToken : new PlaceholderToken(el as XElement) as IToken);

        public string ToString(IListProvider provider) => string.Join("", _tokens.Select(token => token.ToString(provider)));
    }

    public interface IToken
    {
        string ToString(IListProvider provider);
    }

    public struct StringToken : IToken
    {
        private readonly string _content;

        public StringToken(string content) => _content = content;
        public StringToken(XText element) : this(element.Value) { }

        public string ToString(IListProvider _)
        {
            return _content;
        }
    }

    public struct PlaceholderToken : IToken
    {
        private readonly string _listName;

        public PlaceholderToken(string listName) => _listName = listName;
        public PlaceholderToken(XElement el) : this(el.Attribute("type").Value) { }

        public string ToString(IListProvider provider) => provider.GetItems(_listName).PickRandom().ToString(provider);
    }
}
