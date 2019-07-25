using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class GridLayoutScript : MonoBehaviour
{
    public UnityEngine.UI.Text prefab;

    public void populateContacts(Newtonsoft.Json.Linq.JToken Contacts) {
        Newtonsoft.Json.Linq.JArray contacts = (Newtonsoft.Json.Linq.JArray)Contacts;

        for (int i = 0; i < contacts.Count; i++) {
            var contact = contacts[i];

            var instance = (UnityEngine.UI.Text)Instantiate(prefab, transform);

            instance.fontSize = 30;

            var text = "";

            if (contact["handle"] != null && contact["handle"].ToString().Length > 0) {
                text += contact["handle"];
            }
            else {
                text += contact["name"];
            }

            text += " (" + contact["hydratedConnection"]["provider"]["name"] + ")";

            instance.text = text;
        }
    }
}
