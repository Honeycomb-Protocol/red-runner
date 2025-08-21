using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// using UnityWebView;  // Ensure you are using UnityWebView

[Serializable]
public class UIDataConatainer
{
    public string boosterName;
    public TextMeshProUGUI boosterNameText;
    public TextMeshProUGUI SOL_Price;
    public TextMeshProUGUI USDC_Price;
    public TextMeshProUGUI UsesLeft;
    public Image boosterImage;
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    private WebViewObject webViewObject;  // Declare the WebViewObject

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Initialize the WebViewObject
        webViewObject = new GameObject("WebViewObject").AddComponent<WebViewObject>();
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.canvas = GameObject.Find("Canvas");
        if (webViewObject.canvas == null)
        {
            Debug.LogError("Canvas not found in the scene!");
        }
#endif

    }

    [Header("Detail Panel UI")]
    //public GameObject DetailPanel;
    public TextMeshProUGUI attribute;
    public TextMeshProUGUI BoosterName;
    public TextMeshProUGUI Description;
    public Image boosterImage;
    public Button MintButton;
    public Button ListButton;
    public GameObject Blocker;

    public List<UIDataConatainer> speedbooster;
    public List<UIDataConatainer> doublejump;
    public List<UIDataConatainer> skins;

    public Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();
    public GameShop _gameShop;

    private string _selectedMintId;

        private Coroutine _loadCoroutine;

    void HidePopUp()
    {
        UIManager.Instance.SelectDefaultFeatureWindowOption();
    }

    void GetAllShopData(bool success, GameShop response)
    {
        if (success)
        {
            if ((_gameShop != null) && _gameShop.boosters.speed_boosters.speed_booster_6.price.SOL == response.boosters.speed_boosters.speed_booster_6.price.SOL &&
                _gameShop.boosters.double_jump_boosters.double_jump_3.price.SOL == response.boosters.double_jump_boosters.double_jump_3.price.SOL &&
                _gameShop.skins.alienSkin.price.SOL == response.skins.alienSkin.price.SOL)
            {
                UIManager.Instance.SelectDefaultFeatureWindowOption();
                return;
            }
            _gameShop = response;
            StartCoroutine(PopulateData());
        }
        else
        {
            UIManager.Instance.ActivateFailureScreen("shop");
        }
    }

    IEnumerator PopulateData()
    {
        StartCoroutine(DownloadImage(_gameShop.boosters.double_jump_boosters.double_jump_3.imageUrl, _gameShop.boosters.double_jump_boosters.double_jump_3.name, _gameShop.boosters.double_jump_boosters.double_jump_3.price.SOL, _gameShop.boosters.double_jump_boosters.double_jump_3.price.USDC, _gameShop.boosters.double_jump_boosters.double_jump_3.attributes[0].value, doublejump[0]));
        StartCoroutine(DownloadImage(_gameShop.boosters.double_jump_boosters.double_jump_6.imageUrl, _gameShop.boosters.double_jump_boosters.double_jump_6.name, _gameShop.boosters.double_jump_boosters.double_jump_6.price.SOL, _gameShop.boosters.double_jump_boosters.double_jump_6.price.USDC, _gameShop.boosters.double_jump_boosters.double_jump_6.attributes[0].value, doublejump[1]));
        StartCoroutine(DownloadImage(_gameShop.boosters.double_jump_boosters.double_jump_10.imageUrl, _gameShop.boosters.double_jump_boosters.double_jump_10.name, _gameShop.boosters.double_jump_boosters.double_jump_10.price.SOL, _gameShop.boosters.double_jump_boosters.double_jump_10.price.USDC, _gameShop.boosters.double_jump_boosters.double_jump_10.attributes[0].value, doublejump[2]));
        StartCoroutine(DownloadImage(_gameShop.boosters.double_jump_boosters.double_jump_999.imageUrl, _gameShop.boosters.double_jump_boosters.double_jump_999.name, _gameShop.boosters.double_jump_boosters.double_jump_999.price.SOL, _gameShop.boosters.double_jump_boosters.double_jump_999.price.USDC, _gameShop.boosters.double_jump_boosters.double_jump_999.attributes[0].value, doublejump[3]));
        StartCoroutine(DownloadImage(_gameShop.boosters.speed_boosters.speed_booster_3.imageUrl, _gameShop.boosters.speed_boosters.speed_booster_3.name, _gameShop.boosters.speed_boosters.speed_booster_3.price.SOL, _gameShop.boosters.speed_boosters.speed_booster_3.price.USDC, _gameShop.boosters.speed_boosters.speed_booster_3.attributes[0].value, speedbooster[0]));
        StartCoroutine(DownloadImage(_gameShop.boosters.speed_boosters.speed_booster_6.imageUrl, _gameShop.boosters.speed_boosters.speed_booster_6.name, _gameShop.boosters.speed_boosters.speed_booster_6.price.SOL, _gameShop.boosters.speed_boosters.speed_booster_6.price.USDC, _gameShop.boosters.speed_boosters.speed_booster_6.attributes[0].value, speedbooster[1]));
        StartCoroutine(DownloadImage(_gameShop.boosters.speed_boosters.speed_booster_10.imageUrl, _gameShop.boosters.speed_boosters.speed_booster_10.name, _gameShop.boosters.speed_boosters.speed_booster_10.price.SOL, _gameShop.boosters.speed_boosters.speed_booster_10.price.USDC, _gameShop.boosters.speed_boosters.speed_booster_10.attributes[0].value, speedbooster[2]));
        StartCoroutine(DownloadImage(_gameShop.boosters.speed_boosters.speed_booster_999.imageUrl, _gameShop.boosters.speed_boosters.speed_booster_999.name, _gameShop.boosters.speed_boosters.speed_booster_999.price.SOL, _gameShop.boosters.speed_boosters.speed_booster_999.price.USDC, _gameShop.boosters.speed_boosters.speed_booster_999.attributes[0].value, speedbooster[3]));
        StartCoroutine(DownloadImage(_gameShop.skins.alienSkin.imageUrl, _gameShop.skins.alienSkin.name, _gameShop.skins.alienSkin.price.SOL, _gameShop.skins.alienSkin.price.USDC, _gameShop.skins.alienSkin.attributes[0].value, skins[0]));
        StartCoroutine(DownloadImage(_gameShop.skins.christmasSkin.imageUrl, _gameShop.skins.christmasSkin.name, _gameShop.skins.christmasSkin.price.SOL, _gameShop.skins.christmasSkin.price.USDC, _gameShop.skins.christmasSkin.attributes[0].value, skins[1]));
        StartCoroutine(DownloadImage(_gameShop.skins.halloweenSkin.imageUrl, _gameShop.skins.halloweenSkin.name, _gameShop.skins.halloweenSkin.price.SOL, _gameShop.skins.halloweenSkin.price.USDC, _gameShop.skins.halloweenSkin.attributes[0].value, skins[2]));
        StartCoroutine(DownloadImage(_gameShop.skins.polkaDotSkin.imageUrl, _gameShop.skins.polkaDotSkin.name, _gameShop.skins.polkaDotSkin.price.SOL, _gameShop.skins.polkaDotSkin.price.USDC, _gameShop.skins.polkaDotSkin.attributes[0].value, skins[3]));
        StartCoroutine(DownloadImage(_gameShop.skins.robotSkin.imageUrl, _gameShop.skins.robotSkin.name, _gameShop.skins.robotSkin.price.SOL, _gameShop.skins.robotSkin.price.USDC, _gameShop.skins.robotSkin.attributes[0].value, skins[4]));
        StartCoroutine(DownloadImage(_gameShop.skins.solanaSkin.imageUrl, _gameShop.skins.solanaSkin.name, _gameShop.skins.solanaSkin.price.SOL, _gameShop.skins.solanaSkin.price.USDC, _gameShop.skins.solanaSkin.attributes[0].value, skins[5]));
        StartCoroutine(DownloadImage(_gameShop.skins.spaceSkin.imageUrl, _gameShop.skins.spaceSkin.name, _gameShop.skins.spaceSkin.price.SOL, _gameShop.skins.spaceSkin.price.USDC, _gameShop.skins.spaceSkin.attributes[0].value, skins[6]));
        StartCoroutine(DownloadImage(_gameShop.skins.thiefSkin.imageUrl, _gameShop.skins.thiefSkin.name, _gameShop.skins.thiefSkin.price.SOL, _gameShop.skins.thiefSkin.price.USDC, _gameShop.skins.thiefSkin.attributes[0].value, skins[7]));
        StartCoroutine(DownloadImage(_gameShop.skins.wrestlerSkin.imageUrl, _gameShop.skins.wrestlerSkin.name, _gameShop.skins.wrestlerSkin.price.SOL, _gameShop.skins.wrestlerSkin.price.USDC, _gameShop.skins.wrestlerSkin.attributes[0].value, skins[8]));
        StartCoroutine(DownloadImage(_gameShop.skins.zombieSkin.imageUrl, _gameShop.skins.zombieSkin.name, _gameShop.skins.zombieSkin.price.SOL, _gameShop.skins.zombieSkin.price.USDC, _gameShop.skins.zombieSkin.attributes[0].value, skins[9]));
        yield return new WaitForSecondsRealtime(1f);
        UIManager.Instance.SelectDefaultFeatureWindowOption();
    }

    IEnumerator DownloadImage(string imageUrl, string imageName, string SOLPrice, string USDCPrice, string _UsesLeft, UIDataConatainer data)
    {
        bool istartchecking = false;
        GlobalFeaturesManager.Instance.ImageCache.DownloadImage(imageUrl, imageName, (m_sprite) =>
        {
            istartchecking = true;
            if (m_sprite != null)
            {
                Sprite tempSprite = Sprite.Create(m_sprite, new Rect(0, 0, m_sprite.width, m_sprite.height), new Vector2(0.5f, 0.5f));

                if (spriteDictionary.ContainsKey(imageName))
                {
                    spriteDictionary[imageName] = tempSprite;
                }
                else
                {
                    spriteDictionary.Add(imageName, tempSprite);
                }
                data.boosterNameText.text = StaticDataBank.RemoveWordFromString(imageName);
                data.boosterImage.sprite = tempSprite;
                data.SOL_Price.text = SOLPrice;
                data.USDC_Price.text = USDCPrice;
                data.UsesLeft.text = _UsesLeft;

                if (imageName.Contains("Skin"))
                    data.boosterImage.SetNativeSize();
            }
            else
            {
                Debug.Log("Failed to fetch image");
            }
        });

        yield return new WaitUntil(() => istartchecking);
    }

    public void GetSkinsDetails(int index)
    {
        switch (index)
        {
            case 0:
                GameShop.Skin alienSkin = _gameShop.skins.alienSkin;
                ShowDetailPanel(alienSkin);
                break;
            case 1:
                GameShop.Skin christmasSkin = _gameShop.skins.christmasSkin;
                ShowDetailPanel(christmasSkin);
                break;
            case 2:
                GameShop.Skin halloweenSkin = _gameShop.skins.halloweenSkin;
                ShowDetailPanel(halloweenSkin);
                break;
            case 3:
                GameShop.Skin polkaDotSkin = _gameShop.skins.polkaDotSkin;
                ShowDetailPanel(polkaDotSkin);
                break;
            case 4:
                GameShop.Skin robotSkin = _gameShop.skins.robotSkin;
                ShowDetailPanel(robotSkin);
                break;
            case 5:
                GameShop.Skin solanaSkin = _gameShop.skins.solanaSkin;
                ShowDetailPanel(solanaSkin);
                break;
            case 6:
                GameShop.Skin spaceSkin = _gameShop.skins.spaceSkin;
                ShowDetailPanel(spaceSkin);
                break;
            case 7:
                GameShop.Skin thiefSkin = _gameShop.skins.thiefSkin;
                ShowDetailPanel(thiefSkin);
                break;
            case 8:
                GameShop.Skin wrestlerSkin = _gameShop.skins.wrestlerSkin;
                ShowDetailPanel(wrestlerSkin);
                break;
            case 9:
                GameShop.Skin zombieSkin = _gameShop.skins.zombieSkin;
                ShowDetailPanel(zombieSkin);
                break;
        }
    }

    public void GetSpeedBoostersDetails(int index)
    {
        switch (index)
        {
            case 0:
                GameShop.Booster speed = _gameShop.boosters.speed_boosters.speed_booster_3;
                ShowDetailPanel(speed, "speed_booster_3");
                break;
            case 1:
                GameShop.Booster speed1 = _gameShop.boosters.speed_boosters.speed_booster_6;
                ShowDetailPanel(speed1, "speed_booster_6");
                break;
            case 2:
                GameShop.Booster speed2 = _gameShop.boosters.speed_boosters.speed_booster_10;
                ShowDetailPanel(speed2, "speed_booster_10");
                break;
            case 3:
                GameShop.Booster speed3 = _gameShop.boosters.speed_boosters.speed_booster_999;
                ShowDetailPanel(speed3, "speed_booster_999");
                break;

        }
    }

    public void ShowDetailPanel(GameShop.Skin skin)
    {
        if (spriteDictionary.ContainsKey(skin.name))
            boosterImage.sprite = spriteDictionary[skin.name];
        boosterImage.SetNativeSize();
        BoosterName.text = StaticDataBank.RemoveWordFromString(skin.name);
        Description.text = skin.description;
        attribute.text = skin.attributes[0].traitType + ":" + skin.attributes[0].value;
        _selectedMintId = skin.attributes[0].value;
        DetailsBuyButtonPressed();
    }

    public void ShowDetailPanel(GameShop.Booster booster, string mintid)
    {
        if (spriteDictionary.ContainsKey(booster.name))
            boosterImage.sprite = spriteDictionary[booster.name];
        boosterImage.SetNativeSize();
        BoosterName.text = booster.name;
        Description.text = booster.description;
        attribute.text = booster.attributes[0].traitType + ":" + booster.attributes[0].value;
        _selectedMintId = mintid;
        DetailsBuyButtonPressed();
    }

    public void DetailsBuyButtonPressed()
    {
        Minting(_selectedMintId);
    }

    public void Minting(string MintID)
    {
        PopupData newPopupData = new PopupData();
        newPopupData.showSecondButton = true;
        newPopupData.titleString = "Buy Asset";
        newPopupData.contentString = "Choose a currency to buy with";
        newPopupData.firstButtonString = "SOL";
        newPopupData.secondButtonString = "USD";
        newPopupData.firstButtonCallBack = () => MintNft(MintID, true);
        newPopupData.secondButtonCallBack = () => MintNft(MintID, false);
        GlobalCanvasManager.Instance.PopUIHandler.ShowPopup(newPopupData);
        GlobalCanvasManager.Instance.PopUIHandler.ToggleSpecialKillButton(true);
    }

    private void OnDisable()
    {
        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
        }
    }
    public void MintNft(string itemName, bool withSol)
    {
        GlobalCanvasManager.Instance.LoadingPanel.ShowPopup("Processing payment", 5,
            new List<SocketEventsType> { SocketEventsType.paymentComplete });

        Debug.Log("Item Name : " + itemName);
        API_Manager.Instance.BuyNft(itemName, withSol, (success, message) =>
        {
            if (success)
            {
                // Open URL in WebView
                _loadCoroutine = StartCoroutine(LoadWebView(message));
                webViewObject.SetVisibility(true);
                Debug.Log("Checkout URL: " + message);
            }
            else
            {
                Debug.Log(message);
                GlobalCanvasManager.Instance.LoadingPanel.HidePopup();

                GlobalCanvasManager.Instance.PopUIHandler.ShowPopup(new PopupData()
                {
                    titleString = "Error",
                    contentString = message,
                    firstButtonString = "OK",
                    firstButtonCallBack = null
                });
            }
        });
    }

    private void OpenURLInWebView(string Url)
    {
#if !UNITY_WEBPLAYER && !UNITY_WEBGL
        if (Url.StartsWith("http"))
        {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        }
        else
        {
            var exts = new string[]{
                ".jpg",
                ".js",
                ".html"  // should be last
            };
            foreach (var ext in exts)
            {
                var url = Url.Replace(".html", ext);
                var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                var dst = System.IO.Path.Combine(Application.temporaryCachePath, url);
                byte[] result = null;
                if (src.Contains("://"))
                {  // for Android
#if UNITY_2018_4_OR_NEWER
                    // NOTE: a more complete code that utilizes UnityWebRequest can be found in https://github.com/gree/unity-webview/commit/2a07e82f760a8495aa3a77a23453f384869caba7#diff-4379160fa4c2a287f414c07eb10ee36d
                    var unityWebRequest = UnityWebRequest.Get(src);
                    yield return unityWebRequest.SendWebRequest();
                    result = unityWebRequest.downloadHandler.data;
#else
                    var www = new WWW(src);
                    yield return www;
                    result = www.bytes;
#endif
                }
                else
                {
                    result = System.IO.File.ReadAllBytes(src);
                }
                System.IO.File.WriteAllBytes(dst, result);
                if (ext == ".html")
                {
                    webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
                    break;
                }
            }
        }
#else
        if (Url.StartsWith("http")) {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        } else {
            webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
        }
#endif
        // var webView = gameObject.AddComponent<WebViewObject>();
        // webViewObject.LoadURL(url);
        // webView.OnLoaded += (sender, args) =>
        // {
        //     Debug.Log("WebView Loaded");
        // };
        // webView.OnError += (sender, args) =>
        // {
        //     Debug.LogError("WebView Error: " + args.message);
        // };
        // webView.Load();
    }

    public void ReactivateDetailPanel()
    {
        DetailsBuyButtonPressed();
    }

    public void FetchShopData()
    {
        API_Manager.Instance.GetShopData(GetAllShopData);
    }

    // Note: Load web view loads the page but wont make it visible.
    // to do this, you must run SetVisibility(true);
    private IEnumerator LoadWebView(string Url)
    {
        
        webViewObject.Init(
            cb: (msg) =>
            {
                // Debug.Log(string.Format("CallFromJS[{0}]", msg));
            },
            err: (msg) =>
            {
                // Debug.Log(string.Format("CallOnError[{0}]", msg));
            },
            httpErr: (msg) =>
            {
                // Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
            },
            started: (msg) =>
            {
                // Debug.Log(string.Format("CallOnStarted[{0}]", msg));
            },
            hooked: (msg) =>
            {
                // Debug.Log(string.Format("CallOnHooked[{0}]", msg));
            },
            cookies: (msg) =>
            {
                // Debug.Log(string.Format("CallOnCookies[{0}]", msg));
            },
            ld: (msg) =>
            {
                // Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
                // NOTE: the following js definition is required only for UIWebView; if
                // enabledWKWebView is true and runtime has WKWebView, Unity.call is defined
                // directly by the native plugin.
#if true
                var js = @"
                    if (!(window.webkit && window.webkit.messageHandlers)) {
                        window.Unity = {
                            call: function(msg) {
                                window.location = 'unity:' + msg;
                            }
                        };
                    }
                ";
#else
                // NOTE: depending on the situation, you might prefer this 'iframe' approach.
                // cf. https://github.com/gree/unity-webview/issues/189
                var js = @"
                    if (!(window.webkit && window.webkit.messageHandlers)) {
                        window.Unity = {
                            call: function(msg) {
                                var iframe = document.createElement('IFRAME');
                                iframe.setAttribute('src', 'unity:' + msg);
                                document.documentElement.appendChild(iframe);
                                iframe.parentNode.removeChild(iframe);
                                iframe = null;
                            }
                        };
                    }
                ";
#endif
#elif UNITY_WEBPLAYER || UNITY_WEBGL
                var js = @"
                    window.Unity = {
                        call:function(msg) {
                            parent.unityWebView.sendMessage('WebViewObject', msg);
                        }
                    };
                ";
#else
                var js = "";
#endif
                webViewObject.EvaluateJS(js + @"Unity.call('ua=' + navigator.userAgent)");
            }
            //transparent: false,
            //zoom: true,
            //ua: "custom user agent string",
            //radius: 0,  // rounded corner radius in pixel
            //// android
            //androidForceDarkMode: 0,  // 0: follow system setting, 1: force dark off, 2: force dark on
            //// ios
            //enableWKWebView: true,
            //wkContentMode: 0,  // 0: recommended, 1: mobile, 2: desktop
            //wkAllowsLinkPreview: true,
            //// editor
            //separated: false
            );
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.bitmapRefreshCycle = 1;
        webViewObject.devicePixelRatio = 1;  // 1 or 2
#endif
        // cf. https://github.com/gree/unity-webview/pull/512
        // Added alertDialogEnabled flag to enable/disable alert/confirm/prompt dialogs. by KojiNakamaru · Pull Request #512 · gree/unity-webview
        //webViewObject.SetAlertDialogEnabled(false);

        // cf. https://github.com/gree/unity-webview/pull/728
        //webViewObject.SetCameraAccess(true);
        //webViewObject.SetMicrophoneAccess(true);

        // cf. https://github.com/gree/unity-webview/pull/550
        // introduced SetURLPattern(..., hookPattern). by KojiNakamaru · Pull Request #550 · gree/unity-webview
        //webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

        // cf. https://github.com/gree/unity-webview/pull/570
        // Add BASIC authentication feature (Android and iOS with WKWebView only) by takeh1k0 · Pull Request #570 · gree/unity-webview
        //webViewObject.SetBasicAuthInfo("id", "password");

        //webViewObject.SetScrollbarsVisibility(true);

        webViewObject.SetMargins(12, 12, 12, 12);
        webViewObject.SetTextZoom(100);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410

#if !UNITY_WEBPLAYER && !UNITY_WEBGL
        if (Url.StartsWith("http"))
        {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        }
        else
        {
            var exts = new string[]{
                ".jpg",
                ".js",
                ".html"  // should be last
            };
            foreach (var ext in exts)
            {
                var url = Url.Replace(".html", ext);
                var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                var dst = System.IO.Path.Combine(Application.temporaryCachePath, url);
                byte[] result = null;
                if (src.Contains("://"))
                {  // for Android
#if UNITY_2018_4_OR_NEWER
                    // NOTE: a more complete code that utilizes UnityWebRequest can be found in https://github.com/gree/unity-webview/commit/2a07e82f760a8495aa3a77a23453f384869caba7#diff-4379160fa4c2a287f414c07eb10ee36d
                    var unityWebRequest = UnityWebRequest.Get(src);
                    yield return unityWebRequest.SendWebRequest();
                    result = unityWebRequest.downloadHandler.data;
#else
                    var www = new WWW(src);
                    yield return www;
                    result = www.bytes;
#endif
                }
                else
                {
                    result = System.IO.File.ReadAllBytes(src);
                }
                System.IO.File.WriteAllBytes(dst, result);
                if (ext == ".html")
                {
                    webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
                    break;
                }
            }
        }
#else
        if (Url.StartsWith("http")) {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        } else {
            webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
        }
#endif
        yield break;
    }
}
