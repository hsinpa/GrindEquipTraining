using UnityEngine;
using System.Collections.Generic;

namespace Hsinpa.Utility
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AudioSRP", order = 2)]
    public class AudioSRP : ScriptableObject
    {
        public string tag;

        [SerializeField]
        public List<AudioSet> audioSets = new List<AudioSet>();

        [System.Serializable]
        public struct AudioSet {
            public string id;

            public AudioClip audioClip;
        }
    }
}