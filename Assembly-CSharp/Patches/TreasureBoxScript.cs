using MonoMod;
using UnityEngine;

#pragma warning disable 0649
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("global::TreasureBoxScript")]
    public class patched_TreasureBoxScript : global::TreasureBoxScript
    {
		[MonoModIgnore]
		private Animator myAnime;

		[MonoModIgnore]
		private bool boxOpenFlag;

		[MonoModReplace]
		private void openBox()
		{
			this.sta = 7;
			if (!this.openState.Equals(string.Empty))
			{
				this.myAnime.enabled = true;
				this.myAnime.Play(this.openState);
			}
			else
			{
				this.myAnime.enabled = false;
			}
			this.sys.setEffectFlag(this.openActionFlags);
			if (this.sys.checkStartFlag(this.itemFlags))
			{
				if (this.itemObj != null)
				{
					AbstractItemBase component = this.itemObj.GetComponent<AbstractItemBase>();
					if (component.itemLabel == "Coin")
					{
						Vector3 actionPosition = this.actionPosition;
						actionPosition.z -= 5f;
						base.getL2Core().dropItemGenerator.dropCoins(ref actionPosition, component.itemValue);
					}
					else if (component.itemLabel == "Weight")
					{
						Vector3 actionPosition = this.actionPosition;
						actionPosition.z -= 5f;
						base.getL2Core().dropItemGenerator.dropWeight(ref actionPosition, 1);
						sys.setFlagData(31, component.itemValue, 1);
					}
					else if (!this.boxOpenFlag)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.itemObj, this.actionPosition, Quaternion.identity);
						component = gameObject.GetComponent<AbstractItemBase>();
						if (this.closetMode)
						{
							component.itemValue = this.costumeId;
						}
						component.initTask();
						component.setTreasureBoxOut();
						this.boxOpenFlag = true;
					}
				}
				else if (this.dropitem != null)
				{
					this.dropitem.popDropItem();
				}
			}
		}
	}
}
