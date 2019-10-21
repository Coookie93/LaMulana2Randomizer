using UnityEngine;
using L2Base;
using L2Flag;

namespace LM2RandomiserMod
{
    public class DevUI : MonoBehaviour
    {
        private L2System sys;

        bool showUI = true;

        private string areaString;
        private string screenXString;
        private string screenYString;
        private string posXString;
        private string posYString;
        private bool sceneJump = true;
        private BGScrollSystem currentBGSys;

        private string sheetString;
        private string flagString;
        private string valueString;

        private string getSheetString;
        private string getFlagString;
        private string getValueString;

        public void Initialise(L2System l2System)
        {
            sys = l2System;
            Cursor.visible = true;
        }

        void OnGUI() {

            if (showUI && sys.getPlayer() != null)
            {
                areaString = GUI.TextArea(new Rect(0, 0, 100, 25), areaString);
                screenXString = GUI.TextArea(new Rect(0, 25, 50, 25), screenXString);
                screenYString = GUI.TextArea(new Rect(50, 25, 50, 25), screenYString);
                posXString = GUI.TextArea(new Rect(0, 50, 50, 25), posXString);
                posYString = GUI.TextArea(new Rect(50, 50, 50, 25), posYString);

                if (GUI.Button(new Rect(0, 75, 100, 25), "Warp"))
                {
                    DoDebugWarp();
                }

                sheetString = GUI.TextArea(new Rect(100, 0, 100, 25), sheetString);
                flagString = GUI.TextArea(new Rect(100, 25, 100, 25), flagString);
                valueString = GUI.TextArea(new Rect(100, 50, 100, 25), valueString);

                if (GUI.Button(new Rect(100, 75, 100, 25), "Set Flag"))
                {
                    SetFlag();
                }

                getSheetString = GUI.TextArea(new Rect(200, 0, 100, 25), getSheetString);
                getFlagString = GUI.TextArea(new Rect(200, 25, 100, 25), getFlagString);
                getValueString = GUI.TextArea(new Rect(200, 50, 100, 25), getValueString);

                if (GUI.Button(new Rect(200, 75, 100, 25), "Get Flag"))
                {
                    GetFlag();
                }
                
                sys.setPandaModeHP(GUI.Toggle(new Rect(300, 0, 120, 25), sys.getPandaModeHP(), "Panda Mode"));
                sys.setPandaModeHit(GUI.Toggle(new Rect(300, 25, 120, 25), sys.getPandaModeHit(), "Panda Hit Mode"));
            }
        }

        
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F10))
            {
                showUI = !showUI;
            }

            UpdateBGSys();
        }

        private void UpdateBGSys()
        {
            if (sceneJump)
            {
                currentBGSys = sys.getL2SystemCore().ScrollSystem;
                if (currentBGSys != null)
                {
                    sceneJump = false;
                    UpdatePositionInfo();
                }
            }
        }

        private void UpdatePositionInfo()
        {
            L2SystemCore sysCore = sys.getL2SystemCore();
            GameObject playerObj = sys.getPlayer().gameObject;
            Vector3 position = playerObj.transform.position;
            ViewProperty currentView = currentBGSys.roomSetter.getCurrentView(position.x, position.y);
            int currentScene = sysCore.SceaneNo;
            areaString = currentScene.ToString();
            float num;
            float num2;
            if (currentView == null)
            {
                screenXString = "-1";
                screenYString = "-1";
                num = 0f;
                num2 = 0f;
            }
            else
            {
                screenXString = currentView.ViewX.ToString();
                screenYString = currentView.ViewY.ToString();
                num = position.x - currentView.ViewLeft;
                num2 = position.y - currentView.ViewBottom;
            }
            int num3 = (int)Mathf.Round(num * (float)BGAbstractScrollController.NumberCls);
            int num4 = (int)Mathf.Round(num2 * (float)BGAbstractScrollController.NumberCls);
            num3 /= 80;
            num4 /= 80;
            posXString = num3.ToString();
            posYString = num4.ToString();
        }

        private void SetFlag()
        {
            int sheet = int.Parse(sheetString);
            int flag = int.Parse(flagString);
            short value = short.Parse(valueString);

            sys.setFlagData(sheet, flag, value);
        }

        private void GetFlag()
        {
            int sheet = int.Parse(getSheetString);
            int flag = int.Parse(getFlagString);
            sys.getFlagSys().getFlagBaseObject(sheet, flag, out L2FlagBase l2Flag);
            getValueString = l2Flag.flagValue.ToString();
        }

        private void DoDebugWarp()
        {
            int area = int.Parse(areaString);
            int screenX = int.Parse(screenXString);
            int screenY = int.Parse(screenYString);
            int posX = int.Parse(posXString);
            int posY = int.Parse(posYString);

            L2SystemCore sysCore = sys.getL2SystemCore();

            sysCore.setJumpPosition(screenX, screenY, posX, posY, 0f);
            if (sysCore.SceaneNo != area)
            {
                sysCore.gameScreenFadeOut(10);
                sysCore.setFadeInFlag(true);
                sysCore.changeFieldSceane(area, true, false);
                sceneJump = true;
            }
            else
            {
                JumpPosition();
                UpdatePositionInfo();
            }
        }

        private void JumpPosition()
        {
            if (this.sceneJump)
            {
                return;
            }
            Vector3 vector = default(Vector3);
            L2SystemCore sysCore = sys.getL2SystemCore();
            if (sysCore.getJumpPosition(out vector))
            {
                sysCore.L2Sys.movePlayer(vector);
                currentBGSys.setPlayerPosition(vector, false);
                sysCore.resetFairy();
                currentBGSys.forceResetCameraPosition();
            }
        }
    }
}
