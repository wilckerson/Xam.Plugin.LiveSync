using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace Xam.Plugin.LiveSync.XamlParser
{
    internal class XamlAstBuilder
    {
        string defaultAssemblyQualifiedName;
        string defaultConverterAssemblyQualifiedName;
        //LayoutOptionsConverter loConverter = null;
        //ThicknessTypeConverter thicknessConverter = null;
        //ColorTypeConverter colorConverter = null;
        //GridLengthTypeConverter gridLConverter = null;
        object rootPage;

        public XamlAstBuilder(object rootPage)
        {
            //Obtendo o assemblyQualifiedName padrão
            this.rootPage = rootPage;
            defaultAssemblyQualifiedName = new ContentPage().GetType().AssemblyQualifiedName;
            defaultConverterAssemblyQualifiedName = new LayoutOptionsConverter().GetType().AssemblyQualifiedName;
        }

        Type GetTypeFromName(string name, string namespaceName = null)
        {
            Type type = null;
            if (!string.IsNullOrEmpty(namespaceName) && namespaceName.Trim().StartsWith("clr-namespace"))
            {
                var parts = namespaceName.Split(';');
                var usingNamespace = parts[0].Replace("clr-namespace:", string.Empty).Trim();
                var assembly = parts.ElementAtOrDefault(1)?.Replace("assembly=", string.Empty).Trim();

                if (string.IsNullOrEmpty(assembly))
                {
                    assembly = usingNamespace.Split('.').FirstOrDefault();
                }

                var assemblyQualifiedName = $"{usingNamespace}.{name}, {assembly}";

                type = Type.GetType(assemblyQualifiedName);
            }
            else
            {
                var assemblyQualifiedName = defaultAssemblyQualifiedName.Replace("ContentPage", name);
                type = Type.GetType(assemblyQualifiedName);
            }

            if (type == null)
            {
                var n = namespaceName ?? "System";
                var assemblyQualifiedName = $"{n}.{name}";
                type = Type.GetType(assemblyQualifiedName);
            }

            return type;
        }

        public object BuildNode(AstNode node, Type type = null)
        {
            if (node == null) return null;

            if (type == null)
            {
                type = GetTypeFromName(node.Name, node.Namespace);
            }

            if (type == null)
            {
                Application.Current.MainPage.DisplayAlert("Livesync", $"Error on namespace declaration for {node.Name}", "Ok");
                return null;
            }

            if (type == typeof(DataTemplate))
            {
                var dataTemplate = new DataTemplate(() =>
                {

                    var subObj = BuildNode(node.Childrens.FirstOrDefault());
                    return subObj;
                });
                return dataTemplate;
            }
            else if (type == typeof(ViewCell))
            {
                var subObj = BuildNode(node.Childrens.FirstOrDefault());
                return new ViewCell() { View = (subObj as View) };
            }

            var obj = Activator.CreateInstance(type);
            ApplyAttributes(type, obj, node);
            ApplyAttachedProperties(type, obj, node);
            ApplyElementProperties(type, obj, node);
            ApplyChildrens(type, obj, node.Childrens);

            return obj;
        }

        AstNode GetNodeToBuild(AstNode node)
        {
            AstNode nodeToBuild = node;
            if (node.Name == "OnIdiom")
            {
                var v = ExtractValueFromOnIdiom(node);
                if (v != null && v is AstNode)
                {
                    nodeToBuild = (AstNode)v;
                }
            }
            else if (node.Name == "OnPlatform")
            {
                var v = ExtractValueFromOnPlatform(node);
                if (v != null && v is AstNode)
                {
                    nodeToBuild = (AstNode)v;
                }
            }
            return nodeToBuild;
        }

        void ApplyChildrens(Type type, object obj, List<AstNode> childrens)
        {
            if (childrens.Count > 0)
            {

                var propChildren = type.GetTypeInfo().GetDeclaredProperty("Children");
                if (propChildren != null)
                {
                    foreach (var children in childrens)
                    {
                        var nodeToBuild = GetNodeToBuild(children);
                        var subObj = BuildNode(nodeToBuild);
                        if (subObj == null) { continue; }
                        var layout = (IList<View>)propChildren.GetValue(obj);
                        layout.Add((subObj as View));
                    }
                }
                else
                {
                    var prop2Children = type.GetRuntimeProperties().FirstOrDefault(f => f.Name == "Children");
                    //var prop2Children = type.GetRuntimeProperty("Children");
                    if (prop2Children != null)
                    {
                        foreach (var children in childrens)
                        {
                            var nodeToBuild = GetNodeToBuild(children);
                            var subObj = BuildNode(nodeToBuild);
                            if (subObj == null) { continue; }

                            var layout = (IList<View>)prop2Children.GetValue(obj);
                            layout.Add((subObj as View));

                        }
                    }
                    else
                    {
                        var propContent = type.GetRuntimeProperty("Content");
                        if (propContent != null)
                        {
                            AstNode children = childrens.FirstOrDefault();

                            var nodeToBuild = GetNodeToBuild(children);
                            var subObj = BuildNode(nodeToBuild);
                            if (subObj != null)
                            {
                                propContent.SetValue(obj, subObj);
                            }
                        }
                    }
                }
            }
        }
        object ExtractValueFromOnIdiom(AstNode onPlatformNode)
        {
            var currentIdiom = Device.Idiom.ToString();
            onPlatformNode.AttributeProperties.TryGetValue("TypeArguments", out string typeDescription);

            string valuePlatformDescription = null;
            if (onPlatformNode.ElementProperties.Any())
            {
                var elmProp = onPlatformNode.ElementProperties.FirstOrDefault(f =>f.Key == currentIdiom);
                return elmProp.Value.FirstOrDefault();
            }
            else
            {
                onPlatformNode.AttributeProperties.TryGetValue(currentIdiom, out valuePlatformDescription);
            }

            if (string.IsNullOrEmpty(typeDescription) || string.IsNullOrEmpty(valuePlatformDescription))
            {
                return null;
            }

            var idx = typeDescription.IndexOf(':');
            if (idx != -1)
            {
                typeDescription = typeDescription.Substring(idx + 1);
            }

            var type = GetTypeFromName(typeDescription);
            object value = ResolveValueForPropertyType(type, valuePlatformDescription);

            return value;
        }

        object ExtractValueFromOnPlatform(AstNode onPlatformNode)
        {
            string currentPlatform = Device.RuntimePlatform;
            onPlatformNode.AttributeProperties.TryGetValue("TypeArguments", out string typeDescription);

            string valuePlatformDescription = null;
            if (onPlatformNode.Childrens.Any())
            {
                var child = onPlatformNode.Childrens.FirstOrDefault(f =>
                    f.AttributeProperties.ContainsKey("Platform")
                    && f.AttributeProperties["Platform"].Contains(currentPlatform));

                if (child != null)
                {
                    if (child.AttributeProperties.ContainsKey("Value"))
                    {
                        valuePlatformDescription = child.AttributeProperties["Value"];
                    }
                    else if (child.Childrens.Any())
                    {
                        return child.Childrens.FirstOrDefault();
                    }
                    else
                    {
                        valuePlatformDescription = child.TextContent;
                    }
                }
            }
            else
            {
                onPlatformNode.AttributeProperties.TryGetValue(currentPlatform, out valuePlatformDescription);
            }


            if (string.IsNullOrEmpty(typeDescription) || string.IsNullOrEmpty(valuePlatformDescription))
            {
                return null;
            }

            var idx = typeDescription.IndexOf(':');
            if (idx != -1)
            {
                typeDescription = typeDescription.Substring(idx + 1);
            }

            var type = GetTypeFromName(typeDescription);
            object value = ResolveValueForPropertyType(type, valuePlatformDescription);

            return value;
        }

        void ApplyElementProperties(Type type, object obj, AstNode node)
        {
            foreach (var elmProp in node.ElementProperties)
            {
                if (elmProp.Key == "Content" || elmProp.Key == "Children")
                {
                    ApplyChildrens(type, obj, elmProp.Value);
                    continue;
                }

                var prop = type.GetRuntimeProperty(elmProp.Key);
                var propTypeInfo = prop.PropertyType.GetTypeInfo();

                foreach (var item in elmProp.Value)
                {
                    object subObj = null;

                    if (item.Name == "OnPlatform")
                    {
                        subObj = ExtractValueFromOnPlatform(item);
                    }
                    else if (item.Name == "OnIdiom")
                    {
                        subObj = ExtractValueFromOnIdiom(item);
                    }
                    else
                    {
                        subObj = BuildNode(item);
                    }

                    if (subObj == null) { continue; }

                    if (propTypeInfo.IsGenericType && propTypeInfo.GetGenericTypeDefinition() == typeof(IList<>))
                    {
                        var lst = (IList)prop.GetValue(obj);
                        lst.Add(subObj);
                    }
                    else if (prop.PropertyType == typeof(ColumnDefinitionCollection))
                    {
                        var lst = (ColumnDefinitionCollection)prop.GetValue(obj);
                        lst.Add((subObj as ColumnDefinition));
                    }
                    else if (prop.PropertyType == typeof(RowDefinitionCollection))
                    {
                        var lst = (RowDefinitionCollection)prop.GetValue(obj);
                        lst.Add((subObj as RowDefinition));
                    }
                    else
                    {
                        try
                        {
                            prop.SetValue(obj, subObj);
                        }
                        catch (Exception ex)
                        {
                            Application.Current.MainPage.DisplayAlert("Livesync", $"Invalid value on property {prop.Name}. {ex.Message}", "Ok");
                        }
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

        Type TryGetTypeConverter(string propName)
        {
            if (string.IsNullOrEmpty(propName)) { return null; }

            string typeConverterName = $"{propName}TypeConverter";
            var assemblyQualifiedName = defaultConverterAssemblyQualifiedName.Replace("LayoutOptionsConverter", typeConverterName);
            var converterType = Type.GetType(assemblyQualifiedName);
            if (converterType == null)
            {
                string typeConverterName2 = $"{propName}Converter";
                var assemblyQualifiedName2 = defaultConverterAssemblyQualifiedName.Replace("LayoutOptionsConverter", typeConverterName2);
                converterType = Type.GetType(assemblyQualifiedName2);
            }
            return converterType;
        }

        object ResolveValueForPropertyType(Type type, string value)
        {
            //Enum
            if (type.GetTypeInfo().IsEnum)
            {
                var r = Enum.Parse(type, value);
                return r;
            }

            //Propriedades com TypeConverter (Tipo + TypeConverter || Tipo + Converter)
            if (type.GetTypeInfo().IsSubclassOf(typeof(TypeConverter)))
            {
                var typeConverter = (TypeConverter)Activator.CreateInstance(type);
                var v = typeConverter.ConvertFromInvariantString(value);
                return v;
            }
            else if (type.Namespace != "System") //Tipos primitivos não possuem Converter
            {
                var converterType = TryGetTypeConverter(type.Name);

                if (converterType != null)
                {
                    var typeConverter = (TypeConverter)Activator.CreateInstance(converterType);
                    var v = typeConverter.ConvertFromInvariantString(value);
                    return v;
                }
            }

            //TimeSpan
            if (type == typeof(TimeSpan))
            {
                var d = double.Parse(value);
                var tspan = TimeSpan.FromMilliseconds(d);
                return tspan;
            }

            //Nulabbles
            Type t = Nullable.GetUnderlyingType(type) ?? type;

            //Outros
            var resolvedValue = Convert.ChangeType(value, t);
            return resolvedValue;
        }

        void ApplyAttributes(Type type, object obj, AstNode node)
        {

            foreach (var attr in node.AttributeProperties)
            {
                try
                {
                    //Eventos
                    var ev = type.GetRuntimeEvent(attr.Key);
                    if (ev != null && rootPage != null)
                    {
                        var evMethod = rootPage.GetType().GetRuntimeMethods().FirstOrDefault(f => f.Name == attr.Value);
                        if (evMethod != null)
                        {
                            var del = evMethod.CreateDelegate(ev.EventHandlerType, rootPage);
                            ev.AddEventHandler(obj, del);
                        }
                        continue;
                    }

                    //Propriedades
                    var prop = type.GetRuntimeProperty(attr.Key);
                    if (prop == null) { continue; }
                    //var propTypeinfo = prop.PropertyType.GetTypeInfo();

                    //Propriedades com Binding
                    if (attr.Value.StartsWith("{") && attr.Value.Contains("Binding") && obj is BindableObject)
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
                        var fieldInfo = TypeHelper.GetField(type, attr.Key + "Property");
                        if (fieldInfo == null) { continue; }

                        BindableProperty bindProp = (BindableProperty)fieldInfo.GetValue(null);
                        if (obj is Page)
                        {
                            (rootPage as BindableObject).SetBinding(bindProp, binding);
                        }
                        else
                        {
                            (obj as BindableObject).SetBinding(bindProp, binding);
                        }

                        continue;
                    }

                    //StaticResource e DynamicResource
                    if (attr.Value.StartsWith("{")
                        && (attr.Value.Contains("StaticResource") || attr.Value.Contains("DynamicResource")))
                    {
                        var resourcePath = attr.Value.Replace("{", string.Empty)
                            .Replace("}", string.Empty)
                            .Replace("StaticResource", string.Empty)
                            .Replace("DynamicResource", string.Empty)
                            .Trim();

                        //Busca o estilo na pagina
                        var resDic = (rootPage as VisualElement).Resources;
                        if (resDic != null && resDic.TryGetValue(resourcePath, out object pageResourceValue))
                        {
                            prop.SetValue(obj, pageResourceValue);
                            continue;
                        }
                        //Caso contrário busca o estilo global
                        else if (Application.Current.Resources != null && Application.Current.Resources.TryGetValue(resourcePath, out object resourceValue))
                        {
                            var onIdiomType = resourceValue.GetType();

                            if (onIdiomType.IsConstructedGenericType && onIdiomType.GetGenericTypeDefinition() == typeof(Xamarin.Forms.OnIdiom<>))
                            {
                                object idiomValue = null;

                                switch (Device.Idiom)
                                {

                                    case TargetIdiom.Phone:

                                        var propInf = onIdiomType.GetRuntimeProperty("Phone");
                                        idiomValue = propInf.GetValue(resourceValue);
                                        break;

                                    case TargetIdiom.Desktop:
                                        var propInf2 = onIdiomType.GetRuntimeProperty("Desktop");
                                        idiomValue = propInf2.GetValue(resourceValue);
                                        break;

                                    case TargetIdiom.Tablet:
                                        var propInf3 = onIdiomType.GetRuntimeProperty("Tablet");
                                        idiomValue = propInf3.GetValue(resourceValue);
                                        break;

                                    case TargetIdiom.Unsupported:
                                    default:
                                        break;
                                }

                                prop.SetValue(obj, idiomValue);
                                continue;
                            }
                        }
                    }

                    //TypeConverterAttribute
                    var typeConvertAttr = prop.CustomAttributes.FirstOrDefault(f => f.AttributeType == typeof(TypeConverterAttribute));
                    if (typeConvertAttr != null)
                    {
                        var convertType = typeConvertAttr.ConstructorArguments[0].Value as Type;
                        var converter = (TypeConverter)Activator.CreateInstance(convertType);
                        var v = converter.ConvertFromInvariantString(attr.Value);
                        prop.SetValue(obj, v);
                        continue;
                    }

                    var resolvedValue = ResolveValueForPropertyType(prop.PropertyType, attr.Value);

                    prop.SetValue(obj, resolvedValue);

                }
                catch (Exception ex)
                {
                    Application.Current.MainPage.DisplayAlert("Livesync", $"Invalid value on property {attr.Key}. {ex.Message}", "Ok");
                }
            }
        }
    }
}