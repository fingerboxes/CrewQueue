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


namespace FingerboxLib.Interface
{
    public abstract class ProtoAppLauncher : MonoBehaviourExtended
    {
        // Singleton boilerplate
        private static ProtoAppLauncher _instance;
        public static ProtoAppLauncher instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<ProtoAppLauncher>();
                }
                return _instance;
            }
        }
        
        private ApplicationLauncherButton button;

        public abstract Texture AppLauncherIcon { get; }
        public abstract ApplicationLauncher.AppScenes Visibility { get; set; }

        protected override void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(this);

            GameEvents.onGUIApplicationLauncherReady.Add(onGUIApplicationLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(onGUIApplicationLauncherDestroyed);

            Visibility = ApplicationLauncher.AppScenes.SPACECENTER;
        }

        void onGUIApplicationLauncherReady()
        {
            if (button == null)
            {
                ApplicationLauncher appLauncher = ApplicationLauncher.Instance;

                button = appLauncher.AddModApplication(OnClick,         // true
                                                       OnUnclick,       // false
                                                       OnHover,         // hover
                                                       OnUnhover,       // unhover
                                                       OnEnable,        // enable? what does this mean
                                                       OnDisable,       // disable? what does this mean
                                                       Visibility,
                                                       AppLauncherIcon);

                appLauncher.EnableMutuallyExclusive(button);
            }
        }

        void onGUIApplicationLauncherDestroyed()
        {
            button.SetFalse();
            button = null;
        }

        virtual protected void OnClick() { }

        virtual protected void OnUnclick() { }

        virtual protected void OnHover() { }

        virtual protected void OnUnhover() {}

        virtual protected void OnEnable() { }

        virtual protected void OnDisable() { }
    }
}
