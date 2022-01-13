//-----------------------------------------------------------------------
// <copyright file="PlayFabUserInfoManager.cs" company="Full Circle Games">
//     Copyright (c) Full Circle Games. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

namespace Lost.PlayFab
{
    using Lost.Networking;

    public class PlayFabUserInfoManager : Manager<PlayFabUserInfoManager>, IUserInfoManager
    {
        public long UserId { get; private set; }

        public string UserHexId { get; private set; }

        public string DisplayName { get; private set; }

        private UserInfo userInfo = new UserInfo();

        public UserInfo GetMyUserInfo()
        {
            this.userInfo.UserId = this.UserId;
            this.userInfo.UserHexId = this.UserHexId;
            this.userInfo.DisplayName = this.DisplayName;

            userInfo.SetSessionTicket(PlayFabManager.Instance.Login.SessionTicket);

            return userInfo;
        }

        public override void Initialize()
        {
            this.StartCoroutine(Initialize());

            System.Collections.IEnumerator Initialize()
            {
                yield return PlayFabManager.WaitForInitialization();

                this.UserId = PlayFabManager.Instance.User.PlayFabNumericId;
                this.UserHexId = PlayFabManager.Instance.User.PlayFabId;
                this.DisplayName = PlayFabManager.Instance.User.DisplayName;

                this.SetInstance(this);
            }
        }
    }
}

#endif
