using UnityEngine;

namespace MiniGames
{
    public class Storm : AbstractMiniGame
    {
        [SerializeField] private StateABController orage;
        [SerializeField] private ShaderStateABController pluieLeft;
        [SerializeField] private ShaderStateABController pluieRight;
        [SerializeField] private GameObject eclairs;
        [SerializeField] private GameObject rainEffect;
        [SerializeField] private float rainDelay;
        [SerializeField] private float stormTime;
        private float stormBeginingTime;
        private bool rainOnScreen = false;
        private float? lastRain = null;
        [SerializeField] private GameObject picto;
        protected override void MiniGameStart()
        {
            picto.SetActive(true);
            rainEffect.SetActive(true);
            orage.GoAToB(orage.defaultDuration);
            eclairs.SetActive(true);
            stormBeginingTime = Time.time;
        }

        protected override void MiniGameUpdate()
        {
            if (Time.time - stormBeginingTime > stormTime)
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
            picto.SetActive(true);
            if (rainOnScreen)
            {
                pluieLeft.GoBToA(pluieLeft.defaultDuration);
                pluieRight.GoBToA(pluieRight.defaultDuration);
                rainOnScreen = false;
            }
            orage.GoBToA(orage.defaultDuration);
            eclairs.SetActive(false);
            enabled = false;
        }
    }
}