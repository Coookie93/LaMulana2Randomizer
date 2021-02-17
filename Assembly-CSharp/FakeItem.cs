using L2Base;
using L2Flag;
using UnityEngine;

namespace LM2RandomiserMod
{
    public class FakeItem : MonoBehaviour
    {
        private Rect bounds;
        private Rect playerRect;
        private L2System sys;
        private int flagNo;
        private L2FlagBoxParent[] activeFlags;

        public void Init(L2System system, L2FlagBoxParent[] flags, Vector3 pos, int flag)
        {
            sys = system;
            bounds.size = new Vector2(20, 20);
            bounds.center = new Vector2(pos.x, pos.y);
            playerRect.size = new Vector2(16, 20);
            activeFlags = flags;
            flagNo = flag;
        }

        public void Update()
        {
            if (sys.checkStartFlag(activeFlags))
            {
                var sprite = GetComponent<SpriteRenderer>();
                sprite.enabled = true;

                var player = sys.getPlayer();
                if (player != null)
                {
                    Vector3 pPos = player.getPlayerPositon();
                    pPos.y += 15f;
                    playerRect.center = pPos;

                    if (playerRect.Overlaps(bounds))
                    {
                        sys.setFlagData(31, flagNo, 1);
                        sys.setFlagData(1, 12, 1);
                        gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
