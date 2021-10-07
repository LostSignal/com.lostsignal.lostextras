﻿// <auto-generated/>
#pragma warning disable

#if USING_PLAYFAB

namespace Lost.CloudFunctions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lost.CloudFunctions.Common;
    using Lost.CloudFunctions.Debug;
    using Lost.CloudFunctions.Login;

    public static class CommonCloudFunctionsExtensions
    {
        public static Task<Result> Common_IncrementBadCallCount(this CloudFunctionsManager cloudFunctionsManager) => cloudFunctionsManager.Execute("Common_IncrementBadCallCount");

        public static Task<Result> Common_StartUnityCloudBuilds(this CloudFunctionsManager cloudFunctionsManager, StartUnityCloudBuildsRequest request) => cloudFunctionsManager.Execute("Common_StartUnityCloudBuilds", request);

        public static Task<Result> Common_GrantDefaultCharacter(this CloudFunctionsManager cloudFunctionsManager) => cloudFunctionsManager.Execute("Common_GrantDefaultCharacter");
    }
}

#endif
