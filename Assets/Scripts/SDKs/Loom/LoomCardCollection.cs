using System.Threading.Tasks;
using Loom.Unity3d.Zb;

public partial class LoomManager
{
	private const string GetCardCollectionMethod = "GetCollection";
    
	public async Task<GetCollectionResponse> GetCardCollection(string userId)
	{
		var request = new GetCollectionRequest
		{
			UserId = userId
		};
        
		return await Contract.StaticCallAsync<GetCollectionResponse>(GetCardCollectionMethod, request);
	}
	
}
