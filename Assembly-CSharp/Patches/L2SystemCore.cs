using System;
using MonoMod;
using L2Base;
using UnityEngine;

#pragma warning disable 0649, 0414, 0108
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("global::L2SystemCore")]
    public class patched_L2SystemCore: L2SystemCore
	{
		private bool fadeInFlag;
		private bool scrollSystemInited;
		private AsyncOperation asOpe;
		private FairyBaseScript activeFairy;

		[MonoModReplace]
		public bool setAsyncScene(bool sysflgReset = true)
		{
			if (this.asOpe == null)
			{
				if (this.fadeInFlag)
				{
					this.gameScreenFadeIn(10);
				}
				this.jumpPlayerPositionToPresetPosition();
				if (cartenObj != null)
					Destroy(cartenObj);

				return true;
			}
			if (this.asOpe.progress < 0.9f)
			{
				return false;
			}
			this.sys.clearSceneTasks();
			if (this.sys.getPlayer() != null)
			{
				this.sys.getPlayer().transform.SetParent(base.transform);
			}
			if (this.activeFairy != null)
			{
				this.activeFairy.transform.SetParent(base.transform);
			}
			this.scrollSys = null;
			if (sysflgReset)
			{
				this.sys.delSysFlag(SYSTEMFLAG.DRAMATEJI);
			}
			GC.Collect();
			this.sys.setTaskRuningMode(false);
			if (this.cartenFirstFrame)
			{
				this.cartenFirstFrame = false;
				if (this.cartenObj != null)
				{
					MapCartain component = this.cartenObj.GetComponent<MapCartain>();
					component.initCartain();
				}
			}
			this.setMenuCameraActive(false);
			this.scrollSystemInited = false;
			this.asOpe.allowSceneActivation = true;
			return true;
		}
	}
}
