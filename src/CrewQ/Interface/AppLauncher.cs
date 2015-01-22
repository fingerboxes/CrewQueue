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
using KSPPluginFramework;
using FingerboxLib;


namespace CrewQ.Interface
{
    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    class AppLauncher : MonoBehaviourExtended
    {
        // Singleton boilerplate
        private static AppLauncher _instance;
        public static AppLauncher instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<AppLauncher>();
                }
                return _instance;
            }
        }

        private ApplicationLauncherButton button;
        private ApplicationLauncher.AppScenes visibility;

        protected override void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(this);

            GameEvents.onGUIApplicationLauncherReady.Add(onGUIApplicationLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(onGUIApplicationLauncherDestroyed);

            visibility = ApplicationLauncher.AppScenes.SPACECENTER;
        }

        void onGUIApplicationLauncherReady()
        {
            if (button == null)
            {
                ApplicationLauncher appLauncher = ApplicationLauncher.Instance;

                button = appLauncher.AddModApplication(OnClick,         // true
                                                       OnUnclick,       // false
                                                       null,            // hover
                                                       null,            // unhover
                                                       null,            // enable? what does this mean
                                                       null,            // disable? what does this mean
                                                       visibility,
                                                       GameDatabase.Instance.GetTexture("Fingerboxes/CrewQ/Icons/appLauncher",false));

                appLauncher.EnableMutuallyExclusive(button);
            }
        }

        void onGUIApplicationLauncherDestroyed()
        {
            button = null;
        }

        void OnClick()
        {

        }

        void OnUnclick()
        {

        }
    }
}
