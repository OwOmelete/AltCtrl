using UnityEngine;

namespace MiniGames
{
    public class Bird : AbstractMiniGame
    {
        protected override void MiniGameStart()
        {
            Debug.Log("oh un oiseau s'est écrase sur votre parre-brise ^^");
        }

        protected override void MiniGameUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                GameManager.INSTANCE.savon = !GameManager.INSTANCE.savon;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (GameManager.INSTANCE.savon)
                {
                    Debug.Log("bravo vous avez tout netoyé !!! :D");
                    Win();
                }
                else
                {
                    Debug.Log("oh non vous en avez étalé partout :( ");
                }
            }
        }

        public override void Win()
        {
            enabled = false;
        }
    }
}
