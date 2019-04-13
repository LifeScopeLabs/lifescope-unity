using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Async;

public class LogIn : MonoBehaviour
{
    void login()
    {
        public const string LSurl = "https://app.lifescope.io/auth?";

        public const string clientID = "b47e68020f898a74"; 
        public const string clientSecret = "52570d6eccc4139f315079e0f5f030a7202b42fc7f90ae02c49fac3bdf0bb762"

        public const string LogInurl = LSurl + "client_id=" + clientID + ;

        var query = from LSLogin in ObservableWWW.Get(LogInurl)
                    select new { LSLogin };

        var cancel = query.Subscribe(x => Debug.Log(x.LSLogin.Substring(0, 100)));

        // Call Dispose is cancel downloading.
        cancel.Dispose();
    }
}
