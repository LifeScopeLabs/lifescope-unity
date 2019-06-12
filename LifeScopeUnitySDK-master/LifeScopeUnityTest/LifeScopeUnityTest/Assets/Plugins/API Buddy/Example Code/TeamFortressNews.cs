using UnityEngine;
using System.Collections;
using PerspecDev;

public class TeamFortressNews : MonoBehaviour {

    // SteamResponseData reflects the Steam API JSON response, but only contains the fields we actually want to retrieve.
    // No need to include every JSON field that the Steam API provides here.  Just the ones we want.  These field names do
    // need to match the JSON response fields returned by the Steam API.  The class name can be whatever you want.
    // To better see how these fields match up, you should visit the API URL in your browser and compare with the fields in SteamResponseData:
    // http://api.steampowered.com/ISteamNews/GetNewsForApp/v0002/?appid=440&count=3&maxlength=300&format=json
    public class SteamResponseData : APIBuddy.WebResponseData {

        public AppNews appnews; // This matches the "appnews" element in the JSON response from the API.

        public class AppNews : APIBuddy.WebResponseData {
            public NewsItem[] newsitems; // This matches the "newsitems" array element in the JSON response.
        }

        public class NewsItem : APIBuddy.WebResponseData {
            // These match the "title," "contents," and "url" element in each news item in the JSON response.
            public string title;
            public string contents;
            public string url;
        }

    }

    private string _outputText = "";

    private void OnGUI() {
        if (GUI.Button(new Rect(20.0f, 20.0f, 200.0f, 50.0f), "Request Team Fortress news")) {
            RequestTeamFortressNews();
        }

        GUI.TextArea(new Rect(20.0f, 100.0f, 500.0f, 400.0f), _outputText);
    }

    private void RequestTeamFortressNews() {
        _outputText = "";

        // Simply pass in the URL for the API endpoint and wait for the response...
        string url = "http://api.steampowered.com/ISteamNews/GetNewsForApp/v0002/?appid=440&count=3&maxlength=300&format=json";
        APIBuddy.GetInstance().SendGetRequest<SteamResponseData>(url, 30.0f, (bool success, string errorMessage, int statusCode, object responseObject) => {
            if (success) {
                // APIBuddy handles populating SteamResponseData for us, based on the fields we set up in it up above.  All we need to do now is
                // cast responseObject to the SteamResponseData type and then read our data!
                SteamResponseData steamResponseData = (SteamResponseData)responseObject;

                _outputText = "";

                if (steamResponseData.appnews != null && steamResponseData.appnews.newsitems != null) {
                    foreach (SteamResponseData.NewsItem newsItem in steamResponseData.appnews.newsitems) {
                        _outputText += "==== " + newsItem.title + " ====";
                        _outputText += "\n(" + newsItem.url + ")";
                        _outputText += "\n" + newsItem.contents;
                        _outputText += "\n\n";
                    }
                }
            } else {
                _outputText = "Oops!  Couldn't Team Fortress news.  This demo won't work if you are targeting WebPlayer or have the Editor set to emulate WebPlayer security because api.steampowered.com does not have a crossdomain.xml file.  The IP to Location demo should still work because freegeoip.net does have a crossdomain.xml file that permits access from anywhere.\n" + errorMessage;
            }
        });
    }

}
