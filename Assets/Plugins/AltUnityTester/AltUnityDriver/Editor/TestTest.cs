using NUnit.Framework;

public class TestTest
{
    public AltUnityDriver AltUnityDriver;
    //Before any test it connects with the socket
    [OneTimeSetUp]
    public void SetUp()
    {
        AltUnityDriver =new AltUnityDriver();
    }

    //At the end of the test closes the connection with the socket
    [OneTimeTearDown]
    public void TearDown()
    {
        AltUnityDriver.Stop();
    }

    [Test]
    public void TestTTTTTT()
    {
        var a = AltUnityDriver.FindElement("PlayerBoard/BoardCreature(Clone)/Other/Frozen").GetAllComponents();
        foreach(var bb in a)
        {
            var b = AltUnityDriver.FindElement("PlayerBoard/BoardCreature(Clone)/Other/Frozen").GetAllProperties(bb);
            var cc = 3 + 3;
        }
        var c = AltUnityDriver.FindElement("PlayerBoard/BoardCreature(Clone)/Other/Picture_CardMechanics").GetAllComponents();

        foreach (var dd in c)
        {
            var d = AltUnityDriver.FindElement("PlayerBoard/BoardCreature(Clone)/Other/Frozen").GetAllProperties(dd);
            var tt = 3 + 3;
        }
    }

}
