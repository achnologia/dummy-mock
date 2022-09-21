using System.Reflection;
using System.Text;
using Mock.MethodBuilders.Base;

namespace Mock.MethodBuilders;

public sealed class VoidMethodBuilder : MethodBuilder
{
    public override bool DoesMatch(string returnType) => returnType == "System.Void";

    public override StringBuilder Build(StringBuilder sb, MethodInfo methodInfo)
    {
        var parametersStr = BuildParameters(methodInfo);

        sb.AppendLine($"public void {methodInfo.Name}({parametersStr})");
        sb.AppendLine("{");
        sb.AppendLine("}");
        sb.AppendLine("");

        return sb;
    }
}