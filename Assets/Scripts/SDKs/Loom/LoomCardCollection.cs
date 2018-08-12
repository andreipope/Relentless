using System.Threading.Tasks;
using LoomNetwork.CZB.Protobuf;

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
