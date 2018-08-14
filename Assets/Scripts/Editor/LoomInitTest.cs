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
            Assert.IsNull(LoomTestContext.BackendFacade.Contract);
            await LoomTestContext.BackendFacade.CreateContract(LoomTestContext.BackendDataControlMediator.UserDataModel.PrivateKey);
            Assert.IsNotNull(LoomTestContext.BackendFacade.Contract);
        });
    }

    [UnityTest]
    public IEnumerator TestLoomInit_Empty_Writer_Link()
    {
        return LoomTestContext.AsyncTest(async () =>
        {
            LoomTestContext.BackendFacade.WriterHost = string.Empty;
            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.BackendFacade.CreateContract(LoomTestContext.BackendDataControlMediator.UserDataModel.PrivateKey);
            });
        });
    }

    [UnityTest]
    public IEnumerator TestLoomInit_Wrong_Writer_Link()
    {
        return LoomTestContext.AsyncTest(async () =>
        {
            LoomTestContext.BackendFacade.WriterHost = "https://www.google.com";
            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.BackendFacade.CreateContract(LoomTestContext.BackendDataControlMediator.UserDataModel.PrivateKey);
            });
        });
    }

    [UnityTest]
    public IEnumerator TestLoomInit_Empty_Reader_Link()
    {
        return LoomTestContext.AsyncTest(async () =>
        {
            LoomTestContext.BackendFacade.ReaderHost = string.Empty;
            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.BackendFacade.CreateContract(LoomTestContext.BackendDataControlMediator.UserDataModel.PrivateKey);
            });
        });
    }

    [UnityTest]
    public IEnumerator TestLoomInit_Wrong_Reader_Link()
    {
        return LoomTestContext.AsyncTest(async () =>
        {
            LoomTestContext.BackendFacade.ReaderHost = "https://www.google.com";
            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.BackendFacade.CreateContract(LoomTestContext.BackendDataControlMediator.UserDataModel.PrivateKey);
            });
        });
    }
}
