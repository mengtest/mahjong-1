using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.Desktop
{
    public class MahJongTools
    {

        public static bool IsCanHU(List<int> mah, int ID)
        {
            List<int> pais = new List<int>(mah);

            pais.Add(ID);
            //只有两张牌
            if (pais.Count == 2)
            {
                return pais[0] == pais[1];
            }

            //先分出一对将
            pais.Sort();

            for (int i = 0; i < pais.Count; i++)
            {
                List<int> paiT = new List<int>(pais);
                List<int> ds = pais.FindAll(delegate (int d)
                {
                    return pais[i] == d;
                });

                if (ds.Count >= 2)
                {
                    paiT.Remove(pais[i]);
                    paiT.Remove(pais[i]);
                    i++;

                    if (HuPaiPanDin(paiT))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HuPaiPanDin(List<int> mahs)
        {
            if (mahs.Count == 0)
            {
                return true;
            }

            List<int> fs = mahs.FindAll(delegate (int a)
            {
                return mahs[0] == a;
            });

            if (fs.Count == 3)
            {
                mahs.Remove(mahs[0]);
                mahs.Remove(mahs[0]);
                mahs.Remove(mahs[0]);

                return HuPaiPanDin(mahs);
            }
            else
            {
                if (mahs.Contains(mahs[0] + 1) && mahs.Contains(mahs[0] + 2))
                {
                    mahs.Remove(mahs[0] + 2);
                    mahs.Remove(mahs[0] + 1);
                    mahs.Remove(mahs[0]);

                    return HuPaiPanDin(mahs);
                }

                return false;
            }

        }

        /// <summary>
        /// 判断是否能碰牌的ID为MahID的牌
        /// </summary>
        /// <param name="mahs">手牌</param>
        /// <param name="MahID">被打出来的牌</param>
        /// <param name="state">当前玩家状态</param>
        /// <returns>能不能杠牌</returns>
        public static bool IsCanPon(List<int> mahs, int MahID, MahPlayer.STATE state)
        {
            if (state != MahPlayer.STATE.waiting)
            {
                return false;
            }
            bool isCanPon = false;

            List<int> currentMahes = mahs.FindAll(delegate (int a)
            {
                return a == MahID;
            });

            if (currentMahes.Count >= 2)
            {
                isCanPon = true;
            }

            return isCanPon;
        }

        /// <summary>
        /// 判断是否能杠ID为MahID的牌
        /// </summary>
        /// <param name="mahs">手牌</param>
        /// <param name="MahID">被打出来的牌</param>
        /// <param name="state">当前玩家状态</param>
        /// <returns>能不能杠牌</returns>
        public static bool IsCanGang(List<int> mahs, int MahID, MahPlayer.STATE state)
        {
            if (state != MahPlayer.STATE.waiting)
            {
                return false;
            }
            bool isCanPon = false;

            List<int> currentMahes = mahs.FindAll(delegate (int a)
            {
                return a == MahID;
            });

            if (currentMahes.Count == 3)
            {
                isCanPon = true;
            }

            return isCanPon;
        }

    }
}