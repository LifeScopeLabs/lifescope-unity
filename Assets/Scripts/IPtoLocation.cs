using UnityEngine;
using System.Collections;
using PerspecDev;

public class IPtoLocation : MonoBehaviour {

    // GeoResponseData reflects the freegioip.net JSON response, but only contains the fields we actually want to retrieve.
    // No need to include every JSON field that freegioip.net provides here.  Just the ones we want.  These field names do
    // need to match the JSON response fields returned by freegioip.net.  The class name can be whatever you want.
    // To better see how these fields match up, you should visit the API URL in your browser and compare with the fields in GeoResponseData:
    // http://freegeoip.net/json/
    public class GeoResponseData : APIBuddy.WebResponseData {
        // These fields match the "ip," "country_name," "region_name," and "city" elements in the JSON response from the API.
        public string ip;
        public string country_name;
        public string region_name;
        public string city;
    }
    
    private string _outputText = "";

	private void OnGUI() {
        if (GUI.Button(new Rect(20.0f, 20.0f, 200.0f, 50.0f), "Request IP geolocation info")) {
            RequestIPGeoLocationInfo();
        }

        GUI.TextArea(new Rect(20.0f, 100.0f, 500.0f, 400.0f), _outputText);
    }

    private void RequestIPGeoLocationInfo() {
        _outputText = "";

        // Simply pass in the URL for the API endpoint and wait for the response...
        string url = "http://freegeoip.net/json/";
        APIBuddy.GetInstance().SendGetRequest<GeoResponseData>(url, 30.0f, (bool success, string errorMessage, int statusCode, object responseObject) => {
            if (success) {
                // APIBuddy handles populating GeoResponseData for us, based on the fields we set up in it up above.  All we need to do now is
                // cast responseObject to the GeoResponseData type and then read our data!
                GeoResponseData geoResponseData = (GeoResponseData)responseObject;

                _outputText = "IP address: " + geoResponseData.ip;
                _outputText += "\nCountry: " + geoResponseData.country_name;
                _outputText += "\nRegion: " + geoResponseData.region_name;
                _outputText += "\nCity: " + geoResponseData.city;
            } else {
                _outputText = "Oops!  Couldn't retrieve IP geolocation info.\n" + errorMessage;
            }
        });
    }

}
