using System.Collections.Generic;
using System.Linq;

namespace XamarinFormsLiveSync.Core.XamlParser
{
    public class AstNode
    {

        public string Name { get; set; }
        public string Namespace { get; set; }
        public Dictionary<string, List<AstNode>> ElementProperties { get; set; }
        public Dictionary<string, string> AttributeProperties { get; set; }
        public Dictionary<string, string> AttachedProperties { get; set; }
        public List<AstNode> Childrens { get; set; }

        public AstNode()
        {
            AttributeProperties = new Dictionary<string, string>();
            AttachedProperties = new Dictionary<string, string>();
            ElementProperties = new Dictionary<string, List<AstNode>>();

            Childrens = new List<AstNode>();
        }

        public override string ToString()
        {
            return Childrens.Count > 0 ? $"<{Name}>...</{Name}>" : $"<{Name} />";
        }
    }
}
