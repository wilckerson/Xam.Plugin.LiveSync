using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Xam.Plugin.LiveSync.XamlParser
{
    public class XamlAstParser
    {
        
        public AstNode ExtractAst(string xaml)
        {
            var rootNode = new AstNode();

            xaml = xaml?.TrimStart();

            using (var reader = XmlReader.Create(new StringReader(xaml)))
            {
                while (reader.Read())
                {
                    //Skip until element
                    if (reader.NodeType == XmlNodeType.XmlDeclaration)
                        continue;
                    if (reader.NodeType == XmlNodeType.Whitespace)
                        continue;
                    if (reader.NodeType != XmlNodeType.Element)
                    {
                        Debug.WriteLine("Unhandled node {0} {1} {2}", reader.NodeType, reader.Name, reader.Value);
                        continue;
                    }

                    XElement elm = XNode.ReadFrom(reader) as XElement;
                    rootNode = ParseElement(elm);
                }
            }

            return rootNode;
        }

        AstNode ParseElement(XElement elm)
        {

            var node = new AstNode();
            node.Name = elm.Name.LocalName;
            node.Namespace = elm.Name.NamespaceName;

            ParseAttributeProperties(node, elm);
            ParseChildrens(node, elm);

            node.TextContent = elm.Value;

            return node;
        }
        void ParseAttributeProperties(AstNode node, XElement elm)
        {
            var lstAttr = elm.Attributes();
            foreach (var attr in lstAttr)
            {
                if (attr.IsNamespaceDeclaration) continue;

                var propName = attr.Name.LocalName;
                //Verifica se é um AttachedProperty
                if (attr.Name.LocalName.Contains("."))
                {
                    if (node.AttachedProperties.ContainsKey(propName))
                    {
                        node.AttachedProperties[propName] = attr.Value;
                    }
                    else
                    {
                        node.AttachedProperties.Add(propName, attr.Value);
                    }
                }
                else
                {
                    if (node.AttributeProperties.ContainsKey(propName))
                    {
                        node.AttributeProperties[propName] = attr.Value;
                    }
                    else
                    {
                        node.AttributeProperties.Add(propName, attr.Value);
                    }
                }
            }
        }
        void ParseElementProperty(AstNode node, XElement elm)
        {
            var propName = elm.Name.LocalName.Replace($"{node.Name}.", String.Empty);

            if (elm.HasElements)
            {
                var lstElm = elm.Elements();
                node.ElementProperties.Add(propName, new List<AstNode>());
                foreach (var subElm in lstElm)
                {
                    var subNode = ParseElement(subElm);
                    node.ElementProperties[propName].Add(subNode);
                }
            }
            else
            {
                if (node.AttributeProperties.ContainsKey(propName))
                {
                    node.AttributeProperties[propName] =  elm.Value;
                }
                else
                {
                    node.AttributeProperties.Add(propName, elm.Value);
                }
            }
        }

        void ParseChildrens(AstNode node, XElement elm)
        {
            var lstElm = elm.Elements();
            foreach (var subElm in lstElm)
            {
                //Verifica se é um PropertyElement
                if (subElm.Name.LocalName.StartsWith($"{node.Name}."))
                {
                    ParseElementProperty(node, subElm);
                }
                else
                {
                    var subNode = ParseElement(subElm);
                    node.Childrens.Add(subNode);
                }
            }
        }
    }
}
