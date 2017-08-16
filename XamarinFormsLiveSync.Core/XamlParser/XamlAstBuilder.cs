using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace XamarinFormsLiveSync.Core.XamlParser
{
    internal class XamlAstBuilder
    {
        string defaultAssemblyQualifiedName;
        LayoutOptionsConverter loConverter = null;
        ThicknessTypeConverter thicknessConverter = null;
        ColorTypeConverter colorConverter = null;

        public XamlAstBuilder()
        {
            //Obtendo o assemblyQualifiedName padrão
            defaultAssemblyQualifiedName = new ContentPage().GetType().AssemblyQualifiedName;
        }
        
        public object BuildNode(AstNode node)
        {
            //TODO: node.Namespace
            if (node == null) return null;

            var assemblyQualifiedName = defaultAssemblyQualifiedName.Replace("ContentPage", node.Name);
            var type = Type.GetType(assemblyQualifiedName);

            if (type == null) return null;

            var obj = Activator.CreateInstance(type);
            ApplyAttributes(type, obj, node);
            ApplyChildrens(type, obj, node);

            return obj;
        }

        void ApplyChildrens(Type type, object obj, AstNode node)
        {
            if (node.Childrens.Count > 0)
            {
                var propChildren = type.GetRuntimeProperty("Children");
                if(propChildren != null)
                {
                    foreach (var children in node.Childrens)
                    {
                        var subObj = BuildNode(children);
                        var layout = (IList<View>)propChildren.GetValue(obj);
                        layout.Add((subObj as View));
                    }
                }
                else
                {
                    var propContent = type.GetRuntimeProperty("Content");
                    if(propContent != null)
                    {
                        var subObj = BuildNode(node.Childrens.FirstOrDefault());
                        propContent.SetValue(obj, subObj);
                    }
                }
                //var contentType = typeof(ContentPropertyAttribute);
                //var contentAttr = type.GetTypeInfo().CustomAttributes.FirstOrDefault(f => f.AttributeType == contentType);
                //string contentPropName = contentAttr?.ConstructorArguments.ElementAtOrDefault(0).Value.ToString();

                //if (!string.IsNullOrEmpty(contentPropName))
                //{
                //    var prop = type.GetRuntimeProperty(contentPropName);

                //    if (prop.PropertyType == typeof(View))
                //    {
                //        var subObj = BuildNode(node.Childrens.FirstOrDefault());
                //        prop.SetValue(obj, subObj);
                //    }
                //    else if (prop.PropertyType == typeof(IList<View>))
                //    {
                //        foreach (var children in node.Childrens)
                //        {
                //            var subObj = BuildNode(children);
                //            var layout = (IList<View>)prop.GetValue(obj);
                //            layout.Add((subObj as View));
                //        }
                //    }
                //    else if (prop.PropertyType == typeof(string))
                //    {
                //        //TODO: Label.Text
                //        //var subObj = BuildNode(node.Childrens.FirstOrDefault());
                //        //prop.SetValue(obj, subObj);
                //    }


                //}
            }
        }

        void ApplyAttributes(Type type, object obj, AstNode node)
        {
            foreach (var attr in node.AttributeProperties)
            {
                try
                {
                    var prop = type.GetRuntimeProperty(attr.Key);

                    if (prop.PropertyType == typeof(LayoutOptions))
                    {
                        if (loConverter == null)
                        {
                            loConverter = new LayoutOptionsConverter();
                        }
                        var op = (LayoutOptions)loConverter.ConvertFromInvariantString(attr.Value);
                        prop.SetValue(obj, op);
                    }
                    else if (prop.PropertyType == typeof(Thickness))
                    {
                        if (attr.Value.Contains(","))
                        {
                            if (thicknessConverter == null)
                            {
                                thicknessConverter = new ThicknessTypeConverter();
                            }
                            var thickness = (LayoutOptions)thicknessConverter.ConvertFromInvariantString(attr.Value.Trim());
                            prop.SetValue(obj, thickness);
                        }
                        else
                        {
                            var uniformSize = Convert.ToDouble(attr.Value);
                            var thickness = new Thickness(uniformSize);
                            prop.SetValue(obj, thickness);
                        }
                    }
                    else
                    {
                        var convertedValue = Convert.ChangeType(attr.Value, prop.PropertyType);
                        prop.SetValue(obj, convertedValue);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}