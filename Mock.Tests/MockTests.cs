using System.Threading.Tasks;
using DummyMock;
using Xunit;

namespace Mock.Tests;

public class MockTests
{
    [Fact]
    public void Should_Mock()
    {
        // Arrange
        var newReturn = "Mocked hello world";
        var mock = new DummyMock<IService>();

        // Act
        mock.Setup(x => x.GetHelloWorld(), () => newReturn);
        var result = mock.Object.GetHelloWorld();

        // Assert
        Assert.Equal(newReturn, result);
    }
    
    [Fact]
    public async void Should_Mock_Async()
    {
        // Arrange
        var newReturn = "Mocked hello world";
        var mock = new DummyMock<IService>();

        // Act
        mock.Setup(x => x.GetHelloWorldAsync(), () => Task.FromResult(newReturn));
        var result = await mock.Object.GetHelloWorldAsync();

        // Assert
        Assert.Equal(newReturn, result);
    }
    
    [Fact]
    public void Should_Mock_WithParams()
    {
        // Arrange
        var newReturn = int.MaxValue;
        var mock = new DummyMock<IService>();

        // Act
        mock.Setup(x => x.ProcessParam(int.MinValue), () => newReturn);
        var result = mock.Object.ProcessParam(int.MinValue);

        // Assert
        Assert.Equal(newReturn, result);
    }
    
    [Fact]
    public async void Should_Mock_WithParamsAsync()
    {
        // Arrange
        var newReturn = int.MaxValue;
        var mock = new DummyMock<IService>();

        // Act
        mock.Setup(x => x.ProcessParamAsync(int.MinValue), () => Task.FromResult(newReturn));
        var result = await mock.Object.ProcessParamAsync(int.MinValue);

        // Assert
        Assert.Equal(newReturn, result);
    }
    
    [Fact]
    public void Should_Mock_Void()
    {
        // Arrange
        var mock = new DummyMock<IService>();

        // Act
        mock.Object.Do();

        // Assert
        
    }
    
    [Fact]
    public async void Should_Mock_VoidAsync()
    {
        // Arrange
        var mock = new DummyMock<IService>();

        // Act
        await mock.Object.DoAsync();

        // Assert
        
    }
    
    [Fact]
    public async void Should_Mock_Chain()
    {
        // Arrange
        var newHelloWorldReturn = "Mocked hello world";
        var newParamReturn = int.MaxValue;
        var mock = new DummyMock<IService>();

        // Act
        mock
            .Setup(x => x.GetHelloWorld(), () => newHelloWorldReturn)
            .Setup(x => x.GetHelloWorldAsync(), () => Task.FromResult(newHelloWorldReturn))
            .Setup(x => x.ProcessParam(int.MinValue), () => newParamReturn)
            .Setup(x => x.ProcessParamAsync(int.MinValue), () => Task.FromResult(newParamReturn));
        
        var helloWorldResult = mock.Object.GetHelloWorld();
        var asyncHelloWorldResult = await mock.Object.GetHelloWorldAsync();
        var paramResult = mock.Object.ProcessParam(int.MinValue);
        var asyncParamResult = await mock.Object.ProcessParamAsync(int.MinValue);

        // Assert
        Assert.Equal(newHelloWorldReturn, helloWorldResult);
        Assert.Equal(newHelloWorldReturn, asyncHelloWorldResult);
        Assert.Equal(newParamReturn, paramResult);
        Assert.Equal(newParamReturn, asyncParamResult);
    }
}