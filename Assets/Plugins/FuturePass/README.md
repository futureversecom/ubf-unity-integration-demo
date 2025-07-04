# Unity FuturePass SDK

The **Futurepass SDK** is an Unity Engine plugin for authenticating users and managing access tokens with **Futurepass**—Futureverse’s identity and authentication system. It provides a seamless interface for custodia user login, token handling, and environment switching.

## ✨ Features

- 🔐 **Custodial Login** via browser-based authentication
- 🔄 **Access & Refresh Token** management
- 🌍 **Runtime Environment Switching** (Production/Staging)
- 🔗 **Token callback events** for login/logout/refresh handling

---

## 🧩 Installation - Package Manager via Git

1. In the Unity Package Manger, click '+' -> "Add package from git URL".
2. Paste the git url for this project and press "add"

## 🧩 Installation - Import .unitypackage

1. In the [Releases](https://github.com/futureversecom/sdk-unity-futurepass/releases) area of this repository, download the .unitypackage for your target version
2. Right-click in the Project window in the Unity engine, and select "Import Package -> Custom Package"
3. Navigate to your downloaded .unityproject file
4. Press "Import" on the next window
---

## 📄 API Reference

### FuturePassAuthentication
The primary source of functions and data when interacting with the FuturePass SDK
<details>
  <summary>Properties</summary>
  
  ```cs
  Environment CurrentEnvironment; // The current Futurepass environment (Development, Staging, or Production)
  ```
  ```cs
  CustodialAuthenticationResponse LoadedAuthenticationDetails; // The current
  ```

</details>
<details>
  <summary>Methods</summary>
  
  ```cs
  SetEnvironment (Environment environment); // Set Futurepass environment (Development, Staging, or Production)
  ```
  ```cs
  SetTokenAutoCache (bool cacheAutomatically); // Toggle whether refresh token is cached in PlayerPrefs
  ```
  ```cs
  StartLogin(Action onSuccess, Action<Exception> onFailure); // Begin the custodial authentication process
  ```
  ```cs
  AbortLogin(); // Cancel ongoing login, closing the web socket
  ```
  ```cs
  RefreshToken(); // Request a new authentication packet using loaded refresh token
  ```
  ```cs
  CacheRefreshToken(); // Encrypt and store loaded refresh token in PlayerPrefs
  ```
  ```cs
  CacheRefreshToken(string refreshToken, string passKey); // Encrypt and store provided refresh token using passKey as encryption pass-key
  ```
  ```cs
  LoginFromCachedRefreshToken(string passKey); // Load and decrypt cached refresh token, then request authentication
  ```

</details>

### FuturePassExecutor
An optional scene object to demonstrate runtime functionality and API usage.<br>
The inspector contains a text area to view loaded authentication data
<details>
  <summary>Properties</summary>

  ```cs
  FuturePassAuthentication‎.Environment environment; // Inspector enum field to set Futurepass environment
  ```
  ```cs
  bool cacheRefreshToken; // Inspector toggle whether to automatically cache refresh token
  ```
</details>
<details>
  <summary>Methods</summary>

  ```cs
  StartLogin(); // Begin the custodial authentication process
  ```
  ```cs
  AbortLogin(); // Cancel ongoing login, closing the web socket
  ```
  ```cs
  RefreshToken(); // Request a new authentication packet using loaded refresh token
  ```
  ```cs
  CacheRefreshToken(); // Encrypt and store loaded refresh token in PlayerPrefs
  ```
  ```cs
  LoginFromCachedRefreshToken(string passKey); // Load and decrypt cached refresh token, then request authentication
  ```
</details>

### CoroutineSceneObject
A utility object used to run coroutines from the static `FuturePassAuthentication` class
<details>
  <summary>Properties</summary>

  ```cs
  CoroutineSceneObject Instance; // Singleton reference to the scene object
  ```
</details>

### CustodialHttpListener
Simple HTTP listener implmenetation to receive callbacks from custodial web requests
<details>
  <summary>Properties</summary>

  ```cs
  CustodialHttpListener Instance; // Singleton reference to the listener object
  ```
  ```cs
  string ExpectedState; // The expected state value for validating CSRF protection
  ```
</details>
<details>
  <summary>Methods</summary>

  ```cs
  StartTokenAuthListener(Action<string,string,string> onAuthCodeReceived); // Create HttpListener and begin listening for callbacks. On receiving a valid packet, returns auth code details (authCode, state, ExpectedState)
  ```

  ```cs
  StopAuthTokenListener(); // Close HttpListener connection
  ```

  ```cs
  byte[] ConvertFromBase64String‎(string base64); // Convert a base64 string into a byte[]
  ```
</details>

## 🛠️ Getting Started: Using the Debug Functions

<img align="right" src="docs/sc-prefab.png" width=45%>
<p>Add the FuturePass prefab to your game scene</p>
<p>This prefab has buttons for each major function in this API</p>

<br>

<p>Begin a play session, and press the "Start Login" button on your FuturePassExecutor component</p>
<p>This will open a web browser, beginning your custodial authentication process</p>

<br>

<p>Once login is complete, your browser will redirect to a http://localhost:3000/callback page with text that says "You may now close this webpage!". At this point, you may return to Unity and again inspect your authentication component. </p>

<br>

<p>Upon a successful login, the text area below the debug buttons will now contain the details of your authentication. 
If you fail authentication, or want to cancel mid-process, you may use the "Abort Login" button to close the webserver listening for the login callback.</p>

<br>

<p>Once a token has been provisioned, you may ask for a 'refresh' in order to request a new token without running through the custodial flow again. 
  Press the "Refresh Token" button to trigger this process. After a second that the text area again updates with the new details of your authentication (you may need to mouse over the window to update its contents)</p>

<br>

<p>The "Cache Refresh Token" will encrpyt and cache the refresh token from your currently loaded authentication. You may cache up to one token per production environment.</p>

<br>

<p>The "Login From Cached Token" will load and decrypt this refresh token, and then perform the refresh flow to fetch a new authentication packet.</p>

<br>

<p>The refresh token is cached by default whenever you perform a custodial flow, or a refresh flow. You may disable this behaviour by un-ticking the "Cache Refresh Token" boolean.</p>
