﻿/*
 * Created by SharpDevelop.
 * DateCreated: 31.08.2017
 * Time: 20:32
 *
 */

using Microsoft.UI.Xaml;
using System;
using System.IO;

namespace Log4CSharp
{
    /// <summary>
    /// Description of LogWriter.
    /// </summary>
    public static class LogWriter
    {
        private static StreamWriter textStream;
        private static readonly string FacilityName = Application.Current.GetType().Name;

        private static readonly string _logFileName = "log.txt";

        /// <summary>
        ///
        /// </summary>
        /// <param name="entry"></param>
        public static void CreateLogEntry(string entry)
        {
            var _logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FacilityName, "log");

            if (!Directory.Exists(_logFilePath))
            {
                Directory.CreateDirectory(_logFilePath);
            }

            try
            {
                if (!File.Exists(Path.Combine(_logFilePath, _logFileName)))
                {
                    textStream = File.CreateText(Path.Combine(_logFilePath, _logFileName));
                }
                else
                {
                    textStream = File.AppendText(Path.Combine(_logFilePath, _logFileName));
                }

                textStream.WriteAsync(string.Format("{0}" + Environment.NewLine, entry.Replace("\r\n", "; ")));
                textStream.Close();
                textStream.Dispose();
            }
            catch
            {
                throw new Exception("ERROR 0007: Unable to Create Logfile...");
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="entry"></param>
        public static void CreateLogEntry(Exception e)
        {
            var _logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FacilityName, "log");

            if (!Directory.Exists(_logFilePath))
            {
                Directory.CreateDirectory(_logFilePath);
            }

            try
            {
                if (!File.Exists(Path.Combine(_logFilePath, _logFileName)))
                {
                    textStream = File.CreateText(Path.Combine(_logFilePath, _logFileName));
                }
                else
                {
                    textStream = File.AppendText(Path.Combine(_logFilePath, _logFileName));
                }

                textStream.WriteAsync(
                    string.Format("{0}" + Environment.NewLine, 
                    string.Format("{0}: {1}; {2}", DateTime.Now, e.Message, e.InnerException != null ? e.InnerException.Message : "").Replace("\r\n", "; ")));
                textStream.Close();
                textStream.Dispose();
            }
            catch
            {
            }
        }
    }
}