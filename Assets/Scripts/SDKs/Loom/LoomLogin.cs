

using System;
using System.Threading.Tasks;
using Loom.Unity3d.Zb;

public partial class LoomManager 
{
    private const string CreateAccountMethod = "CreateAccount";
    
    public async Task SignUp(string userId, Action<string> errorResult)
    {
        if (Contract == null)
            await CreateContract();
        
        var req = new UpsertAccountRequest {
            UserId = userId
        };

        try
        {
            await Contract.CallAsync(CreateAccountMethod, req);
            errorResult?.Invoke(string.Empty);
        }
        catch (Exception ex)
        {
            //Debug.Log("Exception = " + ex);
            errorResult?.Invoke(ex.ToString());
        }
    }
}
