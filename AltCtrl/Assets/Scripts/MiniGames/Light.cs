using UnityEngine;

namespace MiniGames
{
    public class Light : AbstractMiniGame

    {

        [SerializeField] private GameObject picto;
        protected override void MiniGameStart()
        {
            picto.SetActive(true);
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
            enabled = false;
        }
    }
}