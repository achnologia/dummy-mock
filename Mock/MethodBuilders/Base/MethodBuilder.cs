using System.Reflection;
using System.Text;

namespace Mock.MethodBuilders.Base;

public abstract class MethodBuilder
{
    public abstract bool DoesMatch(string returnType);
    public abstract StringBuilder Build(StringBuilder sb, MethodInfo methodInfo);

    protected string BuildParameters(MethodInfo methodInfo)
    {
        var parametersStr = string.Empty;
        var parameters = methodInfo.GetParameters();
            
        if (parameters.Any())
        {
            parametersStr = string.Join(',', parameters.Select(x => $"{x.ParameterType} {x.Name}"));
        }

        return parametersStr;
    }
}