using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.Desktop
{
    public class Timer : MonoBehaviour
    {

        public Image image;

        public int time;

        public void Show()
        {
            image.enabled = true;
            StartCoroutine(ShowTimer());
        }

        private IEnumerator ShowTimer()
        {
            if (time <= 0)
            {
                //ActiveNext;
                Hide();
                yield break;
            }

            yield return new WaitForSeconds(1f);

            this.transform.Rotate(new Vector3(0, 0, 1));
            time--;

            StartCoroutine(ShowTimer());
        }

        internal void Hide()
        {
            image.enabled = false;
        }
    }
}