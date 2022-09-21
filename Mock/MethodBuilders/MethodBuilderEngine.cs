using System.Reflection;
using System.Text;
using Mock.MethodBuilders.Base;

namespace Mock.MethodBuilders;

public class MethodBuilderEngine
{
    private readonly IEnumerable<MethodBuilder> _builders;

    private MethodBuilderEngine(IEnumerable<MethodBuilder> builders)
    {
        _builders = builders;
    }

    public StringBuilder BuildMethod(StringBuilder sb, MethodInfo methodInfo)
    {
        foreach (var builder in _builders)
        {
            if (builder.DoesMatch(methodInfo.ReturnType.ToString()))
            {
                builder.Build(sb, methodInfo);

                break;
            }
        }

        return sb;
    }

    public class Builder
    {
        private readonly List<MethodBuilder> _builders = new();

        public Builder WithVoidMethodBuilder()
        {
            _builders.Add(new VoidMethodBuilder());

            return this;
        }
        
        public Builder WithTaskMethodBuilder()
        {
            _builders.Add(new TaskMethodBuilder());

            return this;
        }
        
        public MethodBuilderEngine Build()
        {
            _builders.Add(new DefaultMethodBuilder());

            var engine = new MethodBuilderEngine(_builders);

            return engine;
        }
    }
}