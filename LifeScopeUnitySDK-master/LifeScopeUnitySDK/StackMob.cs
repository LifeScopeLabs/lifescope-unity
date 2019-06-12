using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System;
using System.Text;

public class StackMob : MonoBehaviour
{
	public const string acceptHeader = "application/vnd.stackmob+json; version=0"; // 0 = development, 1 = production
	public const string apiKey = "e78a2646-d479-47b1-9ae5-65732c5eabdc"; // nixApp
	
	private string accessToken = ""; // AccessToken
	private string macKey = ""; // HashKey
	
	// Entry point
	void Start ()
	{
		StartCoroutine (QueryAccessToken ("norris", "myfists"));
	}
	
	// Query Access Token Coroutine
	IEnumerator QueryAccessToken (string name, string password)
	{
		// Populate HTML request
		HTTP.Request request = new HTTP.Request ("POST", "http://api.stackmob.com/user/accessToken");
		request.Text = string.Format("username={0}&password={1}&token_type=mac", name, password);
		
		// Add request headers
		request.AddHeader ("Content-Type", "application/x-www-form-urlencoded");
		request.AddHeader ("Accept", acceptHeader);
		request.AddHeader ("X-StackMob-API-Key", apiKey);
		request.AddHeader ("X-StackMob-User-Agent", "StackMob Platform");
		
		// Send request
		request.Send ();
		
		// Yield coroutine until request returns
		while(!request.isDone)
			yield return null;
		
		// Dump request response to debug console
		Debug.Log (request.response.Text);
		
		// Parse JSON object
		Hashtable table = JSON.JsonDecode (request.response.Text) as Hashtable;
		
		// Store Mac Key & Access Token
		accessToken = GetString ("access_token", table);
		macKey = GetString ("mac_key", table);
		
		// Query a schema with permissions enabled (ie. Allow any logged in user)
		StartCoroutine (Query ("GET", "http://api.mob1.stackmob.com/safe"));
	}
	
	// General Query Coroutine
	IEnumerator Query (string method, string url)
	{
		// Populate HTML request
		HTTP.Request request = new HTTP.Request (method, url);
		
		// Add request headers
		request.AddHeader ("Accept", acceptHeader);
		request.AddHeader ("X-StackMob-API-Key", apiKey);
		request.AddHeader ("X-StackMob-User-Agent", "StackMob Platform");
		request.AddHeader ("Authorization", GenerateAuthorizationHeader (method, url));
		
		// Send request
		request.Send ();
		
		// Yield coroutine until request returns
		while(!request.isDone)
			yield return null;
		
		// Dump request response to debug console
		Debug.Log (request.response.Text);
	}
	
	// Helper function for JSON hashtable
	string GetString (string key, Hashtable table)
	{
		if (table.ContainsKey(key))
			return table[key] as string;
		return "";
	}
	
	// Helper function for JSON hashtable
	Hashtable GetTable (string key, Hashtable table)
	{
		if (table.ContainsKey(key))
			return table[key] as Hashtable;
		return new Hashtable ();
	}
	
	// Generate Authorisation header as described at https://gist.github.com/f5e8dc879f506c9a0268
	string GenerateAuthorizationHeader (string method, string url)
	{		Uri uri = new Uri (url);

		string timestamp = Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString ();
		string nonce = Guid.NewGuid ().ToString ().Replace ("-", "").Substring (0, 10);
		string request = uri.PathAndQuery;
		string host = uri.Host;
		int port = uri.Port;
		
		// Verify port
		if (port < 0)
		{
			if (url.StartsWith ("https:", StringComparison.InvariantCultureIgnoreCase))
				port = 443;
			else
				port = 80;
		}
				
		// Populate normailised request string
		char newline = '\n';
		StringBuilder builder = new StringBuilder ();
		builder.Append (timestamp);
		builder.Append (newline);
		builder.Append (nonce);
		builder.Append (newline);
		builder.Append (method.ToUpper ());
		builder.Append (newline);
		builder.Append (request);
		builder.Append (newline);
		builder.Append (host);
		builder.Append (newline);
		builder.Append (port.ToString ());
		builder.Append (newline);
		builder.Append (newline);
		
		string normailisedRequest = builder.ToString ();
		
		// Dump normailisedRequest to debug console
		Debug.Log ("normalised request : '" + normailisedRequest + "'");
		
		// Sign with mac key
		KeyedHashAlgorithm hmac = new HMACSHA1 (GetBytes (macKey));
		string mac = ToBase64 (hmac.ComputeHash (GetBytes (normailisedRequest)));
		
		// Format header (should not have any whitespace in the string)
		string header = string.Format ("MAC id=\"{0}\",ts=\"{1}\",nonce=\"{2}\",mac=\"{3}\"", accessToken, timestamp, nonce, mac);

		// Dump header to debug console
		Debug.Log ("Authorization : '" + header + "'");
		
		return header;
	}
	
	private static byte[] GetBytes (string input)
	{
		return UTF8Encoding.UTF8.GetBytes (input);
	}
	
	private static string ToBase64(byte[] input)
	{
		return Convert.ToBase64String(input);
	}
}