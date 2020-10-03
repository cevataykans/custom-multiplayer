using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    private void Awake()
    {
        if ( instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public GameObject startMenu;
    public InputField userNameField;

    public void Connect()
    {
        startMenu.SetActive(false);
        userNameField.interactable = false;

        Client.instance.ConnectToServer();
    }
}
