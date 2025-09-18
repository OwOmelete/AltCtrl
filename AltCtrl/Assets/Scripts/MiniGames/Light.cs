using System.Collections.Generic;
using UnityEngine;

namespace MiniGames
{
    public class Light : AbstractMiniGame

    {
        List<string> clips = new List<string> { "SFX electricity relaunch" };
        List<string> clips2 = new List<string> { "Sound_passagers" };
        

        [SerializeField] private GameObject picto;
        protected override void MiniGameStart()
        {
            picto.SetActive(true);
            SoundManager.Instance.PlayRandomSFX(clips2, 1f, 1f);
        }

        protected override void MiniGameUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("vous avez rétablit la lumière :)");
                
                Win();
            }
        }

        public override void Win()
        {
            picto.SetActive(false);
            SoundManager.Instance.PlayRandomSFX(clips, 1f, 1f);
            enabled = false;
        }
    }
}