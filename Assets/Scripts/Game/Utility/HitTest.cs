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
	/// Example class to handle activation of doors, switches, etc. from Fire1 input.
	/// </summary>
	public class HitTest
	{
		// Check if raycast hit a static door
		public static bool StaticDoorCheck(RaycastHit hitInfo, out DaggerfallStaticDoors door)
		{
			door = hitInfo.transform.GetComponent<DaggerfallStaticDoors>();
			if (door == null)
				return false;

			return true;
		}

		// Check if raycast hit an action door
		public static bool ActionDoorCheck(RaycastHit hitInfo, out DaggerfallActionDoor door)
		{
			door = hitInfo.transform.GetComponent<DaggerfallActionDoor>();
			if (door == null)
				return false;

			return true;
		}

		// Check if raycast hit a generic action component
		public static bool ActionCheck(RaycastHit hitInfo, out DaggerfallAction action)
		{
			// Look for action
			action = hitInfo.transform.GetComponent<DaggerfallAction>();
			if (action == null)
				return false;
			else
				return true;
		}

		// Check if raycast hit a lootable object
		public static bool LootCheck(RaycastHit hitInfo, out DaggerfallLoot loot)
		{
			loot = hitInfo.transform.GetComponent<DaggerfallLoot>();
			if (loot == null)
				return false;
			else
				return true;
		}

		// Check if raycast hit a StaticNPC
		public static bool NPCCheck(RaycastHit hitInfo, out StaticNPC staticNPC)
		{
			staticNPC = hitInfo.transform.GetComponent<StaticNPC>();
			if (staticNPC != null)
				return true;
			else
				return false;
		}

		// Check if raycast hit a mobile NPC
		public static bool MobilePersonMotorCheck(RaycastHit hitInfo, out MobilePersonNPC mobileNPC)
		{
			mobileNPC = hitInfo.transform.GetComponent<MobilePersonNPC>();
			if (mobileNPC != null)
				return true;
			else
				return false;
		}

		// Check if raycast hit a mobile enemy
		public static bool MobileEnemyCheck(RaycastHit hitInfo, out DaggerfallEntityBehaviour mobileEnemy)
		{
			mobileEnemy = hitInfo.transform.GetComponent<DaggerfallEntityBehaviour>();
			if (mobileEnemy != null)
				return true;
			else
				return false;
		}

		// Check if raycast hit a QuestResource
		public static bool QuestResourceBehaviourCheck(RaycastHit hitInfo, out QuestResourceBehaviour questResourceBehaviour)
		{
			questResourceBehaviour = hitInfo.transform.GetComponent<QuestResourceBehaviour>();
			if (questResourceBehaviour != null)
				return true;
			else
				return false;
		}

	}

}