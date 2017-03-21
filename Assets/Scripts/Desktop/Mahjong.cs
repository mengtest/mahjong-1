using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.Desktop
{
    public class Mahjong : MonoBehaviour
    {

        /// <summary>
        /// 所有的牌
        /// </summary>
        public List<int> allMah = new List<int>();

        /// <summary>
        /// 牌的ID
        /// 1-9 -> 筒
        /// 11-19 -> 条
        /// 21 22 24 -> 中發白
        /// </summary>
        private int[] ID = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 13, 14, 15, 16, 17, 18, 19, 21, 22, 24 };
        public List<int> list = new List<int>();


        #region
        private byte SHUFFLECODE = 0;

        #endregion

        void Awake()
        {
            //注册事件
            PhotonNetwork.OnEventCall += this.OnEvent;

            if (!PhotonNetwork.isMasterClient)
            {
                return;
            }

            list.Clear();
            for (int i = 0; i < ID.Length; i++)
            {
                list.Add(ID[i]);
                list.Add(ID[i]);
                list.Add(ID[i]);
                list.Add(ID[i]);
            }
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        public void ShuffleMah()
        {
            allMah.Clear();

            List<int> mahs = new List<int>(list);

            while (mahs.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, mahs.Count);
                allMah.Add(mahs[index]);
                mahs.RemoveAt(index);
            }
            PhotonNetwork.RaiseEvent(SHUFFLECODE, allMah.ToArray(), true, null);

        }

        private void OnEvent(byte eventcode, object content, int senderid)
        {
            Debug.Log("MahJong " + eventcode);
            if (eventcode == SHUFFLECODE)
            {
                allMah = new List<int>((int[])content);
            }
        }

    }
}