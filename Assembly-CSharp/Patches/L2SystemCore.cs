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
			if (asOpe == null)
			{
				if (fadeInFlag)
				{
					gameScreenFadeIn(10);
				}
				jumpPlayerPositionToPresetPosition();
				if (cartenObj != null)
					Destroy(cartenObj);

				return true;
			}
			if (asOpe.progress < 0.9f)
			{
				return false;
			}
			sys.clearSceneTasks();
			if (sys.getPlayer() != null)
			{
				sys.getPlayer().transform.SetParent(base.transform);
			}
			if (activeFairy != null)
			{
				activeFairy.transform.SetParent(base.transform);
			}
			scrollSys = null;
			if (sysflgReset)
			{
				sys.delSysFlag(SYSTEMFLAG.DRAMATEJI);
			}
			GC.Collect();
			sys.setTaskRuningMode(false);
			if (cartenFirstFrame)
			{
				cartenFirstFrame = false;
				if (cartenObj != null)
				{
					MapCartain component = cartenObj.GetComponent<MapCartain>();
					component.initCartain();
				}
			}
			setMenuCameraActive(false);
			scrollSystemInited = false;
			asOpe.allowSceneActivation = true;
			return true;
		}
	}
}
