using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Runtime.InteropServices;
using Trisibo; // Add this line


public class MenuManager : MonoBehaviour
{
    [Header("Help Window")]
    public GameObject HelpWindow;

    [Header("Profile Attributes Placeholders")]
    public TextMeshProUGUI PlayerName_Placeholder;
    public TextMeshProUGUI WalletID_Placeholder;
    public Texture2D m_CursorDefaultTexture;

    bool GameLaunched = false;
    private void Awake()
    {
        Cursor.SetCursor(m_CursorDefaultTexture, Vector2.zero, CursorMode.Auto);
        PopulateProfileDetails();
    }
    public void LaunchGame()
    {
        if (!GameLaunched)
        {
            GameLaunched = true;
            Loader.Instance.LoadScene(Loader.SceneToLoad.Game);
        }
    }

    public void ToggleHelpWindow(bool State)
    {
        HelpWindow.SetActive(State);
    }


    string InsertAsteriskInMiddle(string input)
    {
        int length = input.Length;

        int middle = length / 2;

        string firstHalf = input.Substring(0, (middle / 2));
        //string secondHalf = input.Substring(middle);

        return firstHalf + "****";
    }

    void PopulateProfileDetails()
    {
        PlayerName_Placeholder.text = StaticDataBank.UserName.ToString();
        string walletaddress = InsertAsteriskInMiddle(StaticDataBank.walletAddress.ToString());
        WalletID_Placeholder.text = walletaddress;
    }

    public void CopyWalletAdress()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    WebGLCopyAndPaste.Copy(StaticDataBank.walletAddress);
#else
        GUIUtility.systemCopyBuffer = StaticDataBank.walletAddress;
#endif
        GlobalCanvasManager.Instance.SocketPrompter.ShowPopup("Wallet Address copied");
    }

    public void ChangeSkin(int skinIndex)
    {
        //m_MainCharacter.Skeleton.ChangeCharacterSkin(skinIndex, CharacterSkins);
    }

    bool _withDrawOpen = false;
    public RectTransform WithDrawPanel;

    public void ToggleWithdrawPanel()
    {
        if (_withDrawOpen)
        {
            WithDrawPanel.DOScaleY(0f, 0.3f).OnComplete(delegate
            {
                _withDrawOpen = false;
            });
        }
        else
        {
            WithDrawPanel.DOScaleY(1f, 0.3f).OnComplete(delegate
            {
                _withDrawOpen = true;
            });
        }
    }
    public void WithDrawCurrency()
    {
        API_Manager.Instance.WithdrawFunds((isSuccess, response) =>
        {
            if (isSuccess)
            {
                Utils.OpenURLInNewTab(response);
            }
        });
    }

    // [DllImport("__Internal")]
    // private static extern void CopyToClipboard(string str);

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void CopyToClipboard(string str);
#else

    /// <summary>Copy text to the clipboard on all platforms.</summary>
    public static void CopyToClipboard(string text)
    {
        GUIUtility.systemCopyBuffer = text;
    }
#endif
}
