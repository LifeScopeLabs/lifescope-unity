Thanks for purchasing API Buddy!  I hope you find it helpful and easy to use.

It's quick and easy to get up and running.  Instead of having to parse JSON into an object yourself,
you simply create a class that matches the definition of the JSON response you expect to receive.
Then you pass that class into one of APIBuddy's Send methods, and it pulls down the data, parses
it, and populates your custom object.

Here's an example:

// First, create a class that matches the JSON API response.
public class GeoResponseData : APIBuddy.WebResponseData {
    public string ip;
    public string country_name;
    public string region_name;
    public string city;
}

Then...

// Simply pass in the URL for the API endpoint and wait for the response...
string url = "http://freegeoip.net/json/";
APIBuddy.GetInstance().SendGetRequest<GeoResponseData>(url, 30.0f, (bool success, int statusCode, object responseObject) => {
    if (success) {
        // APIBuddy handles populating GeoResponseData for us, based on the fields we set up in it up above.  All we need to do now is
        // cast responseObject to the GeoResponseData type and then read our data!
        GeoResponseData geoResponseData = (GeoResponseData)responseObject;

        _outputText = "IP address: " + geoResponseData.ip;
        _outputText += "\nCountry: " + geoResponseData.country_name;
        _outputText += "\nRegion: " + geoResponseData.region_name;
        _outputText += "\nCity: " + geoResponseData.city;
    } else {
        _outputText = "Oops!  Couldn't retrieve IP geolocation info.";
    }
});

It's that simple!  No juggling WWW objects or worrying about parsing JSON text.  And of course,
APIBuddy can handle multi-level JSON responses.  Check out the example scenes and scripts and
you'll see how easy it is.



==== Extending APIBuddy to handle non-JSON API responses ====

While APIBuddy works easiest out of the box with JSON data, it can also handle other types of data.
If you need to work with data that's not in JSON format, you can write your own parser to handle that
data.  To do so, you would call APIBuddy's SendWebRequest method (instead of SendGetRequest or
SendPostRequest).  SendWebRequest has a webResponsePopulateDelegate parameter, and that's where you'll
pass in your custom API response parser.  Your delegate will be passed the raw WWW object received from
the server response, and will be expected to populate a response object and then return a bool indicating
whether the API request was successful or not.

You can take a look at DefaultJSONPopulateDelegate in APIBuddy.cs to see how this was done for JSON.
DefaultJSONPopulateDelegate populates your custom class using C# reflection.  It determines which fields
you're interested in based on the definition of your custom class, and it creates an instance of your
custom class and populates it with the appropriate elements from the JSON response.

You do not necessarily have to populate a custom class object using C# reflection.  Since you know ahead
of time how your custom class is defined, you could simply read the data you need from the WWW object
and then create an instance of your class and populate its fields directly without reflection.


==== Bonus: OAuth 2.0 ====

I've included an example OAuth 2.0 implementation.  OAuth 2.0 authentication happens in a web browser,
and we need some way to get the auth token from the web browser into the Unity app.  Since we can't
reasonably read data directly from the web browser, the OAuth 2.0 implementation you'll find here
actually starts up a very basic web server with one purpose: to receive the OAuth 2.0 authorization token.

A note about iOS and Android:
Starting up a web server in this way is not something we can do on a mobile device, so if your app runs
on iOS or Android, you'll skip the web server and register a custom URL scheme instead.  You'll
find comments in InstagramClient.cs that give an overview of this process for iOS and Android.

Each OAuth provider passes the auth token a little differently, so you'll want to create a subclass
of OAuthCallbackServer for each OAuth provider you need to communicate with.  InstagramCallbackServer.cs
is an example of that.  In a subclass of OAuthCallbackServer, there are two methods you'll need to
override:

ShouldRedirectURIFragmentToQueryString()
^^^^ Implement this to specify whether or not to put the URI fragment (hashtag) into a query string and
redirect the user to a URL with that query string.  This is almost always necessary if the OAuth
provider returns the auth token in the URI fragment because the browser will not pass the fragment to
the server (which, in this case, is us).  The redirection will be in the format:
http://127.0.0.1:[port]/?uri_fragment=[fragment data]

ProcessListenerContext(HttpListenerContext context)
^^^^ Implement this and check the context to see if the auth token is in it.  This method will probably
be called more than once.

When the auth token has been successfully retrieved, you need to pass it to the NotifyAuthTokenReceived()
method.  The auth token will the be sent to the callback you provided when you called the StartListening()
method on your OAuthCallbackServer subclass.  Once you have the auth token, you can pass it to one of
APIBuddy's Send methods.  See InstagramClient.cs for an example of the whole Auth 2.0 flow.


==== SimpleJSON ====

API Buddy includes SimpleJSON.cs.  If your project already includes SimpleJSON.cs, feel free to delete
it from the "API Buddy/Third Party" folder.  I've only made one change to SimpleJSON.cs, and if you're
already using SimpleJSON.cs in your project, you may not need the change I made.  The change I made
fixes an issue where if a JSON response has a "null" value (without the quotes), SimpleJSON may throw
errors and fail to parse the JSON response.  You'll find the fix on line 261 of SimpleJSON.cs:

            if (token.Equals("null")) {
                return new JSONData("null");
            }



Thanks again for purchasing API Buddy!  Send me an email at daniel@perspecdev.com if you have
any problems with it.

Sincerely,
-Daniel Isenhower