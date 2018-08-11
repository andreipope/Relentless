

using System;
using System.Threading.Tasks;
using Loom.Unity3d.Zb;
using System.Linq;
using System.Collections.Generic;

public partial class LoomManager 
{
    private const string UploadHistoryMethod = "UploadHistory"; //just a random method for now

	private List<string> HistoryData;
    
	public void ClearHistory() 
	{
		if (HistoryData == null)
			HistoryData = new List<string> ();

		HistoryData.Clear ();
	}

	public void UpdateHistory(string data) 
	{
		HistoryData.Add (data);
	}

    public async Task UploadHistory(string userId)
    {
        var req = new UpsertAccountRequest {
            UserId = userId,
			//we'll also put all our collected strings in the HistoryData List
        };

        //await Contract.CallAsync(CreateAccountMethod, req);
    }
}
