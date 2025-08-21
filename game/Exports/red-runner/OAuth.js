var gameInstance = null;
// Function to open the Google OAuth sign-in window
function signInWithGoogle(myclientId, myredirectUri) {
    console.log("This is working");
    const GOOGLE_OAUTH_AUTHORIZE_URL = "https://accounts.google.com/o/oauth2/v2/auth";
    const RESPONSE_TYPE = 'code';
    const SCOPE = 'email profile';
    const clientId=myclientId;
    const redirectUri=myredirectUri;
    const authorizeUrl = `${GOOGLE_OAUTH_AUTHORIZE_URL}?redirect_uri=${encodeURIComponent(redirectUri)}&client_id=${clientId}&response_type=${RESPONSE_TYPE}&scope=${encodeURIComponent(SCOPE)}`;
    // Open a new popup window for the Google login
    var popup = window.open(authorizeUrl, 'googleLogin', 'width=500,height=600');

    // Function to monitor the popup state
    var interval = setInterval(function() {
        try {
            // Check if the popup was closed
            if (popup.closed) {
                clearInterval(interval);
                alert('Authentication popup closed by user');
                return;
            }

            // Try to access the popup's URL
            var popupUrl = popup.location.href;

            // Check if the popup URL is the redirect URI
            if (popupUrl.indexOf(redirectUri) === 0) {
                clearInterval(interval);
                var code = new URL(popupUrl).searchParams.get('code');
                if (code) {
                    console.log('Authorization code:', code);

                    // fetch token and then profile using firebase rest api 

                    // You can now close the popup and use the authorization code in the main window
                    popup.close();
                  //  authenticateWithGoogleToken(code);
                    // Example: Call a function to handle the authorization code
                    sendAuthCodeToUnity(code);
                }
            }
        } catch (e) {
            // Error handling, typically for cross-origin requests
        }
    }, 100);
}

// Function to be called from Unity to start the OAuth flow
function startGoogleAuth(clientId, redirectUri) {
    signInWithGoogle(clientId, redirectUri);
}
// This function can be called anytime to send the auth code to Unity, if available
function sendAuthCodeIfAvailable() {
    var queryParams = new URLSearchParams(window.location.search);
    if (queryParams.has('code')) {
        var authCode = queryParams.get('code');
        sendAuthCodeToUnity(authCode);
        // Clear the code from the URL to avoid re-sending it
        history.pushState(null, '', location.pathname);
    }
}
document.addEventListener('UnityReady', function() {
    // Now you can safely use unityGameInstance
    console.log(unityGameInstance); // Example usage
    gameInstance = unityGameInstance;
    // Additional code that depends on unityGameInstance
});
// Function to pass the authorization code to Unity
function sendAuthCodeToUnity(authCode) {
    // Assuming you have a Unity object named 'gameInstance'
    if (gameInstance) {
        gameInstance.SendMessage('API_Manager', 'OnGoogleSignIn', authCode);
    }else {
        console.error("Unity game instance is not yet initialized.");
      }
}

function startTwitterAuth(clientId, redirectUri) {
    const TWITTER_OAUTH_AUTHORIZE_URL = 'https://twitter.com/i/oauth2/authorize';
    const RESPONSE_TYPE = 'code';
    const SCOPE = 'tweet.read users.read offline.access'; // Include other scopes you might need
    const STATE = "state"; // You should generate a random string for state
    const CODE_CHALLENGE = generateCodeChallenge(); // Generate your code challenge here
    const CODE_CHALLENGE_METHOD = 'S256'; // Assuming you're using SHA-256

    // Construct the full authorization URL
    const authorizeUrl = `${TWITTER_OAUTH_AUTHORIZE_URL}?` +
        `response_type=${encodeURIComponent(RESPONSE_TYPE)}` +
        `&client_id=${encodeURIComponent(clientId)}` +
        `&redirect_uri=${encodeURIComponent(redirectUri)}` +
        `&scope=${encodeURIComponent(SCOPE)}` +
        `&state=${encodeURIComponent(STATE)}` +
        `&code_challenge=${encodeURIComponent(CODE_CHALLENGE)}` +
        `&code_challenge_method=${encodeURIComponent(CODE_CHALLENGE_METHOD)}`;

    console.log('Redirecting to:', authorizeUrl);

    var popup = window.open(authorizeUrl, 'twitterLogin', 'width=500,height=600');

    // Function to monitor the popup state
    var interval = setInterval(function() {
        try {
		var popupUrl = popup.location.href;
		console.log("popupUrl",popupUrl);
            // Check if the popup was closed
            if (popup.closed) {
                clearInterval(interval);
                alert('Authentication popup closed by user');
                return;
            }

            

            // Check if the popup URL is the redirect URI
            if (popupUrl.indexOf(redirectUri) === 0) {
                clearInterval(interval);
                var code = new URL(popupUrl).searchParams.get('code');
                if (code) {
                    console.log('Authorization code:', code);
                    // You can now close the popup and use the authorization code in the main window
                    popup.close();

                    // Example: Call a function to handle the authorization code
                    sendAuthDataToUnity(code);
                }
            }
        } catch (e) {
            // Error handling, typically for cross-origin requests
        }
    }, 100);
}

function generateRandomState() {
    // Simple example of state generation
    return Math.random().toString(36).substring(2, 15);
}
function generateCodeChallenge() {
    // Implement your code challenge generation logic here (PKCE)
    return 'y_SfRG4BmOES02uqWeIkIgLQAlTBggyf_G7uKT51ku8'; // Replace this with the actual code challenge
}
// Call this function from the server-side once Twitter redirects to your callback URL
// and after you've exchanged the code for an access token
function sendAuthDataToUnity(accessToken) {
    // Assuming you have a Unity object named 'gameInstance'
    gameInstance.SendMessage('API_Manager', 'OnGoogleSignIn', accessToken);
}
function copyToClipboard(text) {
    let str = typeof text === 'number' ? utf8ToString(text) : text;

    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text).then(() => {
            console.log('Text copied to clipboard');
        }).catch(err => {
            console.error('Failed to copy text: ', err);
        });
    } else {
        fallbackCopyTextToClipboard(text);
        console.warn('Clipboard API not available');
    }
}

function utf8ToString(ptr) {
    let str = '';
    let u8Array = new Uint8Array(Module.HEAPU8.buffer, ptr);
    let idx = 0;
    while (u8Array[idx]) {
        str += String.fromCharCode(u8Array[idx++]);
    }
    return decodeURIComponent(escape(str));
}
function fallbackCopyTextToClipboard(text) {
    // Create a temporary <textarea> element
    const textArea = document.createElement('textarea');
    textArea.value = text;

    // Prevent scrolling to the bottom of the page in some browsers
    textArea.style.position = 'fixed';
    textArea.style.top = 0;
    textArea.style.left = 0;

    // Append the textarea to the document body
    document.body.appendChild(textArea);

    // Focus the textarea and select its content
    textArea.focus();
    textArea.select();

    try {
        // Execute the copy command
        const successful = document.execCommand('copy');
        const msg = successful ? 'successful' : 'unsuccessful';
        console.log('Fallback: Copying text command was ' + msg);
    } catch (err) {
        console.error('Fallback: Unable to copy', err);
    }

    // Remove the textarea from the document body
    document.body.removeChild(textArea);
}
function SaveImageToIndexedDB(key, base64Data) {
    var dbRequest = indexedDB.open("UnityImageStorage", 1);

    dbRequest.onupgradeneeded = function(event) {
        var db = event.target.result;
        if (!db.objectStoreNames.contains('images')) {
            db.createObjectStore('images');
        }
    };

    dbRequest.onsuccess = function(event) {
        var db = event.target.result;
        var transaction = db.transaction('images', 'readwrite');
        var store = transaction.objectStore('images');
        store.put(base64Data, key);
    };
}

function LoadImageFromIndexedDB(key, callback) {
    var dbRequest = indexedDB.open("UnityImageStorage", 1);

    dbRequest.onsuccess = function(event) {
        var db = event.target.result;
        var transaction = db.transaction('images', 'readonly');
        var store = transaction.objectStore('images');
        var getRequest = store.get(key);

        getRequest.onsuccess = function() {
            callback(getRequest.result);
        };
    };
}
