
using System.Threading.Tasks;
using LoomNetwork.CZB.Protobuf;

public partial class LoomManager
{
    private const string GetCardLibraryMethod = "ListCardLibrary";
    
    public async Task<ListCardLibraryResponse> GetCardLibrary()
    {
        var request = new ListCardLibraryRequest();
        
        return await Contract.StaticCallAsync<ListCardLibraryResponse>(GetCardLibraryMethod, request);
    }
}
