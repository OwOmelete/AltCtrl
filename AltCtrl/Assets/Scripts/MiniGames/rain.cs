using UnityEngine;

namespace MiniGames
{
    public class rain : AbstractMiniGame
    {
        [SerializeField] private ShaderStateABController pluieLeft;
        [SerializeField] private ShaderStateABController pluieRight;
        [SerializeField] private float rainDelay;
        [SerializeField] private float stormTime;
        [SerializeField] private GameObject rainEffect;
        private float rainBeginningTime;
        private bool rainOnScreen = false;
        private float? lastRain = null;
        [SerializeField] private GameObject picto;
        protected override void MiniGameStart()
        {
            picto.SetActive(true);
            rainEffect.SetActive(true);
            rainBeginningTime = Time.time;
        }

        protected override void MiniGameUpdate()
        {
            if (Time.time - rainBeginningTime > stormTime)
            {
                Win();
            }

            if ((Time.time - lastRain > rainDelay || lastRain == null) && !rainOnScreen)
            {
                rainOnScreen = true;
                pluieLeft.GoAToB(10);
                pluieRight.GoAToB(10);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                pluieLeft.wiper();
                pluieRight.wiper();
                if (rainOnScreen)
                {
                    lastRain = Time.time;
                    rainOnScreen = false;
                }
            }
        }

        public override void Win()
        {
            rainEffect.SetActive(false);
            picto.SetActive(false);
            if (rainOnScreen)
            {
                pluieLeft.GoBToA(pluieLeft.defaultDuration);
                pluieRight.GoBToA(pluieRight.defaultDuration);
                rainOnScreen = false;
            }
            enabled = false;
        }
    }
}