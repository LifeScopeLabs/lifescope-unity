using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class LifeScopePersonMod : MonoBehaviour
{
    public UnityEngine.UI.Text prefab;

    public void setTexture(Texture2D texture) {
        var avatar = gameObject.GetComponentInChildren<RawImage>();

        avatar.texture = texture;
    }

    public void setName(string FirstName, string MiddleName, string LastName) {
        var nameText = transform.Find("Canvas/NameText").GetComponent<Text>();

        var text = "";

        if (FirstName != null && FirstName.Length > 0) {
            text += FirstName;

            if (MiddleName != null && MiddleName.Length > 0) {
                text += " " + MiddleName;

                if (LastName != null && LastName.Length > 0) {
                    text += " " + LastName;
                }
            }
            else if (LastName != null && LastName.Length > 0) {
                text += " " + LastName;
            }
        }
        else if (MiddleName != null && MiddleName.Length > 0) {
            text += MiddleName;

            if (LastName != null && LastName.Length > 0) {
                text += " " + LastName;
            }
        }
        else if (LastName != null && LastName.Length > 0) {
            text += LastName;
        }

        nameText.text = text;
    }
}
