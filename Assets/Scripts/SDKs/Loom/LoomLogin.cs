

using System;
using System.Threading.Tasks;
using Loom.Unity3d.Zb;

public partial class LoomManager 
{
    private const string CreateAccountMethod = "CreateAccount";
    
    public async Task SignUp(string userId)
    {
        var req = new UpsertAccountRequest {
            UserId = userId
        };

        await Contract.CallAsync(CreateAccountMethod, req);
    }
}
