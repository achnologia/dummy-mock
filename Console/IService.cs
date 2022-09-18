namespace DummyMock;

public interface IService
{
    Task<string> GetHelloWorldAsync();

    string GetHelloWorld();

    Task<int> ProcessParamAsync(int param);

    int ProcessParam(int param);

    Task DoAsync();
    
    void Do();
}