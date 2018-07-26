using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingSelection : MonoBehaviour 
{
    public Text option1Text;
    public Text option2Text;

    public void SelectOption1()
    {
        Color option1Color = Color.white;
        option1Color.a = 1.0f;
        option1Text.color = option1Color;

        Color option2Color = Color.white;
        option2Color.a = 0.3f;
        option2Text.color = option2Color;
    }

    public void SelectOption2()
    {
        Color option1Color = Color.white;
        option1Color.a = 0.3f;
        option1Text.color = option1Color;

        Color option2Color = Color.white;
        option2Color.a = 1.0f;
        option2Text.color = option2Color;
    }
}
