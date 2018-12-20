using Facebook.MiniJSON;
using Facebook.Unity;
using Facebook.Unity.Editor;
using Facebook.Unity.Settings;
using Loom.ZombieBattleground.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class FacebookManager : IService, IFacebookManager
    {
        public event Action<bool> OnFacebookInitEvent;

        private List<string> _facebokReadPermissions,
                             _facebookPublishPermissions;


        private FBUser _fbUser;

        private IDataManager _dataManager;

        public FBUser FacebookUser { get { return _fbUser; } }

        public bool IsLoggined { get { return FB.IsLoggedIn; } }
        public bool IsInitialized { get { return FB.IsInitialized; } }


        public void Dispose()
        {
        }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();

            InitFacebook();
        }

        public void Update()
        {
        }


        public void InitFacebook()
        {
            if (FB.IsInitialized)
                return;

            FacebookSettings.AppIds = new List<string> { Constants.FacebookAppId };

            FB.Init(OnInitComplete, OnHideUnity);
        }

        public void FeedShare(string title, string caption, string description, string uri, string imageUri)
        {
            if (!FB.IsInitialized)
                return;

            FB.ShareLink(new Uri(uri), title, description, new Uri(imageUri), FeedShareResponse);
        }

        private void OnInitComplete()
        {
            FB.ActivateApp();

            if (OnFacebookInitEvent != null)
                OnFacebookInitEvent(true);
        }

        private void OnHideUnity(bool isGameShown)
        {

        }

        #region api responses
        private void FeedShareResponse(IResult result)
        {
            if (!IsResponseSuccessfull(result))
                return;

        }

        #endregion 

        #region helpers

        private bool IsResponseSuccessfull(IResult result)
        {
            if (result == null)
            {
                return false;
            }
            else if (!string.IsNullOrEmpty(result.Error))
            {
                return false;
            }
            else if (result.Cancelled)
            {
                return false;
            }

            return true;
        }


        #endregion
    }


    public struct FBUser
    {
        public string access_token;
        public string key_hash;
        public string id;
        public string first_name;
        public string last_name;
        public string email;
        public Sprite image;
    }
}
