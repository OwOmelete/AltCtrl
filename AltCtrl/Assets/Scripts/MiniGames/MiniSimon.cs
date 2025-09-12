using System.Collections.Generic;
using UnityEngine;

namespace MiniGames
{
    public class MiniSimon : AbstractMiniGame
    {
        [SerializeField] private int ChainCount;
        
        private KeyCode[] keyCodes = {
            KeyCode.Keypad1,
            KeyCode.Keypad2,
            KeyCode.Keypad3,
            KeyCode.Keypad4,
            KeyCode.Keypad5,
            KeyCode.Keypad6,
            KeyCode.Keypad7,
            KeyCode.Keypad8,
            KeyCode.Keypad9
        };
        
        private List<int> Chain = new();
        
        private int getNumberKey()
        {
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if (Input.GetKeyDown(keyCodes[i]))
                {
                    return i+1;
                }
            }
            return 0;
        }
        protected override void MiniGameStart()
        {
            for (int i = 0; i < ChainCount; i++)
            {
                int r = Random.Range(1, 10);
                if (!Chain.Contains(r))
                {
                    Chain.Add(r);
                    Debug.Log(r);
                }
                else
                {
                    i -= 1;
                }
            }
        }

        protected override void MiniGameUpdate()
        {
            int input = getNumberKey();
            for (int i = 0; i < Chain.Count; i++)
            {
                if (input == Chain[i])
                {
                    buttonPressed(input);
                    break;
                }
            }

            if (Chain.Count == 0)
            {
                Win();
            }
        }
        
        private void buttonPressed(int number)
        {
            Chain.Remove(number);
        }

        public override void Win()
        {
            enabled = false;
        }
    }
}