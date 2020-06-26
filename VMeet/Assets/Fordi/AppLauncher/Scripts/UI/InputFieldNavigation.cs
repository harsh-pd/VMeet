using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AL.UI
{
    public class InputFieldNavigation : MonoBehaviour
    {

        public TMPro.TMP_InputField userName, password;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && userName.isFocused)
            {
                password.Select();
            }
            else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Tab) && password.isFocused)
            {
                userName.Select();
            }
        }
    }
}
