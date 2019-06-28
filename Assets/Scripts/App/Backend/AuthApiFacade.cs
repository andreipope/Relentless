using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using log4net;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Iap;
using Newtonsoft.Json;
using Plugins.AsyncAwaitUtil.Source;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class AuthApiFacade : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(AuthApiFacade));

        private BackendDataControlMediator _backendDataControlMediator;
        private BackendFacade _backendFacade;
        
        public string AuthApiHost { get; private set; }
        
        public string VaultApiHost { get; private set; }

        public AuthApiFacade(string authApiHost, string vaultApiHost)
        {
            SetEndpoints(authApiHost, vaultApiHost);
        }

        public void SetEndpoints(string authApiHost, string vaultApiHost)
        {
            AuthApiHost = authApiHost;
            VaultApiHost = vaultApiHost;
            Log.Info("Auth Host: " + AuthApiHost);
            Log.Info("Vault Host: " + VaultApiHost);
        }
        
        #region IService

        public void Init()
        {
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        public void Update() { }

        public void Dispose() { }

        #endregion
        
        private const string userInfoEndPoint = "/user/info";

        private const string loginEndPoint = "/auth/email/login";

        private const string signupEndPoint = "/auth/email/game_signup";

        private const string forgottenPasswordEndPoint = "/auth/mlink/generate";

        private const string createVaultTokenEndPoint = "/auth/loom-userpass/create_token";

        private const string accessVaultEndPoint = "/entcubbyhole/loomauth";

        private const string createVaultTokenForNon2FAUsersEndPoint = "/auth/loom-simple-userpass/create_token";

        public async Task<UserInfo> GetUserInfo(string accessToken)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = AuthApiHost + userInfoEndPoint;
            webrequestCreationInfo.Headers.Add("authorization", "Bearer " + accessToken);

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(
                httpResponseMessage.ReadToEnd(),

                // FIXME: backend should return valid version numbers at all times
                new VersionConverterWithFallback(Version.Parse(Constants.CurrentVersionBase))
            );

            return userInfo;
        }

        public async Task<LoginData> InitiateLogin(string email, string password)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = AuthApiHost + loginEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            LoginRequest loginRequest = new LoginRequest();
            loginRequest.email = email;
            loginRequest.password = password;
            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(loginRequest));
            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");
            webrequestCreationInfo.Headers.Add("authority", "auth.loom.games");

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            Log.Debug(httpResponseMessage.ReadToEnd());
            LoginData loginData = JsonConvert.DeserializeObject<LoginData>(
                httpResponseMessage.ReadToEnd());
            return loginData;
        }

        public async Task<RegisterData> InitiateRegister(string email, string password)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = AuthApiHost + signupEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            LoginRequest loginRequest = new LoginRequest();
            loginRequest.email = email;
            loginRequest.password = password;
            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(loginRequest));
            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");
            webrequestCreationInfo.Headers.Add("authority", "auth.loom.games");

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            RegisterData registerData = JsonConvert.DeserializeObject<RegisterData>(
                httpResponseMessage.ReadToEnd());
            return registerData;
        }

        public async Task<bool> InitiateForgottenPassword(string email)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = AuthApiHost + forgottenPasswordEndPoint + "?email=" + email + "&kind=signup";

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            return true;
        }

        public async Task<CreateVaultTokenData> CreateVaultToken(string otp, string accessToken)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = VaultApiHost + createVaultTokenEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            VaultTokenRequest vaultTokenRequest = new VaultTokenRequest();
            vaultTokenRequest.authy_token = otp;
            vaultTokenRequest.access_token = accessToken;

            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(vaultTokenRequest));
            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);
            Log.Debug(httpResponseMessage.ReadToEnd());

            CreateVaultTokenData vaultTokenData = JsonConvert.DeserializeObject<CreateVaultTokenData>(
                httpResponseMessage.ReadToEnd());
            return vaultTokenData;
        }

        public async Task<CreateVaultTokenData> CreateVaultTokenForNon2FAUsers(string accessToken)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = VaultApiHost + createVaultTokenForNon2FAUsersEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            VaultTokenNon2FARequest vaultTokenRequest = new VaultTokenNon2FARequest();
            vaultTokenRequest.access_token = accessToken;

            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(vaultTokenRequest));
            Log.Debug(JsonConvert.SerializeObject(vaultTokenRequest));
            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);
            Log.Debug(httpResponseMessage.ReadToEnd());

            CreateVaultTokenData vaultTokenData = JsonConvert.DeserializeObject<CreateVaultTokenData>(
                httpResponseMessage.ReadToEnd());
            return vaultTokenData;
        }

        public async Task<GetVaultDataResponse> GetVaultData(string vaultToken)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.GET;
            webrequestCreationInfo.Url = VaultApiHost + accessVaultEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");
            webrequestCreationInfo.Headers.Add("X-Vault-Token", vaultToken);

            HttpResponseMessage httpResponseMessage =
                await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                if (httpResponseMessage.StatusCode.ToString() == Constants.VaultEmptyErrorCode)
                {
                    throw new Exception(httpResponseMessage.StatusCode.ToString());
                }
                else
                {
                    httpResponseMessage.ThrowOnError(webrequestCreationInfo);
                }
            }
            Log.Debug(httpResponseMessage.ReadToEnd());


            GetVaultDataResponse getVaultDataResponse = JsonConvert.DeserializeObject<GetVaultDataResponse>(
                httpResponseMessage.ReadToEnd());
            return getVaultDataResponse;
        }

        public async Task<bool> SetVaultData(string vaultToken, string privateKey)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = VaultApiHost + accessVaultEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            VaultPrivateKeyRequest vaultPrivateKeyRequest = new VaultPrivateKeyRequest();
            vaultPrivateKeyRequest.privatekey = privateKey;

            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(vaultPrivateKeyRequest));
            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");
            webrequestCreationInfo.Headers.Add("X-Vault-Token", vaultToken);

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            Log.Debug(httpResponseMessage.ReadToEnd());
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            return true;
        }

        public async Task<ZbVersion> GetZbVersionData(BackendPurpose backendPurpose)
        {
            string environmentName = backendPurpose.ToString().ToLowerInvariant();
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url =
                $"{AuthApiHost}/zbversion?version={Constants.CurrentVersionBase}&environment={environmentName}";

            Log.Debug(webrequestCreationInfo.Url);

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);
            Log.Debug(httpResponseMessage.ReadToEnd());

            ZbVersion version = JsonConvert.DeserializeObject<ZbVersion>(httpResponseMessage.ReadToEnd());
            return version;
        }

        public BackendEndpoint GetProductionBackendEndpointFromZbVersion(ZbVersion zbVersion, PlasmachainEndpointsConfiguration fallbackPlasmachainEndpointsConfiguration)
        {
            ZbVersion.ZbVersionData versionData = zbVersion.Version;

            PlasmachainEndpointsConfiguration plasmaChainEndpointsConfiguration;
            if (String.IsNullOrEmpty(versionData.PlasmachainChainId) ||
                String.IsNullOrEmpty(versionData.PlasmachainReaderHost))
            {
                // Until prod auth is updated
                plasmaChainEndpointsConfiguration = fallbackPlasmachainEndpointsConfiguration;
            }
            else
            {
                plasmaChainEndpointsConfiguration = new PlasmachainEndpointsConfiguration(
                    versionData.PlasmachainChainId,
                    versionData.PlasmachainReaderHost,
                    versionData.PlasmachainWriterHost,
                    versionData.PlasmachainZbgCardContractAddress,
                    versionData.PlasmachainCardFaucetContractAddress,
                    versionData.PlasmachainBoosterPackContractAddress,
                    versionData.PlasmachainSuperPackContractAddress,
                    versionData.PlasmachainAirPackContractAddress,
                    versionData.PlasmachainEarthPackContractAddress,
                    versionData.PlasmachainFirePackContractAddress,
                    versionData.PlasmachainLifePackContractAddress,
                    versionData.PlasmachainToxicPackContractAddress,
                    versionData.PlasmachainWaterPackContractAddress,
                    versionData.PlasmachainSmallPackContractAddress,
                    versionData.PlasmachainMinionPackContractAddress,
                    versionData.PlasmachainBinancePackContractAddress,
                    versionData.PlasmachainFiatPurchaseContractAddress,
                    versionData.PlasmachainOpenLotteryContractAddress,
                    versionData.PlasmachainTronLotteryContractAddress
                );
            }

            return new BackendEndpoint(
                versionData.AuthUrl,
                versionData.ReadUrl,
                versionData.WriteUrl,
                versionData.VaultUrl,
                versionData.DataVersion,
                versionData.IsMaintenanceMode,
                versionData.IsForceUpdate,
                false,
                plasmaChainEndpointsConfiguration
            );
        }

        private struct LoginRequest
        {
            public string email;
            public string password;
        }

        private struct VaultTokenRequest
        {
            public string authy_token;
            public string access_token;
        }

        private struct VaultTokenNon2FARequest
        {
            public string access_token;
        }

        private struct VaultPrivateKeyRequest
        {
            public string privatekey;
        }
    }
}
