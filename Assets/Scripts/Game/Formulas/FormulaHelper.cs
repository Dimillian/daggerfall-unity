﻿// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2016 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using System;

namespace DaggerfallWorkshop.Game.Formulas
{
    /// <summary>
    /// Common formulas used throughout game.
    /// Where the exact formula is unknown, a "best effort" approximation will be used.
    /// Will likely be migrated to a pluggable IFormulaProvider at a later date.
    /// </summary>
    public static class FormulaHelper
    {
        #region Basic Formulas

        public static int DamageModifier(int strength)
        {
            return (int)Mathf.Floor((float)(strength - 50) / 5f);
        }

        public static int MaxEncumbrance(int strength)
        {
            return (int)Mathf.Floor((float)strength * 1.5f);
        }

        public static int SpellPoints(int intelligence, float multiplier)
        {
            return (int)Mathf.Floor((float)intelligence * multiplier);
        }

        public static int MagicResist(int willpower)
        {
            return (int)Mathf.Floor((float)willpower / 10f);
        }

        public static int ToHitModifier(int agility)
        {
            return (int)Mathf.Floor((float)agility / 10f) - 5;
        }

        public static int HitPointsModifier(int endurance)
        {
            return (int)Mathf.Floor((float)endurance / 10f) - 5;
        }

        public static int HealingRateModifier(int endurance)
        {
            // Original Daggerfall seems to have a bug where negative endurance modifiers on healing rate
            // are applied as modifier + 1. Not recreating that here.
            return (int)Mathf.Floor((float)endurance / 10f) - 5;
        }

        #endregion

        #region Player

        // Generates player health based on level and career hit points per level
        public static int RollMaxHealth(int level, int hitPointsPerLevel)
        {
            const int baseHealth = 25;
            int maxHealth = baseHealth + hitPointsPerLevel;

            for (int i = 1; i < level; i++)
            {
                maxHealth += UnityEngine.Random.Range(1, hitPointsPerLevel + 1);
            }

            return maxHealth;
        }

        // Calculate how much health the player should recover per hour of rest
        public static int CalculateHealthRecoveryRate(Entity.PlayerEntity player)
        {
            short medical = player.Skills.Medical;
            int endurance = player.Stats.Endurance;
            int maxHealth = player.MaxHealth;
            PlayerEnterExit playerEnterExit;
            playerEnterExit = GameManager.Instance.PlayerGPS.GetComponent<PlayerEnterExit>();
            DaggerfallConnect.DFCareer.RapidHealingFlags rapidHealingFlags = player.Career.RapidHealing;

            short addToMedical = 60;

            if (rapidHealingFlags == DaggerfallConnect.DFCareer.RapidHealingFlags.Always)
                addToMedical = 100;
            else if (DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.IsDay && !playerEnterExit.IsPlayerInside)
            {
                if (rapidHealingFlags == DaggerfallConnect.DFCareer.RapidHealingFlags.InLight)
                    addToMedical = 100;
            }
            else if (rapidHealingFlags == DaggerfallConnect.DFCareer.RapidHealingFlags.InDarkness)
                addToMedical = 100;

            medical += addToMedical;

            return Mathf.Max((int)Mathf.Floor(HealingRateModifier(endurance) + medical * maxHealth / 1000), 1);
        }

        // Calculate how much fatigue the player should recover per hour of rest
        public static int CalculateFatigueRecoveryRate(int maxFatigue)
        {
            return Mathf.Max((int)Mathf.Floor(maxFatigue / 8), 1);
        }

        // Calculate how many spell points the player should recover per hour of rest
        public static int CalculateSpellPointRecoveryRate(int maxSpellPoints)
        {
            return Mathf.Max((int)Mathf.Floor(maxSpellPoints / 8), 1);
        }

        // Calculate chance of successfully lockpicking a door in an interior (an animating door). If this is higher than a random number between 0 and 100 (inclusive), the lockpicking succeeds.
        public static int CalculateInteriorLockpickingChance(int level, int lockvalue, int lockpickingSkill)
        {
            int chance = (5 * (level - lockvalue) + lockpickingSkill);
            return Mathf.Clamp(chance, 5, 95);
        }

        // Calculate chance of successfully lockpicking a door in an exterior (a door that leads to an interior). If this is higher than a random number between 0 and 100 (inclusive), the lockpicking succeeds.
        public static int CalculateExteriorLockpickingChance(int lockvalue, int lockpickingSkill)
        {
            int chance = lockpickingSkill - (5 * lockvalue);
            return Mathf.Clamp(chance, 5, 95);
        }

        // Calculate chance of successfully pickpocketing a target
        public static int CalculatePickpocketingChance(Entity.PlayerEntity player, Entity.EnemyEntity target)
        {
            int chance = player.Skills.Pickpocket;
            // If target is an enemy mobile, apply level modifier.
            if (target != null)
            {
                chance += 5 * ((player.Level) - (target.Level));
            }
            return Mathf.Clamp(chance, 5, 95);
        }

        // Calculate how many uses a skill needs before its value will rise.
        public static int CalculateSkillUsesForAdvancement(int skillValue, int skillAdvancementMultiplier, float careerAdvancementMultiplier, int level)
        {
            double levelMod = Math.Pow(1.04, level);
            return (int)Math.Floor((skillValue * skillAdvancementMultiplier * careerAdvancementMultiplier * levelMod * 2 / 5) + 1);
        }

        // Calculate player level.
        public static int CalculatePlayerLevel(int startingLevelUpSkillsSum, int currentLevelUpSkillsSum)
        {
            return (int)Mathf.Floor((currentLevelUpSkillsSum - startingLevelUpSkillsSum + 28) / 15);
        }

        // Calculate hit points player gains per level.
        public static int CalculateHitPointsPerLevelUp(Entity.PlayerEntity player)
        {
            int minRoll = player.Career.HitPointsPerLevel / 2;
            int maxRoll = player.Career.HitPointsPerLevel + 1; // Adding +1 as Unity Random.Range(int,int) is exclusive of maximum value
            int addHitPoints = UnityEngine.Random.Range(minRoll, maxRoll);
            addHitPoints += HitPointsModifier(player.Stats.Endurance);
            if (addHitPoints < 1)
                addHitPoints = 1;
            return addHitPoints;
        }

        #endregion

        #region Damage

        public static int CalculateHandToHandMinDamage(int handToHandSkill)
        {
            return (handToHandSkill / 10) + 1;
        }

        public static int CalculateHandToHandMaxDamage(int handToHandSkill)
        {
            // Daggerfall Chronicles table lists hand-to-hand skills of 80 and above (45 through 79 are omitted)
            // as if they give max damage of (handToHandSkill / 5) + 2, but the hand-to-hand damage display in the character sheet
            // in classic Daggerfall shows it as continuing to be (handToHandSkill / 5) + 1
            return (handToHandSkill / 5) + 1;
        }

        public static int CalculateWeaponDamage(Entity.DaggerfallEntity attacker, Entity.DaggerfallEntity target, FPSWeapon onscreenWeapon)
        {
            if (attacker == null || target == null)
                return 0;

            int damageLow = 0;
            int damageHigh = 0;
            int damageModifiers = 0;
            int baseDamage = 0;
            int damageResult = 0;
            int chanceToHitMod = 0;
            Items.DaggerfallUnityItem weapon = null;
            Entity.PlayerEntity player = GameManager.Instance.PlayerEntity;
            Entity.EnemyEntity AIAttacker = null;
            Entity.EnemyEntity AITarget = null;

            if (attacker != player)
            {
                AIAttacker = attacker as Entity.EnemyEntity;
                weapon = attacker.ItemEquipTable.GetItem(Items.EquipSlots.RightHand);
                if (weapon == null)
                    weapon = attacker.ItemEquipTable.GetItem(Items.EquipSlots.LeftHand);
            }
            else
            {
                if (GameManager.Instance.WeaponManager.UsingRightHand)
                    weapon = attacker.ItemEquipTable.GetItem(Items.EquipSlots.RightHand);
            }

            if (target != player)
            {
                AITarget = target as Entity.EnemyEntity;
            }

            if (weapon != null)
            {
                // If the attacker has a weapon equipped, check if the material is high enough to damage the target
                if (target.MinMetalToHit > (Items.WeaponMaterialTypes)weapon.NativeMaterialValue)
                {
                    if (attacker == player)
                    {
                        DaggerfallUI.Instance.PopupMessage(UserInterfaceWindows.HardStrings.materialIneffective);
                    }
                    return 0;
                }

                // If the attacker has a weapon equipped, get the weapon's damage
                damageLow = weapon.GetBaseDamageMin();
                damageHigh = weapon.GetBaseDamageMax();
                short skillID = weapon.GetWeaponSkillID();
                chanceToHitMod = attacker.Skills.GetSkillValue(skillID);
            }
            else if (attacker == player)
            {
                // If the player is attacking with no weapon equipped, use hand-to-hand skill for damage
                damageLow = CalculateHandToHandMinDamage(attacker.Skills.HandToHand);
                damageHigh = CalculateHandToHandMaxDamage(attacker.Skills.HandToHand);
                chanceToHitMod = attacker.Skills.HandToHand;
            }

            if (AIAttacker != null)
            {
                // Note: In classic, for enemies that have weapons, the damage values in the enemy
                // definitions are overridden by the weapon stats. This is fine for enemy classes,
                // who have non-weapon damage values of 0, but for the monsters that use weapons,
                // they may have a better attack if they don't use a weapon.
                // In DF Unity, enemies are using whichever is more damaging, the weapon or non-weapon attack.
                int weaponAverage = ((damageLow + damageHigh) / 2);
                int noWeaponAverage = ((AIAttacker.MobileEnemy.MinDamage + AIAttacker.MobileEnemy.MaxDamage) / 2);

                if (noWeaponAverage > weaponAverage)
                {
                    damageLow = AIAttacker.MobileEnemy.MinDamage;
                    damageHigh = AIAttacker.MobileEnemy.MaxDamage;
                    chanceToHitMod = attacker.Skills.HandToHand;
                }
            }

            baseDamage = UnityEngine.Random.Range(damageLow, damageHigh + 1);

            if (onscreenWeapon != null && (attacker == player))
            {
                // Apply swing modifiers for player.
                // The Daggerfall manual groups diagonal slashes to the left and right as if they are the same, but they are different.
                // Classic does not apply swing modifiers to hand-to-hand.
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeUp)
                {
                    damageModifiers += -4;
                    chanceToHitMod += 10;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDownRight)
                {
                    damageModifiers += -2;
                    chanceToHitMod += 5;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDownLeft)
                {
                    damageModifiers += 2;
                    chanceToHitMod += -5;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDown)
                {
                    damageModifiers += 4;
                    chanceToHitMod += -10;
                }
            }

            // Apply weapon proficiency modifiers for player.
            if ((attacker == player) && weapon != null && ((int)attacker.Career.ExpertProficiencies & weapon.GetWeaponSkillUsed()) != 0)
            {
                damageModifiers += ((attacker.Level / 3) + 1);
                chanceToHitMod += attacker.Level;
            }
            // Apply hand-to-hand proficiency modifiers for player. Hand-to-hand proficiencty is not applied in classic.
            else if ((attacker == player) && weapon == null && ((int)attacker.Career.ExpertProficiencies & (int)(DaggerfallConnect.DFCareer.ProficiencyFlags.HandToHand)) != 0)
            {
                damageModifiers += ((attacker.Level / 3) + 1);
                chanceToHitMod += attacker.Level;
            }

            // Apply modifiers for Skeletal Warrior.
            // In classic these appear to be applied after the swing and weapon proficiency modifiers but before all other
            // damage modifiers. Doing the same here.
            // DF Chronicles just says "Edged weapons inflict 1/2 damage"
            if (weapon != null && (target != player) && AITarget.CareerIndex == (int)Entity.MonsterCareers.SkeletalWarrior)
            {
                if (weapon.NativeMaterialValue == (int)Items.WeaponMaterialTypes.Silver)
                {
                    baseDamage *= 2;
                    damageModifiers *= 2;
                }
                if (weapon.GetWeaponSkillUsed() != (int)DaggerfallConnect.DFCareer.ProficiencyFlags.BluntWeapons)
                {
                    baseDamage /= 2;
                    damageModifiers /= 2;
                }
            }

            // Apply bonus or penalty by opponent type.
            // In classic this is broken and only works if the attack is done with a weapon that has the maximum number of enchantments.
            if ((target != player) && (AITarget.GetEnemyGroup() == DaggerfallConnect.DFCareer.EnemyGroups.Undead))
            {
                if (((int)attacker.Career.UndeadAttackModifier & (int)DaggerfallConnect.DFCareer.AttackModifier.Bonus) != 0)
                {
                    damageModifiers += attacker.Level;
                }
                if (((int)attacker.Career.UndeadAttackModifier & (int)DaggerfallConnect.DFCareer.AttackModifier.Phobia) != 0)
                {
                    damageModifiers -= attacker.Level;
                }
            }
            else if ((target != player) && (AITarget.GetEnemyGroup() == DaggerfallConnect.DFCareer.EnemyGroups.Daedra))
            {
                if (((int)attacker.Career.DaedraAttackModifier & (int)DaggerfallConnect.DFCareer.AttackModifier.Bonus) != 0)
                {
                    damageModifiers += attacker.Level;
                }
                if (((int)attacker.Career.DaedraAttackModifier & (int)DaggerfallConnect.DFCareer.AttackModifier.Phobia) != 0)
                {
                    damageModifiers -= attacker.Level;
                }
            }
            else if ((target != player) && (AITarget.GetEnemyGroup() == DaggerfallConnect.DFCareer.EnemyGroups.Humanoid))
            {
                if (((int)attacker.Career.HumanoidAttackModifier & (int)DaggerfallConnect.DFCareer.AttackModifier.Bonus) != 0)
                {
                    damageModifiers += attacker.Level;
                }
                if (((int)attacker.Career.HumanoidAttackModifier & (int)DaggerfallConnect.DFCareer.AttackModifier.Phobia) != 0)
                {
                    damageModifiers -= attacker.Level;
                }
            }
            else if ((target != player) && (AITarget.GetEnemyGroup() == DaggerfallConnect.DFCareer.EnemyGroups.Animals))
            {
                if (((int)attacker.Career.AnimalsAttackModifier & (int)DaggerfallConnect.DFCareer.AttackModifier.Bonus) != 0)
                {
                    damageModifiers += attacker.Level;
                }
                if (((int)attacker.Career.AnimalsAttackModifier & (int)DaggerfallConnect.DFCareer.AttackModifier.Phobia) != 0)
                {
                    damageModifiers -= attacker.Level;
                }
            }

            // Apply racial modifiers for player.
            if ((attacker == player) && weapon != null)
            {
                if (player.RaceTemplate.ID == (int)Entity.Races.DarkElf)
                {
                    damageModifiers += (attacker.Level / 4);
                    chanceToHitMod += (attacker.Level / 4);
                }
                else if (weapon.GetWeaponSkillUsed() == (int)DaggerfallConnect.DFCareer.ProficiencyFlags.MissileWeapons)
                {
                    if (player.RaceTemplate.ID == (int)Entity.Races.WoodElf)
                    {
                        damageModifiers += (attacker.Level / 3);
                        chanceToHitMod += (attacker.Level / 3);
                    }
                }
                else if (player.RaceTemplate.ID == (int)Entity.Races.Redguard)
                {
                    damageModifiers += (attacker.Level / 3);
                    chanceToHitMod += (attacker.Level / 3);
                }
            }

            // Apply strength modifier for player or for AI characters using weapons.
            // The in-game display of the strength modifier in Daggerfall is incorrect. It is actually ((STR - 50) / 5).
            if ((attacker == player) || (weapon != null))
            {
                damageModifiers += DamageModifier(attacker.Stats.Strength);
            }

            // Apply material modifier.
            // The in-game display in Daggerfall of weapon damages with material modifiers is incorrect. The material modifier is half of what the display suggests.
            if (weapon != null)
            {
                damageModifiers += weapon.GetWeaponMaterialModifier();
            }

            // Choose struck body part
            int[] bodyParts = { 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 6 };
            int struckBodyPart = bodyParts[UnityEngine.Random.Range(0, bodyParts.Length)];

            // Check for a successful hit.
            if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, weapon, struckBodyPart) == true)
            {
                // 0 damage is possible. Creates no blood splash.
                damageResult = Mathf.Max(0, (baseDamage + damageModifiers));
            }

            // If damage was done by a weapon, damage condition of weapon and armor of hit body part
            // In classic, shields are never damaged because the item in the equip slot of the hit
            // body part is all that is handled.
            // Here, if an equipped shield covered the hit body part, it takes damage instead.
            if (weapon != null && damageResult > 0)
            {
                weapon.DamageThroughPhysicalHit(damageResult, attacker);

                Items.DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(Items.EquipSlots.LeftHand);
                bool shieldTakesDamage = false;
                if (shield != null)
                {
                    Items.BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

                    for (int i = 0; (i < protectedBodyParts.Length) && !shieldTakesDamage; i++)
                    {
                        if (protectedBodyParts[i] == (Items.BodyParts)struckBodyPart)
                            shieldTakesDamage = true;
                    }
                }

                if (shieldTakesDamage)
                    shield.DamageThroughPhysicalHit(damageResult, target);
                else
                {
                    Items.EquipSlots hitSlot = Items.DaggerfallUnityItem.GetEquipSlotForBodyPart((Items.BodyParts)struckBodyPart);
                    Items.DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
                    if (armor != null)
                        armor.DamageThroughPhysicalHit(damageResult, target);
                }
            }

            // If attack was by player or weapon-based, end here
            if ((attacker == player) || (weapon != null))
            {
                return damageResult;
            }
            // Handle multiple attacks by AI characters.
            else
            {
                if (AIAttacker.MobileEnemy.MaxDamage2 != 0 && (CalculateSuccessfulHit(attacker, target, chanceToHitMod, weapon, struckBodyPart) == true))
                {
                    baseDamage = UnityEngine.Random.Range(AIAttacker.MobileEnemy.MinDamage2, AIAttacker.MobileEnemy.MaxDamage2 + 1);
                    damageResult += Mathf.Max(0, (baseDamage + damageModifiers));
                }
                if (AIAttacker.MobileEnemy.MaxDamage3 != 0 && (CalculateSuccessfulHit(attacker, target, chanceToHitMod, weapon, struckBodyPart) == true))
                {
                    baseDamage = UnityEngine.Random.Range(AIAttacker.MobileEnemy.MinDamage3, AIAttacker.MobileEnemy.MaxDamage3 + 1);
                    damageResult += Mathf.Max(0, (baseDamage + damageModifiers));
                }
                return damageResult;
            }
        }

        public static bool CalculateSuccessfulHit(Entity.DaggerfallEntity attacker, Entity.DaggerfallEntity target, int chanceToHitMod, Items.DaggerfallUnityItem weapon, int struckBodyPart)
        {
            if (attacker == null || target == null)
                return false;

            int chanceToHit = chanceToHitMod;
            Entity.PlayerEntity player = GameManager.Instance.PlayerEntity;
            Entity.EnemyEntity AITarget = target as Entity.EnemyEntity;

            int armorValue = 0;

            // Apply hit mod from character biography
            if (target == player)
            {
                chanceToHit -= player.BiographyAvoidHitMod;
            }

            // Get armor value for struck body part
            if (struckBodyPart <= target.ArmorValues.Length)
            {
                armorValue = target.ArmorValues[struckBodyPart];
            }

            chanceToHit += armorValue;

            // Apply adrenaline rush modifiers.
            if (attacker.Career.AdrenalineRush && attacker.CurrentHealth < (attacker.MaxHealth / 8))
            {
                chanceToHit += 5;
            }

            if (target.Career.AdrenalineRush && target.CurrentHealth < (target.MaxHealth / 8))
            {
                chanceToHit -= 5;
            }

            // Apply luck modifier.
            chanceToHit += ((attacker.Stats.Luck - target.Stats.Luck) / 10);

            // Apply agility modifier.
            chanceToHit += ((attacker.Stats.Agility - target.Stats.Agility) / 10);

            // Apply weapon material modifier.
            if (weapon != null)
            {
                chanceToHit += (weapon.GetWeaponMaterialModifier() * 10);
            }

            // Apply dodging modifier.
            // This modifier is bugged in classic and the attacker's dodging skill is used rather than the target's.
            // DF Chronicles says the dodging calculation is (dodging / 10), but it actually seems to be (dodging / 4).
            chanceToHit -= (target.Skills.Dodging / 4);

            // Apply critical strike modifier.
            if (UnityEngine.Random.Range(0, 100 + 1) < attacker.Skills.CriticalStrike)
            {
                chanceToHit += (attacker.Skills.CriticalStrike / 10);
            }

            // Apply monster modifier.
            if ((target != player) && (AITarget.EntityType == EntityTypes.EnemyMonster))
            {
                chanceToHit += 40;
            }

            // DF Chronicles says -60 is applied at the end, but it actually seems to be -50.
            chanceToHit -= 50;

            Mathf.Clamp(chanceToHit, 3, 97);

            int roll = UnityEngine.Random.Range(0, 100 + 1);

            if (roll <= chanceToHit)
                return true;
            else
                return false;
        }

        #endregion

        #region Enemies

        // Generates health for enemy classes based on level and class
        public static int RollEnemyClassMaxHealth(int level, int hitPointsPerLevel)
        {
            const int baseHealth = 10;
            int maxHealth = baseHealth;

            for (int i = 0; i < level; i++)
            {
                maxHealth += UnityEngine.Random.Range(1, hitPointsPerLevel + 1);
            }

            return maxHealth;
        }

        #endregion

        #region Fast Travel

        // Calculate fast travel cost. Based on observing classic Daggerfall behavior.
        public static int CalculateTripCost(float travelTimeTotalLand, float travelTimeTotalWater, bool speedCautious, bool sleepModeInn, bool travelFoot, int cautiousMod, float shipMod)
        {
            int tripCost = 0;

            // For cost calculations we need to know the time taken on land and water for both a cautious and reckless trip
            float travelTimeTotalLandCautious = travelTimeTotalLand;
            float travelTimeTotalLandReckless = travelTimeTotalLand;
            float travelTimeTotalWaterCautious = travelTimeTotalWater;
            float travelTimeTotalWaterReckless = travelTimeTotalWater;

            if (speedCautious)
            {
                travelTimeTotalLandReckless /= cautiousMod;
                travelTimeTotalWaterReckless /= cautiousMod;
            }
            else
            {
                travelTimeTotalLandCautious *= cautiousMod;
                travelTimeTotalWaterCautious *= cautiousMod;
            }

            // Get reckless travel times in days
            int travelTimeDaysLandReckless = 0;
            int travelTimeDaysWaterReckless = 0;
            int travelTimeDaysTotalReckless = 0;

            if (travelTimeTotalLandReckless > 0)
                travelTimeDaysLandReckless = (int)((travelTimeTotalLandReckless / 60 / 24) + 0.5);
            if (travelTimeTotalWaterReckless > 0)
                travelTimeDaysWaterReckless = (int)((travelTimeTotalWaterReckless / 60 / 24) + 0.5);
            travelTimeDaysTotalReckless = travelTimeDaysLandReckless + travelTimeDaysWaterReckless;

            // Get cautious travel times in days
            int travelTimeDaysLandCautious = 0;
            int travelTimeDaysWaterCautious = 0;
            int travelTimeDaysTotalCautious = 0;

            if (travelTimeTotalLandCautious > 0)
                travelTimeDaysLandCautious = (int)((travelTimeTotalLandCautious / 60 / 24) + 0.5);
            if (travelTimeTotalWaterCautious > 0)
                travelTimeDaysWaterCautious = (int)((travelTimeTotalWaterCautious / 60 / 24) + 0.5);
            travelTimeDaysTotalCautious = travelTimeDaysLandCautious + travelTimeDaysWaterCautious;

            // Calculate inn costs. Use cautious travel cost as a base.
            // Inns cost (5 * total land travel days) for land-only travel or travel that crosses water if a ship is used.
            if ((travelTimeTotalWater <= 0 || !travelFoot) && sleepModeInn && (travelTimeTotalLand > 0))
                tripCost += Mathf.Max(5, travelTimeDaysLandCautious * 5);

            // For travel that crosses water and is by foot/horse, the cost of
            // inns is (5 * (total travel days - days on water for reckless travel using a ship)).
            else if ((travelTimeTotalWater > 0) && sleepModeInn && travelFoot)
            {
                int travelTimeRecklessShipDays = (int)((travelTimeTotalWaterReckless * shipMod / 60 / 24) + 0.5);
                tripCost += Mathf.Max(5, (travelTimeDaysTotalCautious - travelTimeRecklessShipDays) * 5);
            }

            // Cost for inns in reckless travel is reduced from cautious travel cost by
            // (number of days less than cautious travel * 5)
            if (!speedCautious)
            {
                if (sleepModeInn)
                    tripCost -= Mathf.Min(tripCost, ((travelTimeDaysTotalCautious - travelTimeDaysTotalReckless) * 5));
            }

            // Calculate ship costs.
            // Ships cost (25 * (days on water for cautious travel))
            if (!travelFoot && (travelTimeTotalWater > 0))
                tripCost += Mathf.Max(25, travelTimeDaysWaterCautious * 25);

            return tripCost;
        }

        #endregion

        #region Holidays

        public static int GetHolidayId(uint gameMinutes, int regionIndex)
        {
            // Gives which regions celebrate which holidays.
            // Values are region IDs, index is holiday ID. 0xFF means all regions celebrate the holiday.
            byte[] regionIndexCelebratingHoliday = { 0xFF, 0x19, 0x01, 0xFF, 0x1D, 0x05, 0x19, 0x06, 0x3C, 0xFF, 0x29, 0x1A,
                0xFF, 0x02, 0x19, 0x01, 0x0E, 0x12, 0x14, 0xFF, 0xFF, 0x1C, 0x21, 0x1F, 0x2C, 0xFF, 0x12,
                0x23, 0xFF, 0x38, 0xFF, 0x01, 0x30, 0x29, 0x0B, 0x16, 0xFF, 0xFF, 0x11, 0x17, 0x14, 0x01,
                0xFF, 0x13, 0xFF, 0x33, 0x3C, 0x2E, 0xFF, 0xFF, 0x01, 0x2D, 0x18 };

            // Gives the day of the year that holidays are celebrated on.
            // Value are days of the year, index is holiday ID.
            short[] holidayDaysOfYear = { 0x01, 0x02, 0x0C, 0x0F, 0x10, 0x12, 0x20, 0x23, 0x26, 0x2E, 0x39, 0x3A,
                0x43, 0x45, 0x55, 0x56, 0x5B, 0x67, 0x6E, 0x76, 0x7F, 0x81, 0x8C, 0x96, 0x97, 0xA6, 0xAD,
                0xAE, 0xBE, 0xC0, 0xC8, 0xD1, 0xD4, 0xDD, 0xE0, 0xE7, 0xED, 0xF3, 0xF6, 0xFC, 0x103, 0x113,
                0x11B, 0x125, 0x12C, 0x12F, 0x134, 0x13E, 0x140, 0x159, 0x15C, 0x162, 0x163 };

            int holidayID = 0;
            uint dayOfYear = gameMinutes % 518400 / 1440 + 1;
            if (dayOfYear <= 355)
            {
                while (holidayID < 53)
                {
                    if ((regionIndexCelebratingHoliday[holidayID] == 0xFF || regionIndexCelebratingHoliday[holidayID] == regionIndex + 1)
                        && dayOfYear == holidayDaysOfYear[holidayID])
                    {
                        return holidayID + 1;
                    }
                    ++holidayID;
                }
            }

            // Not a holiday
            return 0;
        }

        #endregion
    }
}