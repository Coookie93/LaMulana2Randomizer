using System;
using MonoMod;
using UnityEngine;

#if DEV

namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("global::ItemPotScript")]
    public class patched_ItemPotScript : global::ItemPotScript
    {
        [NonSerialized] public Camera[] cams;
        [NonSerialized] public int camIndex;
        [NonSerialized] public Vector3 worldPos;

        [NonSerialized] public Font currentFont = null;
        [NonSerialized] public bool showText = true;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
                showText = !showText;
        }

        public void OnGUI()
        {
            if (showText)
            {
                if (cams == null || cams.Length == 0)
                {
                    cams = FindObjectsOfType<Camera>();
                    return;
                }

                Camera camera = null;
                foreach (var cam in cams)
                {
                    if (cam.gameObject.name == "ExtCamera")
                        camera = cam;
                }

                if (camera == null || exItemPrefab == null)
                    return;

                var centerY = Screen.height / 2;
                worldPos = camera.WorldToScreenPoint(transform.position);

                if (worldPos.y <= centerY)
                {
                    var distToCenter = centerY - worldPos.y;
                    worldPos.Set(worldPos.x, distToCenter + centerY, worldPos.z);
                }
                else
                {
                    var distToCenter = worldPos.y - centerY;
                    worldPos.Set(worldPos.x, centerY - distToCenter, worldPos.z);
                }

                AbstractItemBase component = exItemPrefab.GetComponent<AbstractItemBase>();

                if (component == null)
                    return;

                if (currentFont == null)
                    currentFont = Font.CreateDynamicFontFromOSFont("Consolas", 14);

                var guistyle = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold
                };
                guistyle.normal.textColor = Color.white;
                guistyle.font = currentFont;

                GUI.Label(new Rect(worldPos, new Vector3(100f, 100f)),
                    $"{component.itemLabel ?? "unknown"} ({component.itemValue})",
                    guistyle);
            }
        }
    }
}

#endif