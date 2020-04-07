using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

namespace Cornea
{
    public class IntermediatePanel : MonoBehaviour
    {
        [SerializeField]
        private Button cancelButton, closeScreenButton;
        [SerializeField]
        private TextMeshProUGUI dynamicText;

        private List<Action> onCancelActions = new List<Action>();

        public TextMeshProUGUI DynamicText { get { return dynamicText; } }

        public void Activate(Action _onCancelAction, bool allowCancellation = true)
        {
            //Debug.Log("Activate");
            gameObject.SetActive(true);
            onCancelActions.Add(_onCancelAction);
            if (cancelButton)
                cancelButton.gameObject.SetActive(allowCancellation);
            if (closeScreenButton)
                closeScreenButton.gameObject.SetActive(allowCancellation);
            transform.parent.SetAsLastSibling();
        }

        public void Deactivate()
        {
            //Debug.Log("Deactivate: " + this.name);
            onCancelActions.Clear();
            gameObject.SetActive(false);
        }

        public void OnCancel()
        {
            gameObject.SetActive(false);
            foreach (var item in onCancelActions)
                if (item != null)
                    item.Invoke();
        }
    }
}
