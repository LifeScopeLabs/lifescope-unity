// GetAccessToken function:

[Serializable]
public class TokenClassName
{
    public string access_token;
}

private static IEnumerator GetAccessToken(Action<string> result)
{
    Dictionary<string, string> content = new Dictionary<string, string>();
    //Fill key and value
    content.Add("grant_type", "client_credentials");
    content.Add("client_id", "login-secret");
    content.Add("client_secret", "secretpassword");

    UnityWebRequest www = UnityWebRequest.Post("https://someurl.com//oauth/token", content);
    //Send request
    yield return www.Send();

    if (!www.isError)
    {
        string resultContent = www.downloadHandler.text;
        TokenClassName json = JsonUtility.FromJson<TokenClassName>(resultContent);

        //Return result
        result(json.access_token);
    }
    else
    {
        //Return null
        result("");
    }
}
// GetMeasurements function:

[Serializable]
public class MeasurementClassName
{
    public string Measurements;
}

private static IEnumerator GetMeasurements(string id, DateTime from, DateTime to, Action<string> result)
{
    Dictionary<string, string> content = new Dictionary<string, string>();
    //Fill key and value
    content.Add("MeasurePoints", id);
    content.Add("Sampling", "Auto");
    content.Add("From", from.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    content.Add("To", to.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    content.Add("client_secret", "secretpassword");

    UnityWebRequest www = UnityWebRequest.Post("https://someurl.com/api/v2/Measurements", content);

    string token = null;

    yield return GetAccessToken((tokenResult) => { token = tokenResult; });

    www.SetRequestHeader("Authorization", "Bearer " + token);
    www.Send();

    if (!www.isError)
    {
        string resultContent = www.downloadHandler.text;
        MeasurementClassName[] rootArray = JsonHelper.FromJson<MeasurementClassName>(resultContent);

        string measurements = "";
        foreach (MeasurementClassName item in rootArray)
        {
            measurements = item.Measurements;
        }

        //Return result
        result(measurements);
    }
    else
    {
        //Return null
        result("");
    }
}

// Usage:

string id = "";
DateTime from = new DateTime();
DateTime to = new DateTime();

StartCoroutine(GetMeasurements(id, from, to, (measurementResult) =>
{
    string measurement = measurementResult;

    //Do something with measurement
    UnityEngine.Debug.Log(measurement);

}));