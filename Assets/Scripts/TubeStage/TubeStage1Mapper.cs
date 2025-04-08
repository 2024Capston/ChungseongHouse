using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace TubeStage
{
    public class TubeStage1Mapper : TubeStageMapper
    {
        #region TubeControl
        
        public override void InitTube()
        {
            for (int i = 0; i < _buttonMap.Count; i++)
            {
                _tubeGroupManager.SetTubeLight(i, false);
                _tubeGroupManager.SetSourceTubeValue(i, 1f);
                _tubeGroupManager.SetDestTubeValue(-1f);
            }
        }
        
        public override void ApplyButtonState()
        {
            int count = 0;
            for (int i = 0; i < _buttonMap.Count; i++)
            {
                if ((ButtonState & (1 << i)) != 0)
                {
                    _tubeGroupManager.SetSourceTubeValue(i, -0.25f);
                    ++count;
                }
            }
            
            _tubeGroupManager.SetDestTubeValue(count * 0.0625f);
        }
        
        public override void SetSourceTubeLight(int lightMap)
        {
            for (int i = 0; i < _buttonMap.Count; i++)
            {
                _tubeGroupManager.SetTubeLight(i, (lightMap & (1 << i)) != 0);
            }
        }

        #endregion

        #region ButtonControl

        
        
        public override void SetButtonGroupColor()
        {
            Random random = new Random();
            List<ColorType> randomMap = _buttonMap.OrderBy(x => random.Next()).ToList();
            _buttonMap.Clear();
            _buttonMap.AddRange(randomMap);

            for (int i = 0; i < _buttonMap.Count; i++)
            {
                _buttonGroupManager.SetButtonColor(i, _buttonMap[i]);
                _buttonGroupManager.UnpressButton(i);
                _buttonGroupManager.SetButtonEnable(i, true);
            }
        }

        protected override void OnClickButton(ColorType colorType, bool isHost, bool isPressed)
        {
            if (isPressed)
            {
                ButtonState |= 1 << (int)colorType;
            }
            else
            {
                ButtonState &= ~(1 << (int)colorType);
            }
        }

        #endregion

        public override void ResetAll()
        {
            for (int i = 0; i < _buttonMap.Count; i++)
            {
                _buttonGroupManager.SetButtonColor(i, ColorType.None);
                _buttonGroupManager.UnpressButton(i);
                _buttonGroupManager.SetButtonEnable(i, false);
                
                _tubeGroupManager.SetTubeLight(i, false);
                _tubeGroupManager.SetSourceTubeValue(i, -1f);
            }
            _tubeGroupManager.SetDestTubeValue(-1f);
        }
    }

}