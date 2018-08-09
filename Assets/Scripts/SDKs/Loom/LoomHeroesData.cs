

using System.Threading.Tasks;
using Loom.Unity3d.Zb;

public partial class LoomManager
{
    private const string HeroesList = "ListHeroes";

    public async Task<ListHeroesResponse> GetHeroesList(string userId)
    {
        var request = new ListHeroesRequest
        {
            UserId = userId
        };

        return await Contract.StaticCallAsync<ListHeroesResponse>(HeroesList, request);
    }
}
