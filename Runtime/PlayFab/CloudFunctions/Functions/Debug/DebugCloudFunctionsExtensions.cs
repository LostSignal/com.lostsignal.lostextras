// <auto-generated/>
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

    public static class DebugCloudFunctionsExtensions
    {
        public static Task<Result> Debug_PurchaseItem(this CloudFunctionsManager cloudFunctionsManager, DebugPurchaseItemRequest request) => cloudFunctionsManager.Execute("Debug_PurchaseItem", request);

        public static Task<Result> Debug_GiveCurrency(this CloudFunctionsManager cloudFunctionsManager, DebugGiveCurrencyRequest request) => cloudFunctionsManager.Execute("Debug_GiveCurrency", request);

        public static Task<Result> Debug_DeleteUser(this CloudFunctionsManager cloudFunctionsManager, string request) => cloudFunctionsManager.Execute("Debug_DeleteUser", request);
    }
}

#endif
