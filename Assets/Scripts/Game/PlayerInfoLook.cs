// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2017 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Thomas Ricouard
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using System;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.Player;

namespace DaggerfallWorkshop.Game
{
	/// <summary>
	/// Display centered object info live.
	/// </summary>
	public class PlayerInfoLook : MonoBehaviour
	{
		PlayerGPS playerGPS;
		PlayerEnterExit playerEnterExit;        // Example component to enter/exit buildings
		GameObject mainCamera;

		public bool enabled = false;

		public float RayDistance = 75.0f;        // Distance of ray check, tune this to your scale and preference

		// Maximum distance from which different object types can be activated, in classic distance units
		public float DefaultLookDistance = 128;

		void Start()
		{
			playerGPS = GetComponent<PlayerGPS>();
			playerEnterExit = GetComponent<PlayerEnterExit>();
			mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}

		void Update()
		{
			if (enabled)
			{
				if (mainCamera == null)
					return;

				Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
				RaycastHit hit;
				bool hitSomething = Physics.Raycast(ray, out hit, RayDistance);
				if (hitSomething)
				{
					StaticNPC npc;
					MobilePersonNPC mobileNPC;
					DaggerfallEntityBehaviour enemyEntity;
                    DaggerfallActionDoor actionDoor;
					if (HitTest.NPCCheck(hit, out npc))
					{
						DaggerfallUI.SetMidScreenText(HardStrings.youSee.Replace("%s", npc.DisplayName));
					}
					else if (HitTest.MobilePersonMotorCheck(hit, out mobileNPC))
					{
						DaggerfallUI.SetMidScreenText(HardStrings.youSee.Replace("%s", mobileNPC.NameNPC));
					}
					else if (HitTest.MobileEnemyCheck(hit, out enemyEntity))
					{
						DaggerfallUI.SetMidScreenText(HardStrings.youSee.Replace("%s", enemyEntity.Entity.Name));
					}
                    else if (HitTest.ActionDoorCheck(hit, out actionDoor)) 
                    {
                        DaggerfallUI.SetMidScreenText(HardStrings.youSee.Replace("%s", "a door"));
                    }
					else
					{
						DaggerfallUI.SetMidScreenText("");
					}
				}
				else
				{
					DaggerfallUI.SetMidScreenText("");
				}
			}
		}
	}
}