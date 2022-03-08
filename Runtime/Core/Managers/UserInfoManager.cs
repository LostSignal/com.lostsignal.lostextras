//-----------------------------------------------------------------------
// <copyright file="UserInfoManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using Lost.Networking;

    public interface IUserInfoManager
    {
        long UserId { get; }

        string UserHexId { get; }

        string DisplayName { get; }

        UserInfo GetMyUserInfo();
    }

    public class UserInfoManager : Manager<UserInfoManager>, IUserInfoManager
    {
        private UserInfo userInfo = UserInfo.GenerateRandomUserInfo();

        public long UserId => this.userInfo.UserId;

        public string UserHexId => this.userInfo.UserHexId;

        public string DisplayName => this.userInfo.DisplayName;

        public UserInfo GetMyUserInfo() => this.userInfo;

        public override void Initialize()
        {
            this.SetInstance(this);
        }
    }
}

#endif
