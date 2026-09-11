using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Utility
{
    [System.Serializable]
    public struct AudioSRPSet
    {
        [SerializeField]
        private List<AudioSRP> audioSet;

        public bool Is_Valid => this.audioSet != null && this.audioSet.Count > 0;

        public AudioClip GetAudioClip(string tag, string id) {

            if (audioSet == null) return null;

            var audioTag =  audioSet.Find(x => x.tag == tag);

            return audioTag.audioSets.Find(x => id == x.id).audioClip;
        }
    }
}