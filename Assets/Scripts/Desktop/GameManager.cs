using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.Desktop
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance = null;

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 所有玩家的集合
        /// </summary>
        public List<MahPlayer> players;

        /// <summary>
        /// 麻将牌
        /// </summary>
        public Mahjong mahJong;

        //通讯插件
        public PhotonView photonView;

        #region 
        /// <summary>
        /// 骰子数
        /// </summary>
        private int diceNum;

        /// <summary>
        /// 牌面上被打出去的牌
        /// </summary>
        public MahJongObject abandonMah;

        public Image OverPanel;

        /// <summary>
        /// 事件代码
        /// </summary>
        private byte DEALINGCODE = 10;
        private byte GETMAHCODE = 11;
        #endregion
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }

            PhotonNetwork.OnEventCall += OnEvent;
        }

        void Start()
        {
            //只有主机有发牌的权利
            if (!PhotonNetwork.isMasterClient)
            {
                return;
            }

            //关联Photon玩家 与 Unity中的玩家
            BundlePlauer();

            //洗牌
            ShuffleMah();

            //摇骰子
            Dice();

            //发牌
            DealingMahs();

            //玩家取牌
            players[0].GetMahJong(true);
        }

        private void ShuffleMah()
        {
            mahJong.ShuffleMah();
        }

        /// <summary>
        /// 绑定玩家与photonplayer
        /// </summary>
        private void BundlePlauer()
        {
            //按照 ABCD的顺序传photonplayer.ID
            int[] ids = new int[4];

            //添加互相引用
            int ppindex = 0;
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                if (PhotonNetwork.player.ID == PhotonNetwork.playerList[i].ID)
                {
                    ppindex = i;
                    break;
                }
            }

            for (int i = 0; i < 4; i++, ppindex++)
            {
                ppindex = ppindex % 4;
                players[i].photonPlayer = PhotonNetwork.playerList[ppindex];
                PhotonNetwork.playerList[ppindex].mahPlayer = players[i];

                ids[i] = players[i].photonPlayer.ID;

            }
            Debug.LogError(PhotonNetwork.player.mahPlayer.gameObject.name);

            photonView.RPC("BundlePlayer", PhotonTargets.Others, ids);
        }

        /// <summary>
        /// 同步玩家列表
        /// </summary>
        /// <param name="ids"></param>
        [PunRPC]
        public void BundlePlayer(int[] ids)
        {

            int ppindex = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                if (PhotonNetwork.player.ID == ids[i])
                {
                    ppindex = i;
                    break;
                }
            }

            for (int i = 0; i < 4; i++, ppindex++)
            {
                ppindex = ppindex % 4;
                players[i].photonPlayer = PhotonPlayer.Find(ids[ppindex]);
                PhotonPlayer.Find(ids[ppindex]).mahPlayer = players[i];
            }
            Debug.LogError(PhotonNetwork.player.mahPlayer.gameObject.name);
        }

        /// <summary>
        /// 摇骰子
        /// </summary>
        void Dice()
        {
            int a = UnityEngine.Random.Range(1, 6);
            int b = UnityEngine.Random.Range(1, 6);

            diceNum = a + b;
        }

        /// <summary>
        /// 发牌
        /// </summary>
        void DealingMahs()
        {
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                if (p.IsMasterClient)
                {
                    //主机调用自己的方法
                    for (int j = 0; j < 13; j++)
                    {
                        //TODO 依据骰子数判断取出的是哪一个麻将牌
                        PhotonNetwork.player.mahPlayer.keepedMah.Add(mahJong.allMah[0]);
                        mahJong.allMah.RemoveAt(0);
                    }
                    PhotonNetwork.player.mahPlayer.ShowUI();
                }
                else //主机对其他用户发牌
                {
                    List<int> mahsTOother = new List<int>();
                    for (int j = 0; j < 13; j++)
                    {
                        //TODO 依据骰子数判断取出的是哪一个麻将牌
                        mahsTOother.Add(mahJong.allMah[0]);
                        mahJong.allMah.RemoveAt(0);
                    }
                    RaiseEventOptions option = new RaiseEventOptions();
                    option.TargetActors = new int[] { p.ID };
                    PhotonNetwork.RaiseEvent(DEALINGCODE, mahsTOother.ToArray(), true, option);
                }
            }
        }

        //取牌
        public int GetMahjong(bool isfirst)
        {
            //牌光了 游戏结束
            if (mahJong.allMah.Count == 0)
            {
                photonView.RPC("GameOver", PhotonTargets.All, null);
            }

            int ID = 0;
            int index = 0;

            if (!isfirst)
            {
                index = mahJong.allMah.Count - 1;
            }

            ID = mahJong.allMah[index];
            mahJong.allMah.RemoveAt(index);

            photonView.RPC("RemoveOneMah", PhotonTargets.Others, new int[] { index });

            return ID;
        }

        [PunRPC]
        private void RemoveOneMah(int[] param)
        {
            int index = param[0];
            mahJong.allMah.RemoveAt(index);
        }

        [PunRPC]
        private void GameOver()
        {
            OverPanel.gameObject.SetActive(true);
        }

        [PunRPC]
        void ShowTimer(int[] param)
        {
            PhotonPlayer photonPlayer = PhotonPlayer.Find(param[0]);

            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                player.mahPlayer.timer.Hide();
            }
            photonPlayer.mahPlayer.timer.time = 10;
            photonPlayer.mahPlayer.timer.Show();
        }

        [PunRPC]
        void WinPai(int[] param)
        {
            Text text = OverPanel.GetComponentInChildren<Text>();
            OverPanel.gameObject.SetActive(true);

            PhotonPlayer photonPlayer = PhotonPlayer.Find(param[0]);

            text.text = "玩家" + photonPlayer.NickName + " 胡牌了!";
        }


        private MahPlayer NextPlayer(MahPlayer playerE)
        {
            int index = -1;

            MahPlayer player = players.Find(delegate (MahPlayer playerF)
            {
                index++;
                return playerF.name == playerE.name;
            });

            if (index == players.Count - 1)
            {
                index = 0;
            }
            else
            {
                index++;
            }

            return players[index];
        }

        private void OnEvent(byte eventcode, object content, int senderid)
        {
            //发牌
            if (eventcode == DEALINGCODE)
            {
                List<int> mahs = new List<int>((int[])content);
                PhotonNetwork.player.mahPlayer.keepedMah = mahs;
                PhotonNetwork.player.mahPlayer.ShowUI();

                for (int i = 0; i < 13 * 4; i++)
                {
                    mahJong.allMah.RemoveAt(0);
                }
            }
        }

        public void Back()
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel("Launcher");
        }

    }
}