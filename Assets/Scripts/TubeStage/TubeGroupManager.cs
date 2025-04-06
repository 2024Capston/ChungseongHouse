using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TubeStage
{
    public class TubeGroupManager : MonoBehaviour
    {
        [Tooltip("Source Tube Group")][SerializeField]
        private List<TubeController> _sourceTubeGroup;

        [Tooltip("Destination Tube")][SerializeField] 
        private TubeController _destTube;
        
        public void SetTubeLight(int index, bool isLightOn)
        {
            _sourceTubeGroup[index].SetTubeLightClientRpc(isLightOn);
        }

        public void SetSourceTubeValue(int index, float deltaValue)
        {
            _sourceTubeGroup[index].UpdateValue(deltaValue);
        }

        public void SetDestTubeValue(float deltaValue)
        {
            _destTube.UpdateValue(deltaValue);
        }
    }
}