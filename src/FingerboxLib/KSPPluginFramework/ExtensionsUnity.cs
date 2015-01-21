/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2014 David Tregoning
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

/*
 * Minor modifications to this code have been made from its original version, 
 * in order to make it suitable for this project.
 */

using System;
using UnityEngine;

namespace KSPPluginFramework
{
    /// <summary>
    /// CLass containing some extension methods for Unity Objects
    /// </summary>
    public static class UnityExtensions
    {
        /// <summary>
        /// Ensure that the Rect remains within the screen bounds
        /// </summary>
        public static Rect ClampToScreen(this Rect r)
        {
            return r.ClampToScreen(new RectOffset(0, 0, 0, 0));
        }

        /// <summary>
        /// Ensure that the Rect remains within the screen bounds
        /// </summary>
        /// <param name="ScreenBorder">A Border to the screen bounds that the Rect will be clamped inside (can be negative)</param>
        public static Rect ClampToScreen(this Rect r, RectOffset ScreenBorder)
        {
            r.x = Mathf.Clamp(r.x, ScreenBorder.left, Screen.width - r.width - ScreenBorder.right);
            r.y = Mathf.Clamp(r.y, ScreenBorder.top, Screen.height - r.height - ScreenBorder.bottom);
            return r;
        }

        public static GUIStyle PaddingChange(this GUIStyle g, Int32 PaddingValue)
        {
            GUIStyle gReturn = new GUIStyle(g);
            gReturn.padding = new RectOffset(PaddingValue, PaddingValue, PaddingValue, PaddingValue);
            return gReturn;
        }
        public static GUIStyle PaddingChangeBottom(this GUIStyle g, Int32 PaddingValue)
        {
            GUIStyle gReturn = new GUIStyle(g);
            gReturn.padding.bottom = PaddingValue;
            return gReturn;
        }

        /* 
         * Code below this point is original work, for this project. It is included in 
         * this source file and namespace for the sake of consistency and clarity 
         */

        /// <summary>
        /// Returns the equivalent AppScenes for the given GameScene
        /// </summary>
        public static ApplicationLauncher.AppScenes getAppScene(this GameScenes gameScene)
        {
            switch (gameScene)
            {
                case GameScenes.EDITOR:
                    return ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB;
                case GameScenes.FLIGHT:
                    return ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW;
                case GameScenes.SPACECENTER:
                    return ApplicationLauncher.AppScenes.SPACECENTER;
                case GameScenes.TRACKSTATION:
                    return ApplicationLauncher.AppScenes.TRACKSTATION;
                default:
                    return ApplicationLauncher.AppScenes.NEVER;
            }
        }

        /// <summary>
        /// Returns the LaunchSiteName for the given EditorFacility
        /// </summary>
        public static string getSiteName(this EditorFacility facility)
        {
            switch (facility)
            {
                case EditorFacility.SPH:
                    return "Runway";
                case EditorFacility.VAB:
                    return "LaunchPad";
                default:
                    return "Kerbal Space Center";
            }
        }

        public static string RemoveSuffix(this string s, string suffix)
        {
            if (s.EndsWith(suffix))
            {
                return s.Substring(0, s.Length - suffix.Length);
            }
            else
            {
                return s;
            }
        }
    }
}