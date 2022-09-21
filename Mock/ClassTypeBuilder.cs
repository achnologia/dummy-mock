using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mock.MethodBuilders;

namespace Mock;

public class ClassTypeBuilder
{
    private readonly string _classTemplate = @"
    using System;
    using System.Collections.Generic;
    {3}

    namespace {0}
    {{
        public class {1} : {2}
        {{
            private readonly Dictionary<string, Func<object>> _setups;

            public {1}(Dictionary<string, Func<object>> setups)
            {{
                _setups = setups;
            }}

            private object GetMockedMethodResult(string method)
            {{
                if(!_setups.ContainsKey(method))
                    throw new Exception();

                var setup = _setups[method];
                var result = setup.Invoke();

                return result;
            }}
            
            {4}
        }}
    }}
".Trim();

    public Type Build<T>()
    {
        var destinationType = typeof(T);

        var newClassName = $"{destinationType.Name}Mock";

        var dynamicUsingStr = BuildUsingStr(destinationType);
        var methodsStr = BuildMethodsStr(destinationType);
        
        var classSource = string.Format(_classTemplate, destinationType.Namespace, newClassName, destinationType.FullName, dynamicUsingStr, methodsStr);

        var compilation = BuildCompilation(destinationType, classSource);

        var type = BuildType(compilation, destinationType.Namespace, newClassName);

        return type;
    }

    private string BuildUsingStr(Type type)
    {
        var referenceTypes = GetMethods(type).Select(x => x.ReturnType)
            .Concat(GetMethods(type).SelectMany(xx => xx.GetParameters().Select(p => p.ParameterType)));

        var dynamicUsing = referenceTypes.Select(x => $"using {x.Namespace};").Distinct();
        var dynamicUsingStr = string.Join(Environment.NewLine, dynamicUsing);

        return dynamicUsingStr;
    }
    
    private string BuildMethodsStr(Type type)
    {
        var sb = new StringBuilder();
        var methods = GetMethods(type);

        var mbEngine = new MethodBuilderEngine.Builder()
            .WithVoidMethodBuilder()
            .WithTaskMethodBuilder()
            .Build();

        foreach (var method in methods)
        {
            mbEngine.BuildMethod(sb, method);
        }
        
        return sb.ToString();
    }

    private IEnumerable<MethodInfo> GetMethods(Type type)
    {
        return type.GetMethods().Where(x => x.IsAbstract || x.IsVirtual);
    }
    
    private CSharpCompilation BuildCompilation(Type type, string classSource)
    {
        var assemblies = BuildReferences(type);

        var syntaxTree = CSharpSyntaxTree.ParseText(classSource);
        
        var compilation = CSharpCompilation
            .Create(type.Namespace)
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(assemblies)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

        return compilation;
    }
    
    private IEnumerable<MetadataReference> BuildReferences(Type type)
    {
        var assemblies = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(type.Assembly.Location)
        };

        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        
        assemblies.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")));
        assemblies.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")));
        assemblies.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")));
        assemblies.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));
        assemblies.Add(MetadataReference.CreateFromFile(Assembly.GetEntryAssembly().Location));

        return assemblies;
    }

    private Type BuildType(CSharpCompilation compilation, string newNamespace, string className)
    {
        using var ms = new MemoryStream();

        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            throw new Exception("Some errors occurred during type build");
        }
        
        ms.Seek(0, SeekOrigin.Begin);
        Assembly assembly = Assembly.Load(ms.ToArray());

        var newTypeFullName = $"{newNamespace}.{className}";

        var type = assembly.GetType(newTypeFullName);
        
        return type;
    }
}