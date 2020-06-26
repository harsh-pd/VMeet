using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

namespace AL
{
    public class Settings : MonoBehaviour {
        [SerializeField]
        Preferences selectedPreferences, defaultPreferences;
        [SerializeField]
        TMP_InputField owner, buffersize, updateInterval;
        [SerializeField]
        Button saveButton;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            owner.text = selectedPreferences.owner;
            buffersize.text = selectedPreferences.bufferSize.ToString();
            updateInterval.text = selectedPreferences.updateInterval.ToString();
        }

        public Preferences SelectedPreferences
        {
            get
            {
                return selectedPreferences;
            }
        }

        public void EnableEditing()
        {
            saveButton.interactable = true;
            owner.readOnly = false;
            buffersize.readOnly = false;
            updateInterval.readOnly = false;
        }

        public void Save()
        {
            saveButton.interactable = false;
            owner.readOnly = true;
            buffersize.readOnly = true;
            updateInterval.readOnly = true;
            selectedPreferences.owner = owner.text;
            if(!string.IsNullOrEmpty(buffersize.text))
                selectedPreferences.bufferSize = Int32.Parse(buffersize.text);
            if(!string.IsNullOrEmpty(updateInterval.text))
                selectedPreferences.updateInterval = Int32.Parse(updateInterval.text);
        }

        public void OnReset()
        {
            owner.readOnly = true;
            buffersize.readOnly = true;
            updateInterval.readOnly = true;
            buffersize.text = defaultPreferences.bufferSize.ToString();
            owner.text = defaultPreferences.owner;
            updateInterval.text = defaultPreferences.updateInterval.ToString();
            selectedPreferences.owner = defaultPreferences.owner;
            selectedPreferences.updateInterval = defaultPreferences.updateInterval;
            selectedPreferences.bufferSize = defaultPreferences.bufferSize;
        }
    }
}
