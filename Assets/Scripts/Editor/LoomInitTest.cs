using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class LoomUnitTest
{
    [SetUp]
    public void Init()
    {
        LoomTestContext.TestSetUp();
    }
    
    [TearDown]
    public void TearDown()
    {    
        LoomTestContext.TestTearDown();
    }
    
    [UnityTest]
    public IEnumerator TestLoomInit()
    {
        return LoomTestContext.AsyncTest(async () =>
        {
            Assert.IsNull(LoomTestContext.LoomManager.Contract);
            await LoomTestContext.LoomManager.CreateContract(LoomTestContext.LoomManager.UserDataModel.PrivateKey);
            Assert.IsNotNull(LoomTestContext.LoomManager.Contract);
        });
    }

    [UnityTest]
    public IEnumerator TestLoomInit_Empty_Writer_Link()
    {
        return LoomTestContext.AsyncTest(async () =>
        {
            LoomTestContext.LoomManager.WriteHost = string.Empty;
            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.LoomManager.CreateContract(LoomTestContext.LoomManager.UserDataModel.PrivateKey);
            });
        });
    }

    [UnityTest]
    public IEnumerator TestLoomInit_Wrong_Writer_Link()
    {
        return LoomTestContext.AsyncTest(async () =>
        {
            LoomTestContext.LoomManager.WriteHost = "https://www.google.com";
            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.LoomManager.CreateContract(LoomTestContext.LoomManager.UserDataModel.PrivateKey);
            });
        });
    }

    [UnityTest]
    public IEnumerator TestLoomInit_Empty_Reader_Link()
    {
        return LoomTestContext.AsyncTest(async () =>
        {
            LoomTestContext.LoomManager.ReaderHost = string.Empty;
            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.LoomManager.CreateContract(LoomTestContext.LoomManager.UserDataModel.PrivateKey);
            });
        });
    }

    [UnityTest]
    public IEnumerator TestLoomInit_Wrong_Reader_Link()
    {
        return LoomTestContext.AsyncTest(async () =>
        {
            LoomTestContext.LoomManager.ReaderHost = "https://www.google.com";
            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.LoomManager.CreateContract(LoomTestContext.LoomManager.UserDataModel.PrivateKey);
            });
        });
    }
}
