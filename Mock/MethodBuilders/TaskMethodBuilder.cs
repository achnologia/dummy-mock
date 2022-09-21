using System.Reflection;
using System.Text;
using Mock.MethodBuilders.Base;

namespace Mock.MethodBuilders;

public sealed class TaskMethodBuilder : MethodBuilder
{
    public override bool DoesMatch(string returnType) => returnType == "System.Threading.Tasks.Task";

    public override StringBuilder Build(StringBuilder sb, MethodInfo methodInfo)
    {
        var parametersStr = BuildParameters(methodInfo);

        sb.AppendLine($"public System.Threading.Tasks.Task {methodInfo.Name}({parametersStr})");
        sb.AppendLine("{");
        sb.AppendLine($"return System.Threading.Tasks.Task.CompletedTask;");
        sb.AppendLine("}");
        sb.AppendLine("");

        return sb;
    }
}