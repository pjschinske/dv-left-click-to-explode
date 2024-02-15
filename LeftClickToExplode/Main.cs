using System;
using System.Reflection;
using CommsRadioAPI;
using HarmonyLib;
using LeftClickToExplode.CommsRadioStates;
using UnityEngine;
using UnityModManagerNet;

namespace LeftClickToExplode
{
	public static class Main
	{
		public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
		public static CommsRadioMode CommsRadioMode { get; private set; }

		// Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
		private static bool Load(UnityModManager.ModEntry modEntry)
		{
			Harmony? harmony = null;
			Logger = modEntry.Logger;

			try
			{
				harmony = new Harmony(modEntry.Info.Id);
				harmony.PatchAll(Assembly.GetExecutingAssembly());


				// Other plugin startup logic
				CommsRadioAPI.ControllerAPI.Ready += InitCommsRadioPage;
				
			}
			catch (Exception ex)
			{
				modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
				harmony?.UnpatchAll(modEntry.Info.Id);
				return false;
			}

			return true;
		}

		public static void InitCommsRadioPage()
		{
			CommsRadioMode = CommsRadioMode.Create(new PointingAtNothingStateBehaviour(), Color.red);
		}
	}

}
