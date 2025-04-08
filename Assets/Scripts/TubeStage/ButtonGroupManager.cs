using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TubeStage
{
    public class ButtonGroupManager : MonoBehaviour
    {
        [SerializeField] private List<TubeButtonController> _buttonGroup;

        public void SetButtonColor(int index, ColorType colorType)
        {
            _buttonGroup[index].SetButtonColor(colorType);
        }

        public void UnpressButton(int index)
        {
            _buttonGroup[index].UnpressButton();
            _buttonGroup[index].PlayPressAnimation(false);
        }

        public void SetButtonEnable(int index, bool isEnable)
        {
            if (isEnable)
            {
                _buttonGroup[index].EnableButton();
            }
            else
            {
                _buttonGroup[index].DisableButton();
            }
        }
    }   
}