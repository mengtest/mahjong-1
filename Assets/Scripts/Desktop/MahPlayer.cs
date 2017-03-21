using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.Desktop
{
    public class MahPlayer : MonoBehaviour
    {
        #region 玩家的牌

        /// <summary>
        /// 是否为庄家
        /// 游戏开始 第一出手的玩家
        /// </summary>
        public bool isDealer = false;

        /// <summary>
        /// 是否在玩
        /// </summary>
        public bool isPlaying = true;

        /// <summary>
        /// 手牌
        /// </summary>
        public List<int> keepedMah = new List<int>();

        public GameObject plane_keep;

        /// <summary>
        /// 弃牌
        /// </summary>
        public List<int> abandanedMah = new List<int>();

        public GameObject plane_abandan;

        /// <summary>
        /// 碰的牌
        /// </summary>
        public List<int> ponMah = new List<int>();

        public GameObject plane_pon;

        /// <summary>
        /// 取到的牌
        /// </summary>
        private MahJongObject gotMah;

        /// <summary>
        /// 牌面上被打出去的牌
        /// </summary>
        public MahJongObject abandonMah;

        #endregion

        #region 玩家的状态
        public enum STATE
        {
            //休息状态 场外休息
            resting = 0,

            //等待状态 等待其他玩家作决策
            waiting = 1,

            //正在打牌 正在作决策
            playing = 2
        }

        public STATE state = STATE.resting;

        //玩家昵称
        public Text playerName;

        //具有行动权
        public bool activable = false;

        //与PhotonPlayer中的玩家关联
        public PhotonPlayer photonPlayer;

        //下一个玩家
        public MahPlayer nextPlayer;

        //Photon通讯组件
        public PhotonView photonView;

        #endregion

        #region RiseEnventCode  事件码

        //打牌事件码
        private byte DAPAICODE = 3;

        //显示自己牌的数量
        private byte SHOWPAITOOTHER = 4;

        //下一个玩家的行动激活
        private byte ACTIVENEXT = 5;

        //下为玩家取牌
        private byte NEXTPLAYERGETMAH = 6;

        //碰牌事件
        private byte PONMAH = 7;

        //杠牌事件
        private byte GANGPAI = 8;

        //胡牌
        private byte WINPAI = 9;
        #endregion

        #region UI引用

        public Button btnPon;
        public Button btnWin;
        public Button btnGang;
        public Button btnpass;

        public Timer timer;
        #endregion 

        private void Start()
        {
            //注册RaiseEvent事件函数
            PhotonNetwork.OnEventCall += OnEventCall;
        }
        /// <summary>
        /// 事件绑定
        /// </summary>
        private void BundleUI()
        {
            playerName.text = photonPlayer.NickName;

            if (!photonPlayer.IsLocal)
            {
                return;
            }

            btnWin.onClick.RemoveAllListeners();
            btnWin.onClick.AddListener(delegate () { Win(); });

            btnPon.onClick.RemoveAllListeners();
            btnPon.onClick.AddListener(delegate () { Pon(); });

            btnGang.onClick.RemoveAllListeners();
            btnGang.onClick.AddListener(delegate () { Gang(); });

            btnpass.onClick.RemoveAllListeners();
            btnpass.onClick.AddListener(delegate () { Pass(); });
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {

            if (state == STATE.playing || activable)
            {
                if (timer.image.IsActive())
                {
                    return;
                }

                int[] param = { photonPlayer.ID };
                photonView.RPC("ShowTimer", PhotonTargets.All, param);
            }

            if (activable)
            {
                abandonMah = GameManager.Instance.abandonMah;

                if (abandonMah.player.photonPlayer.ID == photonPlayer.ID)
                {
                    //下一位玩家取牌
                    PhotonNetwork.RaiseEvent(NEXTPLAYERGETMAH, null, true, null);
                }
                else
                {
                    bool isCanWin = MahJongTools.IsCanHU(keepedMah, abandonMah.ID);
                    bool isCanPon = MahJongTools.IsCanPon(keepedMah, abandonMah.ID, state);
                    bool isCanGang = MahJongTools.IsCanGang(keepedMah, abandonMah.ID, state);

                    btnWin.gameObject.SetActive(isCanWin);
                    btnPon.gameObject.SetActive(isCanPon);
                    btnGang.gameObject.SetActive(isCanGang);

                    if (!isCanWin && !isCanPon && !isCanGang)
                    {
                        // 行动权交给下一个玩家
                        PhotonNetwork.RaiseEvent(ACTIVENEXT, null, true, null);
                    }
                    else
                    {
                        btnpass.gameObject.SetActive(true);
                    }
                }
                //行动完
                activable = false;
            }

        }

        /// <summary>
        /// 显示自己的牌
        /// </summary>
        public void ShowUI()
        {
            BundleUI();

            if (photonPlayer.IsLocal)
            {
                keepedMah.Sort();

                foreach (int a in keepedMah)
                {
                    GameObject d = Instantiate(Resources.Load("MahJong/" + a) as GameObject);
                    d.name = a + "";
                    d.transform.SetParent(plane_keep.transform);
                    d.transform.localScale = Vector3.one;
                    d.transform.localRotation = Quaternion.identity;
                    MahJongObject mah = d.GetComponent<MahJongObject>();
                    mah.ID = a;
                    mah.CanCilcked = true;
                    mah.player = this;
                }

                PhotonNetwork.RaiseEvent(SHOWPAITOOTHER, null, true, null);
            }
            else
            {
                for (int i = 0; i < 13; i++)
                {
                    GameObject d = Instantiate(Resources.Load("MahJong/" + 0) as GameObject);
                    d.name = 0 + "";
                    d.transform.SetParent(plane_keep.transform);
                    d.transform.localScale = Vector3.one;
                    d.transform.localRotation = Quaternion.identity;
                }
            }
        }

        /// <summary>
        /// 取牌
        /// </summary>
        public void GetMahJong(bool isfirst)
        {
            activable = false;
            int GotID = GameManager.Instance.GetMahjong(isfirst);

            keepedMah.Add(GotID);

            GameObject d = Instantiate(Resources.Load("MahJong/" + GotID) as GameObject);
            d.name = GotID + "";
            d.transform.SetParent(plane_keep.transform);
            d.transform.localScale = Vector3.one;
            d.transform.localRotation = Quaternion.identity;
            MahJongObject mah = d.GetComponent<MahJongObject>();
            mah.ID = GotID;
            mah.CanCilcked = true;
            mah.player = this;

            gotMah = mah;
            state = STATE.playing;
        }

        /// <summary>
        /// 将渠道的牌按照顺序插入
        /// </summary>
        void SetGotMahPosition()
        {
            keepedMah.Sort();
            int index = 0;
            for (int i = 0; i < keepedMah.Count; i++)
            {
                if (keepedMah[i] >= gotMah.ID)
                {
                    break;
                }
                index++;
            }

            gotMah.transform.SetSiblingIndex(index);
        }

        /// <summary>
        /// 打牌
        /// </summary>
        /// <param name="ID">打出去牌的ID</param>
        public MahJongObject Abandan(int mahID)
        {
            if (photonPlayer.IsLocal)
            {
                if (state != STATE.playing)
                {
                    return null;
                }

                state = STATE.waiting;

                keepedMah.Remove(mahID);
                abandanedMah.Add(mahID);

                GameObject g = plane_keep.transform.Find(mahID + "").gameObject;
                g.GetComponent<MahJongObject>().CanCilcked = false;
                g.transform.SetParent(plane_abandan.transform);

                GameManager.Instance.abandonMah = g.GetComponent<MahJongObject>();

                PhotonNetwork.RaiseEvent(DAPAICODE, mahID, true, null);

                SetGotMahPosition();
            }
            else
            {
                GameObject d = Instantiate(Resources.Load("MahJong/" + mahID) as GameObject);
                d.name = mahID + "";
                d.transform.SetParent(plane_abandan.transform);
                d.transform.localScale = Vector3.one;
                d.transform.localRotation = Quaternion.identity;

                MahJongObject mahObj = d.GetComponent<MahJongObject>();
                mahObj.ID = mahID;
                mahObj.player = this;
                mahObj.CanCilcked = false;
                return mahObj;
            }
            return null;
        }

        /// <summary>
        /// 玩家的动作  碰牌
        /// </summary>
        /// <param name="MahID">碰牌的ID为MahID</param>
        public void Pon()
        {
            if (photonPlayer.IsLocal)
            {
                keepedMah.Remove(abandonMah.ID);
                keepedMah.Remove(abandonMah.ID);

                ponMah.Add(abandonMah.ID);
                ponMah.Add(abandonMah.ID);
                ponMah.Add(abandonMah.ID);

                abandonMah.gameObject.transform.SetParent(plane_pon.transform);
                plane_keep.transform.FindChild(abandonMah.ID + "").SetParent(plane_pon.transform);
                plane_keep.transform.FindChild(abandonMah.ID + "").SetParent(plane_pon.transform);

                state = STATE.playing;
                HideMenu();

                PhotonNetwork.RaiseEvent(PONMAH, null, true, null);
            }
            else
            {
                MahJongObject aMah = GameManager.Instance.abandonMah;

                aMah.transform.SetParent(plane_pon.transform);
                int mahID = aMah.ID;

                for (int i = 0; i < 2; i++)
                {

                    //显示碰了的牌
                    GameObject d = Instantiate(Resources.Load("MahJong/" + mahID) as GameObject);
                    d.name = mahID + "";
                    d.transform.SetParent(plane_pon.transform);
                    d.transform.localScale = Vector3.one;
                    d.transform.localRotation = Quaternion.identity;

                    MahJongObject mahObj = d.GetComponent<MahJongObject>();
                    mahObj.ID = mahID;
                    mahObj.player = this;
                    mahObj.CanCilcked = false;
                }

                //移除手牌
                for (int i = 0; i < 3; i++)
                {
                    GameObject g = plane_keep.transform.GetChild(i).gameObject;
                    Destroy(g);
                }
            }
        }

        /// <summary>
        /// 玩家的动作  杠牌
        /// </summary>
        /// <param name="MahID">碰牌的ID为MahID</param>
        public void Gang()
        {
            if (photonPlayer.IsLocal)
            {
                //显示杠了的牌
                keepedMah.Remove(abandonMah.ID);
                keepedMah.Remove(abandonMah.ID);
                keepedMah.Remove(abandonMah.ID);

                ponMah.Add(abandonMah.ID);
                ponMah.Add(abandonMah.ID);
                ponMah.Add(abandonMah.ID);
                ponMah.Add(abandonMah.ID);

                abandonMah.gameObject.transform.SetParent(plane_pon.transform);
                plane_keep.transform.FindChild(abandonMah.ID + "").SetParent(plane_pon.transform);
                plane_keep.transform.FindChild(abandonMah.ID + "").SetParent(plane_pon.transform);
                plane_keep.transform.FindChild(abandonMah.ID + "").SetParent(plane_pon.transform);

                //设置为能够打牌
                state = STATE.playing;

                //隐藏菜单
                HideMenu();

                //从最后面取一张牌
                GetMahJong(false);

                //通知其他客户端
                PhotonNetwork.RaiseEvent(GANGPAI, null, true, null);
            }
            else  //在其他客户端作处理 主要是显示杠出来的牌
            {
                MahJongObject aMah = GameManager.Instance.abandonMah;

                aMah.transform.SetParent(plane_pon.transform);
                int mahID = aMah.ID;

                for (int i = 0; i < 3; i++)
                {
                    GameObject d = Instantiate(Resources.Load("MahJong/" + mahID) as GameObject);
                    d.name = mahID + "";
                    d.transform.SetParent(plane_pon.transform);
                    d.transform.localScale = Vector3.one;
                    d.transform.localRotation = Quaternion.identity;

                    MahJongObject mahObj = d.GetComponent<MahJongObject>();
                    mahObj.ID = mahID;
                    mahObj.player = this;
                    mahObj.CanCilcked = false;
                }

                //移除手牌
                for (int i = 0; i < 3; i++)
                {
                    GameObject g = plane_keep.transform.GetChild(i).gameObject;
                    Destroy(g);
                }

            }
        }

        /// <summary>
        /// 玩家动作 胡牌
        /// </summary>
        /// <param name="MahID">胡牌的ID</param>
        public void Win()
        {
            int[] param = { photonPlayer.ID };
            photonView.RPC("WinPai", PhotonTargets.All, param);
        }

        /// <summary>
        /// 过
        /// </summary>
        public void Pass()
        {
            if (!photonPlayer.IsLocal)
            {
                return;
            }
            PhotonNetwork.RaiseEvent(ACTIVENEXT, null, true, null);
            HideMenu();
        }

        /// <summary>
        /// 隐藏菜单
        /// </summary>
        private void HideMenu()
        {
            btnPon.gameObject.SetActive(false);
            btnWin.gameObject.SetActive(false);
            btnGang.gameObject.SetActive(false);
            btnpass.gameObject.SetActive(false);
        }

        /// <summary>
        /// 交出行动权 激活下一个玩家
        /// </summary>
        /// <param name="abandonMahID">被打出去的牌</param>
        public void ActiveNext()
        {
            nextPlayer.activable = true;
        }

        /// <summary>
        /// 事件处理
        /// </summary>
        /// <param name="eventCode">事件编码</param>
        /// <param name="content">内容信息</param>
        /// <param name="senderId">发送者</param>
        private void OnEventCall(byte eventCode, object content, int senderId)
        {
            timer.Hide();

            #region 过滤条件  找到自己在其他客户端上的投影玩家
            if (photonPlayer.ID != senderId)
            {
                return;
            }
            #endregion

            #region 显示自己牌的数量
            if (eventCode == SHOWPAITOOTHER)
            {
                ShowUI();
            }

            #endregion

            #region 显示计时器

            if (eventCode == ACTIVENEXT || eventCode == NEXTPLAYERGETMAH)
            {
                nextPlayer.timer.Show();
            }
            #endregion

            #region 打牌
            if (eventCode == DAPAICODE)
            {
                int mahID = (int)content;
                MahJongObject mahObj = Abandan(mahID);

                GameManager.Instance.abandonMah = mahObj;
                if (!nextPlayer.photonPlayer.IsLocal)
                {
                    return;
                }
                ActiveNext();
            }

            #endregion

            #region 碰牌
            if (eventCode == PONMAH)
            {
                Pon();
            }
            #endregion

            #region 杠牌
            if (eventCode == GANGPAI)
            {
                Gang();
            }

            #endregion

            #region 过滤条件 找到下一个玩家为本地玩家(而非投影玩家)的自己的投影
            if (!nextPlayer.photonPlayer.IsLocal)
            {
                return;
            }
            #endregion

            #region 下一个玩家行动
            if (eventCode == ACTIVENEXT)
            {
                ActiveNext();
            }
            #endregion

            #region 下一位玩家取牌
            if (eventCode == NEXTPLAYERGETMAH)
            {
                nextPlayer.GetMahJong(true);
            }
            #endregion

        }
    }
}