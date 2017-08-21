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
        string defaultConverterAssemblyQualifiedName;
        LayoutOptionsConverter loConverter = null;
        ThicknessTypeConverter thicknessConverter = null;
        ColorTypeConverter colorConverter = null;
        GridLengthTypeConverter gridLConverter = null;
        object rootPage;

        public XamlAstBuilder(object rootPage)
        {
            //Obtendo o assemblyQualifiedName padrão
            this.rootPage = rootPage;
            defaultAssemblyQualifiedName = new ContentPage().GetType().AssemblyQualifiedName;
            defaultConverterAssemblyQualifiedName = new LayoutOptionsConverter().GetType().AssemblyQualifiedName;
        }

        public object BuildNode(AstNode node)
        {
            //TODO: node.Namespace
            if (node == null) return null;

            Type type = null;
            if (!string.IsNullOrEmpty(node.Namespace) && node.Namespace.Trim().StartsWith("clr-namespace"))
            {
                var parts = node.Namespace.Split(';');
                var usingNamespace = parts[0].Replace("clr-namespace:", string.Empty).Trim();
                var assembly = parts.ElementAtOrDefault(1)?.Replace("assembly=", string.Empty).Trim();

                if (string.IsNullOrEmpty(assembly))
                {
                    assembly = usingNamespace;
                }

                var assemblyQualifiedName = $"{usingNamespace}.{node.Name}, {assembly}";

                type = Type.GetType(assemblyQualifiedName);
            }
            else
            {
                var assemblyQualifiedName = defaultAssemblyQualifiedName.Replace("ContentPage", node.Name);
                type = Type.GetType(assemblyQualifiedName);
            }

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
                            if (subObj != null)
                            {
                                var layout = (IList<View>)prop2Children.GetValue(obj);
                                layout.Add((subObj as View));
                            }
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
                    //Eventos
                    var ev = type.GetRuntimeEvent(attr.Key);
                    if(ev != null && rootPage != null)
                    {
                        var evMethod = rootPage.GetType().GetRuntimeMethods().FirstOrDefault(f => f.Name == attr.Value);
                        if(evMethod != null)
                        {
                            var del = evMethod.CreateDelegate(ev.EventHandlerType,rootPage);
                            ev.AddEventHandler(obj, del);
                        }
                        continue;
                    }

                    //Propriedades
                    var prop = type.GetRuntimeProperty(attr.Key);
                    if(prop == null) { continue; }
                    var propTypeinfo = prop.PropertyType.GetTypeInfo();
                   
                    //Propriedades com Binding
                    if (attr.Value.StartsWith("{") && attr.Value.Contains("Binding"))
                    {
                        var bindingPath = attr.Value;                           

                        string format = null;
                        if (bindingPath.ToLower().Contains("stringformat"))
                        {
                            var parts = bindingPath.Split(',');
                            bindingPath = parts[0].Trim();

                            format = parts[1].Trim().Substring(14);
                            format = format.Substring(0, format.Length - 2);
                        }

                        bindingPath = bindingPath.Replace("{", string.Empty)
                            .Replace("}", string.Empty)
                            .Replace("Binding", string.Empty)
                            .Trim();

                        var binding = new Binding(bindingPath, stringFormat: format);
                        var fieldInfo = type.GetRuntimeField(attr.Key + "Property");


                        BindableProperty bindProp = (BindableProperty)fieldInfo.GetValue(null);
                        (obj as BindableObject).SetBinding(bindProp, binding);

                        continue;
                    }
                   
                    //Enum
                    if (propTypeinfo.IsEnum)
                    {
                        var r = Enum.Parse(prop.PropertyType, attr.Value);
                        prop.SetValue(obj, r);
                        continue;
                    }
                    
                    //Propriedades com TypeConverter (Tipo + TypeConverter || Tipo + Converter)
                    if (prop.PropertyType.Namespace != "System") //Tipos primitivos não possuem Converter
                    {
                        string typeConverterName = $"{prop.PropertyType.Name}TypeConverter";
                        var assemblyQualifiedName = defaultConverterAssemblyQualifiedName.Replace("LayoutOptionsConverter", typeConverterName);
                        var converterType = Type.GetType(assemblyQualifiedName);
                        if (converterType == null)
                        {
                            string typeConverterName2 = $"{prop.PropertyType.Name}Converter";
                            var assemblyQualifiedName2 = defaultConverterAssemblyQualifiedName.Replace("LayoutOptionsConverter", typeConverterName2);
                            converterType = Type.GetType(assemblyQualifiedName2);
                        }

                        if (converterType != null)
                        {
                            var typeConverter = (TypeConverter)Activator.CreateInstance(converterType);
                            var v = typeConverter.ConvertFromInvariantString(attr.Value);
                            prop.SetValue(obj, v);
                            continue;
                            //var method = converterType.GetTypeInfo().GetDeclaredMethod("ConvertFromInvariantString");
                            //if (method != null)
                            //{
                            //    var r = method.Invoke(typeConverter, new object[] { attr.Value });
                            //    prop.SetValue(obj, r);
                            //    continue;
                            //}
                        }
                    }

                    //TypeConverterAttribute
                    var typeConvertAttr = prop.CustomAttributes.FirstOrDefault(f => f.AttributeType == typeof(TypeConverterAttribute));
                    if(typeConvertAttr != null)
                    {
                        var convertType = typeConvertAttr.ConstructorArguments[0].Value as Type;
                        var converter = (TypeConverter)Activator.CreateInstance(convertType);
                        var v = converter.ConvertFromInvariantString(attr.Value);
                        prop.SetValue(obj, v);
                        continue;
                    }                    

                    //Outros
                    var convertedValue = Convert.ChangeType(attr.Value, prop.PropertyType);
                        prop.SetValue(obj, convertedValue);
                    
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}