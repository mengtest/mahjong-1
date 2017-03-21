using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.Desktop
{
    /// <summary>
    /// 麻将基类
    /// </summary>
    public class MahJongObject : MonoBehaviour
    {
        /// <summary>
        /// 麻将ID
        /// {1  -  9} -> {１－９ 筒}
        /// {11 - 19} -> {1 - 9 条}
        /// {21 - 23} -> {中 發 白}
        /// </summary>
        [HideInInspector]
        public int ID;

        [HideInInspector]
        public MahPlayer player;

        [HideInInspector]
        public bool CanCilcked = false;

        public MahJongObject()
        {

        }

        public MahJongObject(int ID)
        {
            this.ID = ID;
        }

        //打牌
        public void OnClick()
        {
            if (!CanCilcked)
            {
                return;
            }
            player.Abandan(this.ID);
        }

    }
}