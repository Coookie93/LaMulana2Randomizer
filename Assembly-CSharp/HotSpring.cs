using UnityEngine;
using L2Base;

namespace LM2RandomiserMod
{
    class HotSpring : MonoBehaviour
    {
        private Rect bounds;
        private Rect playerRect;
        private L2System sys;
        private int hpFrames = 0;

        private const float hpModifier = 0.0004f;

        public void Init(L2System sys, Vector2 pos)
        {
            this.sys = sys;
            bounds.width = 50;
            bounds.height = 50;
            bounds.center = pos;
            playerRect.width = 16;
            playerRect.height = 20;
        }

        public void Update()
        {
            var player = sys.getPlayer();
            if (player != null)
            {
                playerRect.center = player.getPlayerPositon();

                if (playerRect.Overlaps(bounds))
                {
                    hpFrames++;
                    if (hpFrames > 10)
                    {
                        hpFrames = 0;
                        int currentHP = sys.getPlayerHP();
                        int maxHP = Mathf.RoundToInt(sys.getPlayerMaxHP() * 0.01f);
                        if (currentHP < maxHP)
                        {
                            int hpToAdd = Mathf.RoundToInt(sys.getPlayerMaxHP() * hpModifier);

                            if (currentHP + hpToAdd > maxHP)
                                hpToAdd = maxHP - currentHP;

                            sys.setPLayerHP(hpToAdd);
                            sys.getL2SystemCore().seManager.playSE(player.gameObject, 152);
                        }
                    }
                }
            }
        }
    }
}
