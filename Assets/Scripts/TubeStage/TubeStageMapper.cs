using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TubeStage
{
    public abstract class TubeStageMapper : NetworkSingletonBehaviour<TubeStageMapper>
    {
        [SerializeField] protected TubeGroupManager _tubeGroupManager;
        [SerializeField] protected ButtonGroupManager _buttonGroupManager;
        
        public int ButtonState { get; protected set; }
        
        protected List<ColorType> _buttonMap = new List<ColorType> { ColorType.None , ColorType.Blue, ColorType.Red, ColorType.Purple };

        protected override void Init()
        {
            _isDestroyOnLoad = true;
            base.Init();
        }

        /// <summary>
        /// Tube를 초기 상태로 만든다.
        /// SourceTube.Value = 1f, DestTube.Value = 0f
        /// </summary>
        public abstract void InitTube();

        /// <summary>
        /// 현재 ButtonState에 따라 Tube의 Value를 수정한다.
        /// </summary>
        public abstract void ApplyButtonState();

        /// <summary>
        /// 주어진 LightMap에 따라 Tube Light를 키거나 끈다.
        /// </summary>
        /// <param name="lightMap"></param>
        public abstract void SetSourceTubeLight(int lightMap);
        
        /// <summary>
        /// Button의 색상 배치를 변경한다.
        /// </summary>
        public abstract void SetButtonGroupColor();

        public abstract void ResetAll();

        public abstract void OnClickButton(ColorType colorType, bool isHost, bool isPressed);
        
        
        /// <summary>
        /// Button State를 초기화 한다.
        /// </summary>
        public void ClearState()
        {
            ButtonState = 0;
        }

        
    }
}

