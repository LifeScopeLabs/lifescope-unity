using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using GraphQL.Client;
using GraphQL.Common.Request;


public class LifeScopeDemoBasic : MonoBehaviour {
    public string oauthClientId = "e33af1dc0124dbf5"; // Put your LifeScope OAuth client ID here.

    public Button LoginButton;
    public Button SubmitCodeButton;
    public Button LogoutButton;
    public Canvas LoginCanvas;
    public Canvas TokenCanvas;
    public Canvas FinishCanvas;

    public string[] inputScopes = new string[] {
        "basic"
    };

    private string _accessCode = "";
    
    static string _exchangeBackendUrl = "https://unity.lifescope.io"; //You should enter your own setup's exchange backend URL here.
    static HttpClient exchangeClient = new HttpClient();
    static GraphQLClient graphQLClient = new GraphQLClient("https://api.lifescope.io/gql");

    private class _OAuthTokenResponse {
        public string oauth_token { get; set; }
    }

    void Start() {
        LoginButton.onClick.AddListener(LoginListener);
        SubmitCodeButton.onClick.AddListener(SubmitCodeListener);
        LogoutButton.onClick.AddListener(LogoutListener);

        SaveLoad.Load();

        LoginCanvas.enabled = false;
        TokenCanvas.enabled = false;
        FinishCanvas.enabled = false;

        if (SaveLoad.credentials != null && SaveLoad.credentials.OAuthToken != null && SaveLoad.credentials.OAuthToken.Length == 64) {
            getUserId();
            FinishCanvas.enabled = true;
        }
        else {
            LoginCanvas.enabled = true;
        }
    }

    void LoginListener() {
        LifeScopeLogin.setScopes(inputScopes);
        LifeScopeLogin.launchAuth();

        LoginCanvas.enabled = false;
        TokenCanvas.enabled = true;
    }

    async void SubmitCodeListener() {
        try {
            _accessCode = GameObject.Find("CodeInputField").GetComponent<InputField>().text;
            HttpResponseMessage response = await exchangeClient.GetAsync(_exchangeBackendUrl + "/exchange-code?access_code=" + _accessCode);

            _accessCode = "";

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<_OAuthTokenResponse>(responseBody);

            SaveLoad.credentials.OAuthToken = jsonResponse.oauth_token;
            SaveLoad.Save();

            TokenCanvas.enabled = false;
            FinishCanvas.enabled = true;

            getUserId();
        }
        catch(HttpRequestException e) {
            Debug.Log("Access code exchange error:");
            Debug.Log(e.Message);
        }
    }

    async void getUserId() {
        GraphQLRequest userRequest = new GraphQLRequest{
            Query = @"
                query userBasic {
                    userBasic {
                        id
                    }
                }"
        };

        graphQLClient.DefaultRequestHeaders.Add("authorization", "Bearer " + SaveLoad.credentials.OAuthToken);
        var graphQLResponse = await graphQLClient.PostAsync(userRequest);

        if (graphQLResponse.Errors != null) {
            Debug.Log(JsonConvert.SerializeObject(graphQLResponse.Errors));

            TokenCanvas.enabled = false;
            FinishCanvas.enabled = false;
            LoginCanvas.enabled = true;
        }
        else {
            GameObject.Find("IDText").GetComponent<Text>().text = "Your LifeScope ID is " + graphQLResponse.Data.userBasic.id;
        }
    }

    void LogoutListener() {
        SaveLoad.credentials.OAuthToken = "";
        SaveLoad.Save();

        TokenCanvas.enabled = false;
        FinishCanvas.enabled = false;
        LoginCanvas.enabled = true;
    }
}
