// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2017 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Thomas Ricouard (Dimillian)
// Contributors:    
// 
// Notes: Alternative Inventory window
//

using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DaggerfallConnect.Save;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.Banking;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
	/// <summary>
	/// Daggerfall Unity inventory game interface.
	/// </summary>
	public class DaggerfallUnityInventoryWindow : DaggerfallPopupWindow
	{
		#region UI Rects

		Vector2 mainPanelSize = new Vector2(280, 170);

		#endregion

		#region UI Controls

		const string savePromptText = "Save Game";
		const string loadPromptText = "Load Game";
		const string saveButtonText = "Save";
		const string loadButtonText = "Load";

		Panel mainPanel = new Panel();
		TextLabel promptLabel = new TextLabel();
		ListBox itemsList = new ListBox();

		Color mainPanelBackgroundColor = new Color(0.0f, 0f, 0.0f, 1.0f);
		Color namePanelBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		Color saveButtonBackgroundColor = new Color(0.0f, 0.5f, 0.0f, 0.4f);
		Color cancelButtonBackgroundColor = new Color(0.7f, 0.0f, 0.0f, 0.4f);
		Color savesListBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);
		Color savesListTextColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
		Color saveFolderColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
		VerticalScrollBar savesScroller = new VerticalScrollBar();

		string currentPlayerName;
		bool displayMostRecentChar;

		#endregion


		#region Fields

		const string baseTextureName = "INVE00I0.IMG";
		const string goldTextureName = "INVE01I0.IMG";
		const string greenArrowsTextureName = "INVE06I0.IMG";           // Green up/down arrows when more items available
		const string redArrowsTextureName = "INVE07I0.IMG";             // Red up/down arrows when no more items available
		const int listDisplayUnits = 4;                                 // Number of items displayed in scrolling areas
		const int accessoryCount = 12;                                  // Number of accessory slots
		const int itemButtonMarginSize = 2;                             // Margin of item buttons
		const int accessoryButtonMarginSize = 1;                        // Margin of accessory buttons

		PlayerEntity playerEntity;

		TabPages selectedTabPage = TabPages.WeaponsAndArmor;
		ActionModes selectedActionMode = ActionModes.Equip;
		RemoteTargetTypes remoteTargetType = RemoteTargetTypes.Dropped;

		ItemCollection localItems = null;
		ItemCollection remoteItems = null;
		ItemCollection droppedItems = new ItemCollection();
		List<DaggerfallUnityItem> localItemsFiltered = new List<DaggerfallUnityItem>();
		List<DaggerfallUnityItem> remoteItemsFiltered = new List<DaggerfallUnityItem>();

		DaggerfallLoot lootTarget = null;
		bool usingWagon = false;

		ItemCollection lastRemoteItems = null;
		RemoteTargetTypes lastRemoteTargetType;

		int lastMouseOverPaperDollEquipIndex = -1;

		ItemCollection.AddPosition preferredOrder = ItemCollection.AddPosition.DontCare;

		#endregion

		#region Enums

		enum TabPages
		{
			WeaponsAndArmor,
			MagicItems,
			ClothingAndMisc,
			Ingredients,
		}

		enum RemoteTargetTypes
		{
			Dropped,
			Wagon,
			Loot,
		}

		enum ActionModes
		{
			Info,
			Equip,
			Remove,
			Use,
		}

		#endregion

		#region Properties

		public PlayerEntity PlayerEntity
		{
			get { return (playerEntity != null) ? playerEntity : playerEntity = GameManager.Instance.PlayerEntity; }
		}

		/// <summary>
		/// Gets or sets specific loot to view on next open.
		/// Otherwise will default to ground for dropping items.
		/// </summary>
		public DaggerfallLoot LootTarget
		{
			get { return lootTarget; }
			set { lootTarget = value; }
		}

		#endregion

		#region Constructors

		public DaggerfallUnityInventoryWindow(IUserInterfaceManager uiManager)
			: base(uiManager)
		{
	
		}

		#endregion

		#region Setup Methods

		protected override void Setup()
		{
			// Main panel
			mainPanel.HorizontalAlignment = HorizontalAlignment.Center;
			mainPanel.VerticalAlignment = VerticalAlignment.Middle;
			mainPanel.Size = mainPanelSize;
			mainPanel.Outline.Enabled = true;
			if (TextureReplacement.CustomTextureExist("mainPanelBackgroundColor"))
				mainPanel.BackgroundTexture = TextureReplacement.LoadCustomTexture("mainPanelBackgroundColor");
			else
				mainPanel.BackgroundColor = mainPanelBackgroundColor;
			NativePanel.Components.Add(mainPanel);

			// Prompt
			promptLabel.ShadowPosition = Vector2.zero;
			promptLabel.Position = new Vector2(4, 4);
			mainPanel.Components.Add(promptLabel);
			promptLabel.Text = "Inventory";

			// items panel
			Panel savesPanel = new Panel();
			savesPanel.Position = new Vector2(4, 10);
			savesPanel.Size = new Vector2(100, 141);
			savesPanel.Outline.Enabled = true;
			mainPanel.Components.Add(savesPanel);

			// items list
			itemsList.Position = new Vector2(2, 2);
			itemsList.Size = new Vector2(91, 129);
			itemsList.TextColor = savesListTextColor;
			if (TextureReplacement.CustomTextureExist("savesListBackgroundColor"))
				itemsList.BackgroundTexture = TextureReplacement.LoadCustomTexture("savesListBackgroundColor");
			else
				itemsList.BackgroundColor = savesListBackgroundColor;
			itemsList.ShadowPosition = Vector2.zero;
			itemsList.RowsDisplayed = 16;
			savesPanel.Components.Add(itemsList);

			// items scroller
			savesScroller.Position = new Vector2(94, 2);
			savesScroller.Size = new Vector2(5, 129);
			savesScroller.DisplayUnits = 16;
			savesPanel.Components.Add(savesScroller);

			FilterLocalItems();
			UpdateLocalItemsDisplay();
	
		}

		public override void OnPush()
		{
			base.OnPush();
			base.Update();  // Ensures controls are properly sized for text label clipping

			localItems = PlayerEntity.Items;

			// Always start window with current player name
			currentPlayerName = GameManager.Instance.PlayerEntity.Name;

		}

		public override void Update()
		{
			base.Update();

		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Creates filtered list of local items based on view state.
		/// </summary>
		void FilterLocalItems()
		{
			// Clear current references
			localItemsFiltered.Clear();

			// Do nothing if no items
			if (localItems == null || localItems.Count == 0)
				return;

			// Add items to list
			for (int i = 0; i < localItems.Count; i++)
			{
				DaggerfallUnityItem item = localItems.GetItem(i);

				// Reject if equipped
				if (item.IsEquipped)
					continue;

				bool isWeaponOrArmor = (item.ItemGroup == ItemGroups.Weapons || item.ItemGroup == ItemGroups.Armor);

				// Add based on view
				if (selectedTabPage == TabPages.WeaponsAndArmor)
				{
					// Weapons and armor
					if (isWeaponOrArmor && !item.IsEnchanted)
						localItemsFiltered.Add(item);
				}
				else if (selectedTabPage == TabPages.MagicItems)
				{
					// Enchanted items
					if (item.IsEnchanted)
						localItemsFiltered.Add(item);
				}
				else if (selectedTabPage == TabPages.Ingredients)
				{
					// Ingredients
					if (item.IsIngredient && !item.IsEnchanted)
						localItemsFiltered.Add(item);
				}
				else if (selectedTabPage == TabPages.ClothingAndMisc)
				{
					// Everything else
					if (!isWeaponOrArmor && !item.IsEnchanted && !item.IsIngredient)
						localItemsFiltered.Add(item);
				}
			}
		}

		/// <summary>
		/// Creates filtered list of remote items.
		/// For now this just creates a flat list, as that is Daggerfall's behaviour.
		/// </summary>
		void FilterRemoteItems()
		{
			// Clear current references
			remoteItemsFiltered.Clear();

			// Do nothing if no items
			if (remoteItems == null || remoteItems.Count == 0)
				return;

			// Add items to list
			for (int i = 0; i < remoteItems.Count; i++)
			{
				DaggerfallUnityItem item = remoteItems.GetItem(i);
				remoteItemsFiltered.Add(item);
			}
		}

		/// <summary>
		/// Updates local items display.
		/// </summary>
		void UpdateLocalItemsDisplay()
		{
			// Clear list elements
			itemsList.ClearItems();
			if (localItemsFiltered == null)
				return;

		
			// Update images and tooltips
			for (int i = 0; i < listDisplayUnits; i++)
			{
				
				// Get item and image
				DaggerfallUnityItem item = localItemsFiltered[i];
				itemsList.AddItem (item.LongName);
			}
		}
			
						
		#endregion
			
	}
}