using System;
using System.Collections;
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
        GridLengthTypeConverter gridLConverter = null;

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
            ApplyAttachedProperties(type, obj, node);
            ApplyElementProperties(type, obj, node);
            ApplyChildrens(type, obj, node);

            return obj;
        }

        void ApplyChildrens(Type type, object obj, AstNode node)
        {
            if (node.Childrens.Count > 0)
            {
                var propChildren = type.GetTypeInfo().GetDeclaredProperty("Children");
                if (propChildren != null)
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
                    var prop2Children = type.GetRuntimeProperty("Children");
                    if (prop2Children != null)
                    {
                        foreach (var children in node.Childrens)
                        {
                            var subObj = BuildNode(children);
                            var layout = (IList<View>)prop2Children.GetValue(obj);
                            layout.Add((subObj as View));
                        }
                    }
                    else
                    {
                        var propContent = type.GetRuntimeProperty("Content");
                        if (propContent != null)
                        {
                            var subObj = BuildNode(node.Childrens.FirstOrDefault());
                            propContent.SetValue(obj, subObj);
                        }
                    }
                }
            }
        }

        void ApplyElementProperties(Type type, object obj, AstNode node)
        {
            foreach (var elmProp in node.ElementProperties)
            {
                var prop = type.GetRuntimeProperty(elmProp.Key);

                foreach (var item in elmProp.Value)
                {
                    var subObj = BuildNode(item);
                    if (prop.PropertyType == typeof(ColumnDefinitionCollection))
                    {
                        var lst = (ColumnDefinitionCollection)prop.GetValue(obj);
                        lst.Add((subObj as ColumnDefinition));
                    }
                    else if (prop.PropertyType == typeof(RowDefinitionCollection))
                    {
                        var lst = (RowDefinitionCollection)prop.GetValue(obj);
                        lst.Add((subObj as RowDefinition));
                    }
                }

            }
        }

        void ApplyAttachedProperties(Type type, object obj, AstNode node)
        {
            foreach (var attached in node.AttachedProperties)
            {
                if (attached.Key == "Grid.Row")
                {
                    int.TryParse(attached.Value, out int v);
                    Grid.SetRow((obj as BindableObject), v);
                    continue;
                }
                else if (attached.Key == "Grid.Column")
                {
                    int.TryParse(attached.Value, out int v);
                    Grid.SetColumn((obj as BindableObject), v);
                    continue;
                }
                else if (attached.Key == "Grid.ColumnSpan")
                {
                    int.TryParse(attached.Value, out int v);
                    Grid.SetColumnSpan((obj as BindableObject), v);
                    continue;
                }
                else if (attached.Key == "Grid.RowSpan")
                {
                    int.TryParse(attached.Value, out int v);
                    Grid.SetRowSpan((obj as BindableObject), v);
                    continue;
                }
            }
        }

        void ApplyAttributes(Type type, object obj, AstNode node)
        {

            foreach (var attr in node.AttributeProperties)
            {
                try
                {
                    var prop = type.GetRuntimeProperty(attr.Key);

                    //Verificando se a propriedade possui um TypeConverter
                    //var typeConvertAttr = prop.CustomAttributes.FirstOrDefault(f => f.AttributeType == typeof(TypeConverterAttribute));
                    //if(typeConvertAttr != null)
                    //{
                    //    var convertType = typeConvertAttr.ConstructorArguments[0].Value as Type;
                    //    var converter = (TypeConverter)Activator.CreateInstance(convertType);
                    //    var v = converter.ConvertFromInvariantString(attr.Value);
                    //    prop.SetValue(obj, v);
                    //}

                    if (prop.PropertyType == typeof(LayoutOptions))
                    {
                        if (loConverter == null)
                        {
                            loConverter = new LayoutOptionsConverter();
                        }
                        var op = (LayoutOptions)loConverter.ConvertFromInvariantString(attr.Value);
                        prop.SetValue(obj, op);
                    }
                    else if (prop.PropertyType == typeof(GridLength))
                    {
                        if (gridLConverter == null)
                        {
                            gridLConverter = new GridLengthTypeConverter();
                        }
                        var op = (GridLength)gridLConverter.ConvertFromInvariantString(attr.Value);
                        prop.SetValue(obj, op);
                    }
                    else if (prop.PropertyType == typeof(Color))
                    {
                        if (colorConverter == null)
                        {
                            colorConverter = new ColorTypeConverter();
                        }
                        var op = (Color)colorConverter.ConvertFromInvariantString(attr.Value);
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