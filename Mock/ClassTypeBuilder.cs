using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

    public Type Build(Type destinationType)
    {
        if (destinationType is null)
            throw new ArgumentException("Destination type is missing", nameof(destinationType));

        var newClassName = $"{destinationType.Name}Mock";

        var referenceTypes = GetMethods(destinationType).Select(x => x.ReturnType)
            .Concat(GetMethods(destinationType).SelectMany(xx => xx.GetParameters().Select(p => p.ParameterType))).ToList();

        var dynamicUsing = referenceTypes.Select(x => $"using {x.Namespace};").Distinct();
        var dynamicUsingStr = string.Join(Environment.NewLine, dynamicUsing);

        var methodsStr = BuildMethodsStr(destinationType);
        
        var classSource = string.Format(_classTemplate, destinationType.Namespace, newClassName, destinationType.FullName, dynamicUsingStr, methodsStr);

        var assemblies = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(destinationType.Assembly.Location)
        };

        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        
        assemblies.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")));
        assemblies.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")));
        assemblies.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")));
        assemblies.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));
        assemblies.Add(MetadataReference.CreateFromFile(Assembly.GetEntryAssembly().Location));

        var syntaxTree = CSharpSyntaxTree.ParseText(classSource);
        
        var compilation = CSharpCompilation
            .Create(destinationType.Namespace)
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(assemblies)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

        var type = BuildType(compilation, destinationType.Namespace, newClassName);

        return type;
    }

    private string BuildMethodsStr(Type type)
    {
        var sb = new StringBuilder();
        var methods = GetMethods(type);

        foreach (var method in methods)
        {
            var returnType = method.ReturnType.ToString();

            if (returnType == "System.Void")
            {
                returnType = "void";
            }
            
            if (method.ReturnType.IsGenericType)
            {
                returnType = returnType.Replace("`1[", "<").Replace("]", ">");
            }

            var parametersStr = string.Empty;
            var parameters = method.GetParameters();
            
            if (parameters.Any())
            {
                parametersStr = string.Join(',', parameters.Select(x => $"{x.ParameterType} {x.Name}"));
            }
            
            sb.AppendLine($"public {returnType} {method.Name}({parametersStr})");
            sb.AppendLine("{");
            if (returnType != "void" && returnType != "System.Threading.Tasks.Task")
            {
                sb.AppendLine($"var result = GetMockedMethodResult(\"{method.Name}\");");
                sb.AppendLine($"return ({returnType})result;");
            }

            if (returnType == "System.Threading.Tasks.Task")
            {
                sb.AppendLine($"return System.Threading.Tasks.Task.CompletedTask;");
            }
            sb.AppendLine("}");
            sb.AppendLine("");
        }
        
        return sb.ToString();
    }
    
    private IEnumerable<MethodInfo> GetMethods(Type type)
    {
        return type.GetMethods().Where(x => x.IsAbstract || x.IsVirtual);
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