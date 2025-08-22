using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_2018_4_OR_NEWER
using UnityEngine.Networking;
#endif

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

public class ShopManager : MonoBehaviour, SocketEventListener
{
    public static ShopManager Instance;

    // --- WebView / UI ---
    private WebViewObject webViewObject;
    private GameObject webViewCanvas;
    private Button closeWebViewButton;

    // Reserve a top area so the native WebView (Android/WebGL) doesn't cover the close button.
    [SerializeField] private int topBarHeightDp = 94;

    // Track screen changes to reapply margins / reposition the close button
    private int _lastW, _lastH;
    private Rect _lastSafeArea;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        SocketController.Instance.AddListener(this);
        CreateWebViewCanvasWithCloseButton();

        webViewObject = new GameObject("WebViewObject").AddComponent<WebViewObject>();
        // Parenting is fine (for lifecycle), the plugin still renders natively/overlay.
        webViewObject.transform.SetParent(webViewCanvas.transform, false);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
        // Required by gree/unity-webview on Apple platforms
        webViewObject.canvas = webViewCanvas;
#endif
        // Initialize trackers
        _lastW = Screen.width;
        _lastH = Screen.height;
        _lastSafeArea = Screen.safeArea;
        PositionCloseButton(); // place X correctly considering safe area
    }

    // Create overlay canvas + close button
    private void CreateWebViewCanvasWithCloseButton()
    {
        webViewCanvas = new GameObject("WebViewCanvas");
        var canvas = webViewCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000; // ensure on top of other Unity UI
        webViewCanvas.AddComponent<CanvasScaler>(); // default: constant pixel size
        webViewCanvas.AddComponent<GraphicRaycaster>();

        // Close button
        GameObject closeBtnObj = new GameObject("CloseWebViewButton");
        closeBtnObj.transform.SetParent(webViewCanvas.transform, false);
        closeWebViewButton = closeBtnObj.AddComponent<Button>();
        var image = closeBtnObj.AddComponent<Image>();
        image.color = new Color(0.8f, 0f, 0f, 0.85f);

        RectTransform rect = closeBtnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.sizeDelta = new Vector2(60, 60);
        // anchored position set in PositionCloseButton() so we can include safe area

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(closeBtnObj.transform, false);
        var txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = "✖";
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontSize = 36;
        txt.color = Color.white;
        RectTransform txtRect = textObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        // Make sure the close button is last in hierarchy within this canvas
        closeBtnObj.transform.SetAsLastSibling();

        closeWebViewButton.onClick.AddListener(CloseWebView);
        webViewCanvas.SetActive(false);
    }

    private void ShowWebViewCanvas(bool show)
    {
        if (webViewCanvas != null)
            webViewCanvas.SetActive(show);
    }

    private void CloseWebView()
    {
        if (webViewObject != null)
            webViewObject.SetVisibility(false);
        ShowWebViewCanvas(false);
    }

    // --- Safe area helpers / margins ---

    private int GetSafeTopPx()
    {
#if UNITY_ANDROID || UNITY_IOS
        var sa = Screen.safeArea;
        // Distance from top edge to safe area top in pixels:
        return Mathf.RoundToInt(Screen.height - sa.yMax);
#else
        return 0;
#endif
    }

    private void PositionCloseButton()
    {
        if (closeWebViewButton == null) return;
        var rect = closeWebViewButton.GetComponent<RectTransform>();

        int safeTop = GetSafeTopPx();
        // Keep ~20px from edges, add safe area on top (so it's not under the notch/status bar)
        rect.anchoredPosition = new Vector2(-20f, -(20f + safeTop));
    }

    private void ApplyWebViewMargins()
    {
        if (webViewObject == null) return;

        // Convert dp to pixels (approx; Unity doesn't expose dp)
        float scale = (Screen.dpi > 0f) ? (Screen.dpi / 160f) : 1f;
        int topBarPx = Mathf.RoundToInt(topBarHeightDp * scale);

        int safeTop = GetSafeTopPx();

        // Left, Top, Right, Bottom
        webViewObject.SetMargins(0, topBarPx + safeTop, 0, 0);
    }

    private void LateUpdate()
    {
        // If resolution or safe area changes (rotation, resize), reapply
        if (_lastW != Screen.width || _lastH != Screen.height || _lastSafeArea != Screen.safeArea)
        {
            _lastW = Screen.width;
            _lastH = Screen.height;
            _lastSafeArea = Screen.safeArea;

            PositionCloseButton();
            ApplyWebViewMargins();
        }
    }

    [Header("Detail Panel UI")]
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
                ShowWebViewCanvas(true);
                _loadCoroutine = StartCoroutine(LoadWebView(message));
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

    public void ReactivateDetailPanel()
    {
        DetailsBuyButtonPressed();
    }

    public void FetchShopData()
    {
        API_Manager.Instance.GetShopData(GetAllShopData);
    }

    // Load the page and make the webview visible
    private IEnumerator LoadWebView(string Url)
    {
        webViewObject.Init(
            cb: (msg) => { },
            err: (msg) => { },
            httpErr: (msg) => { },
            started: (msg) => { },
            hooked: (msg) => { },
            cookies: (msg) => { },
            ld: (msg) =>
            {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
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
                var js = "";
#endif
                webViewObject.EvaluateJS(js + @"Unity.call('ua=' + navigator.userAgent)");
            }
        );

        webViewObject.SetTextZoom(100);
        ApplyWebViewMargins(); // <<< reserve the top area so the X is always visible

#if !UNITY_WEBPLAYER && !UNITY_WEBGL
        if (Url.StartsWith("http"))
        {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        }
        else
        {
            var exts = new string[] { ".jpg", ".js", ".html" };
            foreach (var ext in exts)
            {
                var url = Url.Replace(".html", ext);
                var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                var dst = System.IO.Path.Combine(Application.temporaryCachePath, url);
                byte[] result = null;
                if (!src.Contains("://"))
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
        if (Url.StartsWith("http"))
        {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        }
        else
        {
            webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
        }
#endif

        yield return null;

        webViewObject.SetVisibility(true);
        // Reapply margins in case resolution changed between init and first frame
        ApplyWebViewMargins();
    }

    public void RemoveListener()
    {
        SocketController.Instance.RemoveListener(this);
    }

    private void OnDestroy()
    {
        RemoveListener();
    }

    void SocketEventListener.OnSocketMessageReceived(SocketEventsType messageHeader, string payLoad)
    {
        if (messageHeader == SocketEventsType.paymentComplete)
        {
            Debug.Log("Payment completed successfully.");
            GlobalCanvasManager.Instance.LoadingPanel.HidePopup();
            GlobalCanvasManager.Instance.PopUIHandler.ShowPopup(new PopupData()
            {
                titleString = "Success",
                contentString = "Your purchase was successful!",
                firstButtonString = "OK",
                firstButtonCallBack = HidePopUp
            });
            CloseWebView();
        }
        else if (messageHeader == SocketEventsType.paymentFailed)
        {
            Debug.Log("Payment failed.");
            GlobalCanvasManager.Instance.LoadingPanel.HidePopup();
            GlobalCanvasManager.Instance.PopUIHandler.ShowPopup(new PopupData()
            {
                titleString = "Error",
                contentString = "Payment failed. Please try again.",
                firstButtonString = "OK",
                firstButtonCallBack = HidePopUp
            });
            CloseWebView();
        }
    }

    // Optional: back key closes the webview on Android
    private void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (webViewCanvas != null && webViewCanvas.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWebView();
        }
#endif
    }

    //  void OnGUI()
    // {
    //     var x = 10;

    //     GUI.enabled = (webViewObject == null) ? false : webViewObject.CanGoBack();
    //     if (GUI.Button(new Rect(x, 10, 80, 80), "<")) {
    //         webViewObject?.GoBack();
    //     }
    //     GUI.enabled = true;
    //     x += 90;

    //     GUI.enabled = (webViewObject == null) ? false : webViewObject.CanGoForward();
    //     if (GUI.Button(new Rect(x, 10, 80, 80), ">")) {
    //         webViewObject?.GoForward();
    //     }
    //     GUI.enabled = true;
    //     x += 90;

    //     if (GUI.Button(new Rect(x, 10, 80, 80), "r")) {
    //         webViewObject?.Reload();
    //     }
    //     x += 90;

    //     GUI.TextField(new Rect(x, 10, 180, 80), "" + ((webViewObject == null) ? 0 : webViewObject.Progress()));
    //     x += 190;

    //     if (GUI.Button(new Rect(x, 10, 80, 80), "*")) {
    //         var g = GameObject.Find("WebViewObject");
    //         if (g != null) {
    //             Destroy(g);
    //         } else {
    //             StartCoroutine(Start());
    //         }
    //     }
    //     x += 90;

    //     if (GUI.Button(new Rect(x, 10, 80, 80), "c")) {
    //         webViewObject?.GetCookies(Url);
    //     }
    //     x += 90;

    //     if (GUI.Button(new Rect(x, 10, 80, 80), "x")) {
    //         webViewObject?.ClearCookies();
    //     }
    //     x += 90;

    //     if (GUI.Button(new Rect(x, 10, 80, 80), "D")) {
    //         webViewObject?.SetInteractionEnabled(false);
    //     }
    //     x += 90;

    //     if (GUI.Button(new Rect(x, 10, 80, 80), "E")) {
    //         webViewObject?.SetInteractionEnabled(true);
    //     }
    //     x += 90;
    // }
}