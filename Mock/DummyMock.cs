using System.Linq.Expressions;

namespace Mock;

public class DummyMock<T> where T : class
{
    private T? _instance;

    private readonly Dictionary<string, Func<object>> _setups = new();

    private readonly ClassTypeBuilder _typeBuilder = new();
    
    public T Object
    {
        get
        {
            _instance ??= CreateInstance();

            return _instance;
        }
    }

    private T CreateInstance()
    {
        var classType = _typeBuilder.Build<T>();
        
        return (T)Activator.CreateInstance(classType, _setups)!;
    }

    public DummyMock<T> Setup<TResult>(Expression<Func<T, TResult>> exp, Func<TResult> resultBuilder)
    {
        var methodCallExpression = (MethodCallExpression)exp.Body;
        var methodName = methodCallExpression.Method.Name;

        _setups[methodName] = () => resultBuilder();
        
        return this;
    }
}