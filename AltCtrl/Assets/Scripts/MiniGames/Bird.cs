using UnityEngine;

namespace MiniGames
{
    public class Bird : AbstractMiniGame
    {
        private bool birdOnScreen = false;
        protected override void MiniGameStart()
        {
            Debug.Log("oh un oiseau s'est écrase sur votre parre-brise ^^");
            birdOnScreen = true;
        }

        protected override void MiniGameUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (birdOnScreen)
                {
                    Debug.Log("l'oiseau est parti mais il reste les plumes ^^");
                    birdOnScreen = false;
                }
                else
                {
                    Debug.Log("les plumes sont parties aussi :D");
                }
            }
        }

        public override void Win()
        {
            enabled = false;
        }
    }
}
