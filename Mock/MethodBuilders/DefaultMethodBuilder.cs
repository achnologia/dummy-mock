using System.Reflection;
using System.Text;
using Mock.MethodBuilders.Base;

namespace Mock.MethodBuilders;

public sealed class DefaultMethodBuilder : MethodBuilder
{
    public override bool DoesMatch(string returnType) => true;

    public override StringBuilder Build(StringBuilder sb, MethodInfo methodInfo)
    {
        var returnType = methodInfo.ReturnType.ToString();
        var parametersStr = BuildParameters(methodInfo);
        
        if (methodInfo.ReturnType.IsGenericType)
        {
            returnType = returnType.Replace("`1[", "<").Replace("]", ">");
        }
        
        sb.AppendLine($"public {returnType} {methodInfo.Name}({parametersStr})");
        sb.AppendLine("{");
        sb.AppendLine($"var result = GetMockedMethodResult(\"{methodInfo.Name}\");");
        sb.AppendLine($"return ({returnType})result;");
        sb.AppendLine("}");
        sb.AppendLine("");

        return sb;
    }
}