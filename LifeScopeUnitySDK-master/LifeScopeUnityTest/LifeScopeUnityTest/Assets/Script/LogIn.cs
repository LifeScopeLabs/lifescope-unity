using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UniRx;
// using UniRx.Async;
using PerspecDev; // APIBuddy;

public class LogIn : MonoBehaviour
{

    public class LSAuthResponseData : APIBuddy.WebResponseData {
        public string code;
        public string state;
    }
    void login()

    {   
        const string LSurl = "https://app.lifescope.io/auth?";

        const string clientID = "b47e68020f898a74"; 
        const string clientSecret = "52570d6eccc4139f315079e0f5f030a7202b42fc7f90ae02c49fac3bdf0bb762";
        const string redirect_uri = "";
        const string scope = "";
        const string response_type = "code";

        const string LogInurl = LSurl + "client_id=" + clientID + "&redirect_uri=" + redirect_uri +
            "&scope=" + scope + "&response_type=" + response_type;

        APIBuddy.GetInstance().SendGetRequest<LSAuthResponseData>(LogInurl, 30.0f, (bool success, string errorMsg, int statusCode, object responseObject) => {
            if (success) {
                LSAuthResponseData lsAuthResponseData = (LSAuthResponseData)responseObject;
            } else {
                Console.WriteLine(errorMsg);
            }
        });

    }
}