using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MiniGames
{
    public class Simon : AbstractMiniGame
    {
        [SerializeField] private Image[] buttons;
        [SerializeField] private GameObject picto;
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

        private bool canInput = true;
        private List<int> Chain = new();
        private int currentProgression = 0;
        public int MaxChainLength;
        private bool started = false;

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
            started = false;
            picto.SetActive(true);
            StartCoroutine(SimonStart());
        }

        private void Reset()
        {
            Chain.Clear();
            AddNumberToList();
            started = true;
        }

        private void AddNumberToList()
        {
            if (Chain.Count == MaxChainLength)
            {
                Win();
                return;
            }
            int r = Random.Range(1, 10);
            Chain.Add(r);
            StartCoroutine(sequencePlay());
            currentProgression = 0;
            for (int i = 0; i < Chain.Count; i++)
            {
                Debug.Log(Chain[i]);
            }
        }

        protected override void MiniGameUpdate()
        {
            if (canInput && started)
            {
                int i = getNumberKey();
                if (i == Chain[currentProgression])
                {
                    StartCoroutine(buttonBip(i-1));
                    currentProgression += 1;
                    if (currentProgression + 1 > Chain.Count)
                    {
                        AddNumberToList();
                    }
                    else
                    {
                        Debug.Log("trouvé");
                    }
                }
            }
        }

        IEnumerator buttonBip(int i)
        {
            buttons[i].enabled = true;
            yield return new WaitForSeconds(0.2f);
            buttons[i].enabled = false;
        }

        IEnumerator SimonStart()
        {
            foreach (var button in buttons)
            {
                button.enabled = true;
            }
            yield return new WaitForSeconds(0.6f);
            foreach (var button in buttons)
            {
                button.enabled = false;
            }
            yield return new WaitForSeconds(0.6f);
            foreach (var button in buttons)
            {
                button.enabled = true;
            }
            yield return new WaitForSeconds(0.6f);
            foreach (var button in buttons)
            {
                button.enabled = false;
            }
            yield return new WaitForSeconds(0.6f);
            Reset();
        }

        IEnumerator sequencePlay()
        {
            canInput = false;
            yield return new WaitForSeconds(0.8f);
            for (int i = 0; i < Chain.Count; i++)
            {
                buttons[Chain[i]-1].enabled = true;
                yield return new WaitForSeconds(0.4f);
                buttons[Chain[i]-1].enabled = false;
                yield return new WaitForSeconds(0.4f);
            }
            canInput = true;
        }

        public override void Win()
        {
            picto.SetActive(false);
            enabled = false;
        }
    }
}