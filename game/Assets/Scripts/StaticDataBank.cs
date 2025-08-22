
using System.Text.RegularExpressions;

public static class StaticDataBank
{
    #region BackEnd Keys
    public static readonly string FireBase_Web_API_Key = "AIzaSyA1XOUyK4kksadk0MTOc_L25qcgMmruGKo";
    public static readonly string clientId = "713724247097-hnap0s2devatbplgk9pjo8hconih6dhu.apps.googleusercontent.com";
    public static readonly string clientSecret = "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";
    //public static readonly string clientId = "283391004653-4e9vpcvs2ftrnpuebag6gva9814mc3i3.apps.googleusercontent.com";
    //public static readonly string clientSecret = "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";

    #endregion


    #region Firebase Api's
    public static readonly string signUpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FireBase_Web_API_Key}";
    public static readonly string signInUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FireBase_Web_API_Key}";
    public static readonly string firebaseEndpoint = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key={FireBase_Web_API_Key}";
    #endregion

    #region Google Api's
    public static readonly string tokenEndpoint = "https://oauth2.googleapis.com/token";
    #endregion

    #region Backend Api's

    // public static readonly string SOCKET_URL = "http://10.0.2.2:3000";
    public static readonly string SOCKET_URL = "http://localhost:3000";
    // public static readonly string SOCKET_URL = "https://red-runner-monorepo-952100349016.us-central1.run.app";
    // public static readonly string SOCKET_URL = "https://red-runner-monorepo-952100349016.us-central1.run.app";

    public static readonly string QR_SOCKET_URL = SOCKET_URL + "/qr-signup";
    public static readonly string BackendBaseUrl = SOCKET_URL + "/v1/";

    public static readonly string registerOrLogin = BackendBaseUrl + "auth/registerOrLogin";
    //public static readonly string Register = BackendBaseUrl + "auth/register";
    //public static readonly string LogIn = BackendBaseUrl + "auth/login";
    public static readonly string LeaderBoard_Submit = BackendBaseUrl + "leaderboard/addEntry?userId=";
    public static readonly string Get_LeaderBoard = BackendBaseUrl + "leaderboard/getLeaderboard?userId=";
    public static readonly string Get_LeaderBoard_Relative = BackendBaseUrl + "leaderboard/getRelativeRank?userId=";
    public static readonly string Get_ShopData = BackendBaseUrl + "shop/getShopData?userId=";
    public static readonly string Get_Inventory = BackendBaseUrl + "gs/getInventory?userId=";
    public static readonly string Mint = BackendBaseUrl + "gs/mintAsset?userId=";
    public static readonly string Buy = BackendBaseUrl + "gs/buyItem?userId=";
    public static readonly string Withdraw = BackendBaseUrl + "gs/withdraw?userId=";
    public static readonly string TokensPushingLink = BackendBaseUrl + "gs/claimCoins?userId=";
    public static readonly string InventoryUpdateLink = BackendBaseUrl + "gs/updateNft?userId=";
    public static readonly string GetMarketPlace = BackendBaseUrl + "gs/getMarketPlace?userId=";
    public static readonly string listForSale = BackendBaseUrl + "gs/listForSale?userId=";
    public static readonly string UnlistForSale = BackendBaseUrl + "gs/cancelListing?userId=";
    public static readonly string BuyItemFromMarketplace = BackendBaseUrl + "gs/buyItemFromMarket?userId=";
    #endregion

    #region Static Profile Data

    public static string playerlocalid = "";
    public static string UserName = "";
    public static string jwttoken = "";
    public static string walletAddress = "";


    private static string SpeedBoosterCollectionID = "0dfe473e-bbb7-453f-8d3f-ba9af79dfc14";
    private static string DoubleJumpCollectionID = "0b9d2116-b3a2-4452-affb-03282313ab77";
    private static string SkinCollectionID = "36399a18-941c-4c18-bb0d-8cc2aaaa8b06";

    #endregion
    public static string GetCollectionId(int index)
    {
        switch (index)
        {
            case 0:
                return SpeedBoosterCollectionID.ToString();
            case 1:
                return DoubleJumpCollectionID.ToString();
            case 2:
                return SkinCollectionID.ToString();
            default:
                return null;
        }
    }


    public static string RemoveWordFromString(string m_string)
    {

        string[] namesToRemove = new string[] { "Skin", "Speed Boosters", "Double Jumps" };

        foreach (string _name in namesToRemove)
        {
            m_string = Regex.Replace(m_string, @"\b" + Regex.Escape(_name) + @"\b", "", RegexOptions.IgnoreCase).Trim();
        }

        return m_string;
    }



    public static bool CheckInputField(string checkString)
    {
        if (!string.IsNullOrEmpty(checkString) && !string.IsNullOrWhiteSpace(checkString))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
