using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game.Crystals
{
    [System.Serializable]
    public class DependingLightTrail
    {
        public GameObject FirstObject;
        public GameObject SecondObject;
        public GameObject LightTrail;
    }

    public class PentagramLight : MonoBehaviour
    {
        public List<DependingLightTrail> LightTrails = new List<DependingLightTrail>();
        private void Update()
        {
            foreach (var lightTrail in LightTrails)
            {
                if (lightTrail.FirstObject.activeSelf && lightTrail.SecondObject.activeSelf)
                {
                    lightTrail.LightTrail.SetActive(true);
                }
                else
                {
                    lightTrail.LightTrail.SetActive(false);
                }
            }
        }
    }
}
