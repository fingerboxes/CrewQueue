/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2015 Alexander Taylor
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using FingerboxLib;
using FingerboxLib.Interface;
using KSPPluginFramework;

namespace CrewQueue.Interface
{
    [KSPAddon(KSPAddon.Startup.EveryScene,true)]
    class AppLauncher : ProtoAppLauncher
    {        
        public override Texture AppLauncherIcon
        {
            get { return GameDatabase.Instance.GetTexture("Fingerboxes/CrewQueue/Icons/appLauncher", false); }
        }

        public override ApplicationLauncher.AppScenes Visibility
        {
            get
            {
                try
                {
                    bool coalescedCondition = (settingsWindow != null) &&
                                              (CrewQueueSettings.Instance.HideSettingsIcon == false || settingsWindow.Visible == true);

                    return coalescedCondition ? ApplicationLauncher.AppScenes.SPACECENTER : ApplicationLauncher.AppScenes.NEVER;
                }
                catch (Exception)
                {
                    return ApplicationLauncher.AppScenes.NEVER;
                }
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private SettingsWindow settingsWindow;

        protected override void onGUIApplicationLauncherReady()
        {
            settingsWindow = AddComponent<SettingsWindow>();
        }

        protected override void OnClick()
        {
            settingsWindow.Visible = true;
        }

        protected override void OnUnclick()
        {
            settingsWindow.Visible = false;
        }
    }
}
