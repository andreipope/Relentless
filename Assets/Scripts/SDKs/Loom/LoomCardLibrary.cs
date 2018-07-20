
using System.Threading.Tasks;
using Loom.Unity3d.Zb;

public partial class LoomManager
{
    private const string GetCardLibraryMethod = "ListCardLibrary";
    
    public async Task<ListCardLibraryResponse> GetCardLibrary()
    {
        if (_contract == null)
            await Init();

        var request = new ListCardLibraryRequest();
        
        return await _contract.StaticCallAsync<ListCardLibraryResponse>(GetCardLibraryMethod, request);
    }
}
