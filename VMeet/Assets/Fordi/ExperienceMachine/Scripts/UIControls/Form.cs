using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRExperience.Core;

namespace VRExperience.UI.MenuControl
{
    public interface IForm : IScreen
    {
        void DisplayError(Error error);
    }

    public class Form : MenuScreen, IForm
    {
        public void DisplayError(Error error)
        {
            throw new System.NotImplementedException();
        }
    }
}