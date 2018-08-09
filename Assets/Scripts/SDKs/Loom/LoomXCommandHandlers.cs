using Opencoding.CommandHandlerSystem;
using UnityEngine;

public static class LoomXCommandHandlers
{

	public static void Initialize()
	{
		CommandHandlers.RegisterCommandHandlers(typeof(LoomXCommandHandlers));	
	}
	
	
	[CommandHandler(Description = "Get Contract Write Host Link")]
	private static void GetContractWriteLink()
	{
		CustomDebug.Log("Link =  '" + LoomManager.Instance.WriteHost);
	}
	
	
	[CommandHandler(Description = "Get Contract Reader Host Link")]
	private static void GetContractReaderLink()
	{
		CustomDebug.Log("Link =  '" + LoomManager.Instance.ReaderHost);
	}
	
	
	[CommandHandler(Description = "Init Contract with Write Host Link")]
	private static async void InitContractWithWriteLink(string link)
	{
		LoomManager.Instance.WriteHost = link;
		await LoomManager.Instance.CreateContract(()=> { CustomDebug.Log("LoomX Initialized..");});
	}
	
	
	[CommandHandler(Description = "Init Contract with Write Host Link")]
	private static async void InitContractWithReaderLink(string link)
	{
		LoomManager.Instance.ReaderHost = link;
		await LoomManager.Instance.CreateContract(()=> { CustomDebug.Log("LoomX Initialized..");});
	}
	
	
	[CommandHandler(Description = "Init Contract with Write and Reader Host Link")]
	private static async void InitContract(string writer, string reader)
	{
		LoomManager.Instance.WriteHost = writer;
		LoomManager.Instance.ReaderHost = reader;
		await LoomManager.Instance.CreateContract(() =>
		{
			CustomDebug.Log("LoomX Initialized..");
		});
	}
}
