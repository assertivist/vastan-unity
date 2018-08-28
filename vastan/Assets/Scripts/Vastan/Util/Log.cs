using System;
using UnityEngine;	
namespace Vastan.Util {
	class Log {
		static string logformat = "{0:u}| {1}";

		static string LogString(string message) 
		{
			return String.Format(logformat, DateTime.Now, message);
		}

		public static void Debug(string message)
		{
			if (UnityEngine.Debug.isDebugBuild)
			{
				UnityEngine.Debug.Log(LogString(message));
			}
		}

		public static void Debug(string message, params object[] things)
		{
			Debug(String.Format(message, things));
		}
		
		public static void Error(string message)
		{
			UnityEngine.Debug.LogError(LogString(message));
		}

		public static void Error(string message, params object[] things)
        {
			Error(String.Format(message, things));
        }
	}
}