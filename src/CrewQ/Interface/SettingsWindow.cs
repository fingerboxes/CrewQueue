using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSPPluginFramework;
using UnityEngine;
using FingerboxLib;

namespace CrewQ.Interface
{
    class SettingsWindow : MonoBehaviourWindow
    {
        private const string WINDOW_TITLE = "CrewQ(ueue) Settings";
        private const int BORDER_SIZE = 5;

        private const int WINDOW_WIDTH = 330, WINDOW_HEIGHT = 200;
        private const int COLUMN_A = BORDER_SIZE;
        private const int COLUMN_B = COLUMN_A + (WINDOW_WIDTH / 2);

        private string[] toggleCaptions = { "Automatically select crew", "Crew on vacation are ineligible for missions", "Use module type crew compositions", "<color=red>Permanently hide Settings Icon</color>" };

        private bool toggleAutoSelectCrew = true, toggleVacationLock, toggleCrewComp, togglePermHide;

        protected override void Awake()
        {
            ClampToScreenOffset = AppLauncher.instance.DefaultOffset;
            WindowRect.Set(AppLauncher.instance.ScreenPosition.x, AppLauncher.instance.ScreenPosition.y, WINDOW_WIDTH, WINDOW_HEIGHT);
            WindowCaption = WINDOW_TITLE;
        }

        protected override void DrawWindow(int id)
        {
            GUI.Box(new Rect(BORDER_SIZE, 30, WindowRect.width - (BORDER_SIZE * 2), WindowRect.height - (BORDER_SIZE * 2) - 30 - 30), "");
            bool accept = GUI.Button(new Rect(COLUMN_A, WINDOW_HEIGHT - 35, (WINDOW_WIDTH / 2) - (BORDER_SIZE * 2), 30), "Accept");
            bool cancel = GUI.Button(new Rect(COLUMN_B, WINDOW_HEIGHT - 35, (WINDOW_WIDTH / 2) - (BORDER_SIZE * 2), 30), "Cancel");
            

            toggleAutoSelectCrew = GUI.Toggle(new Rect(COLUMN_A, 40, WINDOW_WIDTH - (BORDER_SIZE * 2), 30), toggleAutoSelectCrew, toggleCaptions[0]);
            toggleVacationLock = GUI.Toggle(new Rect(COLUMN_A, 70, WINDOW_WIDTH - (BORDER_SIZE * 2), 30), toggleVacationLock, toggleCaptions[1]);
            toggleCrewComp = GUI.Toggle(new Rect(COLUMN_A, 100, WINDOW_WIDTH - (BORDER_SIZE * 2), 30), toggleCrewComp, toggleCaptions[2]);
            togglePermHide = GUI.Toggle(new Rect(COLUMN_A, 130, WINDOW_WIDTH - (BORDER_SIZE * 2), 30), togglePermHide, toggleCaptions[3]);
        }
    }
}
