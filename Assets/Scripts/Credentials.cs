using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Credentials
{
    public static Credentials current;
    public string OAuthToken;

    public Credentials() {
        this.OAuthToken = OAuthToken;
    }
}
