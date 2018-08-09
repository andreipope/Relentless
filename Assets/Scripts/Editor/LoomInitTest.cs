using NUnit.Framework;

public class LoomUnitTest 
{
    [Test]
    public async void TestLoomInit()
    {
        Assert.IsNull(LoomManager.Instance.Contract);
        await LoomManager.Instance.CreateContract(() => { });
        Assert.IsNotNull(LoomManager.Instance.Contract);
    }

    
    [Test]
    public async void TestLoomInit_Empty_Writer_Link()
    {
        LoomManager.Instance.WriteHost = string.Empty;
        await LoomManager.Instance.CreateContract(() => { });
        
        Assert.IsNotNull(LoomManager.Instance.Contract);
        
    }
    
    [Test]
    public async void TestLoomInit_Wrong_Writer_Link()
    {
        LoomManager.Instance.WriteHost = "https://www.google.com";
        await LoomManager.Instance.CreateContract(() => { });
        
        Assert.IsNotNull(LoomManager.Instance.Contract);
    }
    
    [Test]
    public async void TestLoomInit_Empty_Reader_Link()
    {
        LoomManager.Instance.ReaderHost = string.Empty;
        await LoomManager.Instance.CreateContract(() => { });
        
        Assert.IsNotNull(LoomManager.Instance.Contract);
        
    }
    
    [Test]
    public async void TestLoomInit_Wrong_Reader_Link()
    {
        LoomManager.Instance.ReaderHost = "https://www.google.com";
        await LoomManager.Instance.CreateContract(() => { });
        
        Assert.IsNotNull(LoomManager.Instance.Contract);
    }
}





/*
var ex = Assert.Throws<NullReferenceException>(async () => await LoomManager.Instance.Init(() => { }));
Debug.Log(ex.Message);
Assert.IsNull(ex);
*/