/*
 * Created by SharpDevelop.
 * Date: 12.10.2017
 * Time: 11:21
 *
 */

using System;

namespace CardCheckAssistant.DataAccessLayer
{
    public static class Constants
    {
        public static readonly int MAX_WAIT_INSERTION = 200; //timeout for chip response in ms
        public static readonly string TITLE_SUFFIX = ""; //turns out special app versions
                                                         //public const string TITLE_SUFFIX = "DEVELOPER PREVIEW"; //turns out special app versions
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum WindowThemes
    {
        Light = 0,
        Dark = 1,
        Default = 2
    }
}

