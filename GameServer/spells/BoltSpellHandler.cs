/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.SkillHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell Handler for firing bolts
	/// </summary>
	[SpellHandlerAttribute("Bolt")]
	public class BoltSpellHandler : SpellHandler
	{
		/// <summary>
		/// Fire bolt
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			if ((target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent) && Spell.SpellType != "SiegeArrow")
			{
				MessageToCaster(String.Format("Your spell has no effect on the {0}!", target.Name), eChatType.CT_SpellResisted);
				return;
			}
			base.FinishSpellCast(target);
		}

		#region LOS Checks for Keeps
		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		public override bool StartSpell(GameLiving target)
		{
			foreach (GameLiving targ in SelectTargets(target))
			{
				if (targ is GamePlayer && Spell.Target.ToLower() == "cone" && CheckLOS(Caster))
				{
					GamePlayer player = targ as GamePlayer;
					player.Out.SendCheckLOS(Caster, player, new CheckLOSResponse(DealDamageCheckLOS));
				}
				else
				{
					DealDamage(targ);
				}
			}

			return true;
		}

		private bool CheckLOS(GameLiving living)
		{
			foreach (AbstractArea area in living.CurrentAreas)
			{
				if (area.CheckLOS)
					return true;
			}
			return false;
		}

		private void DealDamageCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100)
			{
				GameLiving target = (GameLiving)(Caster.CurrentRegion.GetObject(targetOID));
				if (target != null)
					DealDamage(target);
			}
		}

		private void DealDamage(GameLiving target)
		{
            int ticksToTarget = m_caster.GetDistanceTo( target ) * 100 / 85; // 85 units per 1/10s
			int delay = 1 + ticksToTarget / 100;
			foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(m_caster, target, m_spell.ClientEffect, (ushort)(delay), false, 1);
			}
			BoltOnTargetAction bolt = new BoltOnTargetAction(Caster, target, this);
			bolt.Start(1 + ticksToTarget);
		}
		#endregion

		/// <summary>
		/// Delayed action when bolt reach the target
		/// </summary>
		protected class BoltOnTargetAction : RegionAction
		{
			/// <summary>
			/// The bolt target
			/// </summary>
			protected readonly GameLiving m_boltTarget;

			/// <summary>
			/// The spell handler
			/// </summary>
			protected readonly BoltSpellHandler m_handler;

			/// <summary>
			/// Constructs a new BoltOnTargetAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="boltTarget">The bolt target</param>
			/// <param name="spellHandler"></param>
			public BoltOnTargetAction(GameLiving actionSource, GameLiving boltTarget, BoltSpellHandler spellHandler) : base(actionSource)
			{
				if (boltTarget == null)
					throw new ArgumentNullException("boltTarget");
				if (spellHandler == null)
					throw new ArgumentNullException("spellHandler");
				m_boltTarget = boltTarget;
				m_handler = spellHandler;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameLiving target = m_boltTarget;
				GameLiving caster = (GameLiving)m_actionSource;
				if (target == null) return;
				if (target.CurrentRegionID != caster.CurrentRegionID) return;
				if (target.ObjectState != GameObject.eObjectState.Active) return;
				if (!target.IsAlive) return;

				// Related to PvP hitchance
				// http://www.camelotherald.com/news/news_article.php?storyid=2444
				// No information on bolt hitchance against npc's
				// Bolts are treated as physical attacks for the purpose of ABS only
				// Based on this I am normalizing the miss rate for npc's to be that of a standard spell

				int missrate = 0;

				if (caster is GamePlayer && target is GamePlayer)
				{
					if (target.InCombat)
					{
						foreach (GameLiving attacker in target.Attackers)
						{
							if (attacker != caster && target.GetDistanceTo(attacker) <= 200)
							{
								// each attacker within 200 units adds a 20% chance to miss
								missrate += 20;
							}
						}
					}
				}

				if (target is GameNPC || caster is GameNPC)
				{
					missrate += (int)(ServerProperties.Properties.PVE_SPELL_CONHITPERCENT * caster.GetConLevel(target));
				}

				// add defence bonus from last executed style if any
				AttackData targetAD = (AttackData)target.TempProperties.getProperty<object>(GameLiving.LAST_ATTACK_DATA, null);
				if (targetAD != null
					&& targetAD.AttackResult == GameLiving.eAttackResult.HitStyle
					&& targetAD.Style != null)
				{
					missrate += targetAD.Style.BonusToDefense;
				}

				AttackData ad = m_handler.CalculateDamageToTarget(target, 0.5 - (caster.GetModified(eProperty.SpellDamage) * 0.01));

				if (Util.Chance(missrate)) 
				{
					ad.AttackResult = GameLiving.eAttackResult.Missed;
					m_handler.MessageToCaster("You miss!", eChatType.CT_YouHit);
					m_handler.MessageToLiving(target, caster.GetName(0, false) + " missed!", eChatType.CT_Missed);
					target.OnAttackedByEnemy(ad);
					target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, caster);
					if(target is GameNPC)
					{
						IOldAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IOldAggressiveBrain;
						if (aggroBrain != null)
							aggroBrain.AddToAggroList(caster, 1);
					}
					return;
				}

				ad.Damage = (int)((double)ad.Damage * (1.0 + caster.GetModified(eProperty.SpellDamage) * 0.01));

				// Block
				bool blocked = false;
				if (target is GamePlayer) 
				{ // mobs left out yet
					GamePlayer player = (GamePlayer)target;
					InventoryItem lefthand = player.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
					if (lefthand!=null && (player.AttackWeapon==null || player.AttackWeapon.Item_Type==Slot.RIGHTHAND || player.AttackWeapon.Item_Type==Slot.LEFTHAND)) 
					{
						if (target.IsObjectInFront(caster, 180) && lefthand.Object_Type == (int)eObjectType.Shield) 
						{
							double shield = 0.5 * player.GetModifiedSpecLevel(Specs.Shields);
							double blockchance = ((player.Dexterity*2)-100)/40.0 + shield + 5;
							// Removed 30% increased chance to block, can find no clear evidence this is correct - tolakram
							blockchance -= target.GetConLevel(caster) * 5;
							if (blockchance >= 100) blockchance = 99;
							if (blockchance <= 0) blockchance = 1;

							if (target.IsEngaging)
							{
								EngageEffect engage = target.EffectList.GetOfType<EngageEffect>();
								if (engage != null && target.AttackState && engage.EngageTarget == caster)
								{
									// Engage raised block change to 85% if attacker is engageTarget and player is in attackstate							
									// You cannot engage a mob that was attacked within the last X seconds...
									if (engage.EngageTarget.LastAttackedByEnemyTick > engage.EngageTarget.CurrentRegion.Time - EngageAbilityHandler.ENGAGE_ATTACK_DELAY_TICK)
									{
										if (engage.Owner is GamePlayer)
											(engage.Owner as GamePlayer).Out.SendMessage(engage.EngageTarget.GetName(0, true) + " has been attacked recently and you are unable to engage.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									}  // Check if player has enough endurance left to engage
									else if (engage.Owner.Endurance < EngageAbilityHandler.ENGAGE_DURATION_LOST)
									{
										engage.Cancel(false); // if player ran out of endurance cancel engage effect
									}
									else
									{
										engage.Owner.Endurance -= EngageAbilityHandler.ENGAGE_DURATION_LOST;
										if (engage.Owner is GamePlayer)
											(engage.Owner as GamePlayer).Out.SendMessage("You concentrate on blocking the blow!", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);

										if (blockchance < 85)
											blockchance = 85;
									}
								}
							}

							if (blockchance >= Util.Random(1, 100)) 
							{
								m_handler.MessageToLiving(player, "You partially block " + caster.GetName(0, false) + "'s spell!", eChatType.CT_Missed);
								m_handler.MessageToCaster(player.GetName(0, true) + " blocks!", eChatType.CT_YouHit);
								blocked = true;
							}
						}
					}
				}

				double effectiveness = 1.0 + (caster.GetModified(eProperty.SpellDamage) * 0.01);

				// simplified melee damage calculation
				if (blocked == false)
				{
					// TODO: armor resists to damage type

					double damage = m_handler.Spell.Damage / 2; // another half is physical damage
					if(target is GamePlayer)
						ad.ArmorHitLocation = ((GamePlayer)target).CalculateArmorHitLocation(ad);

					InventoryItem armor = null;
					if (target.Inventory != null)
						armor = target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

					double ws = (caster.Level * 8 * (1.0 + (caster.GetModified(eProperty.Dexterity) - 50)/200.0));

					damage *= ((ws + 90.68) / (target.GetArmorAF(ad.ArmorHitLocation) + 20*4.67));
					damage *= 1.0 - Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
					ad.Modifier = (int)(damage * (ad.Target.GetResist(ad.DamageType) + SkillBase.GetArmorResist(armor, ad.DamageType)) / -100.0);
					damage += ad.Modifier;

					damage = damage * effectiveness;
					damage *= (1.0 + RelicMgr.GetRelicBonusModifier(caster.Realm, eRelicType.Magic));

                    // WHRIA
                    if (caster is GamePlayer)
                    {
                        GamePlayer player = (GamePlayer)caster;

                        double advantage = 0;
                        if (player.CharacterClass.ID == 1) advantage = ServerProperties.Properties.CLASS1;
                        if (player.CharacterClass.ID == 2) advantage = ServerProperties.Properties.CLASS2;
                        if (player.CharacterClass.ID == 3) advantage = ServerProperties.Properties.CLASS3;
                        if (player.CharacterClass.ID == 4) advantage = ServerProperties.Properties.CLASS4;
                        if (player.CharacterClass.ID == 5) advantage = ServerProperties.Properties.CLASS5;
                        if (player.CharacterClass.ID == 6) advantage = ServerProperties.Properties.CLASS6;
                        if (player.CharacterClass.ID == 7) advantage = ServerProperties.Properties.CLASS7;
                        if (player.CharacterClass.ID == 8) advantage = ServerProperties.Properties.CLASS8;
                        if (player.CharacterClass.ID == 9) advantage = ServerProperties.Properties.CLASS9;
                        if (player.CharacterClass.ID == 10) advantage = ServerProperties.Properties.CLASS10;
                        if (player.CharacterClass.ID == 11) advantage = ServerProperties.Properties.CLASS11;
                        if (player.CharacterClass.ID == 12) advantage = ServerProperties.Properties.CLASS12;
                        if (player.CharacterClass.ID == 13) advantage = ServerProperties.Properties.CLASS13;
                        if (player.CharacterClass.ID == 14) advantage = ServerProperties.Properties.CLASS14;
                        if (player.CharacterClass.ID == 15) advantage = ServerProperties.Properties.CLASS15;
                        if (player.CharacterClass.ID == 16) advantage = ServerProperties.Properties.CLASS16;
                        if (player.CharacterClass.ID == 17) advantage = ServerProperties.Properties.CLASS17;
                        if (player.CharacterClass.ID == 18) advantage = ServerProperties.Properties.CLASS18;
                        if (player.CharacterClass.ID == 19) advantage = ServerProperties.Properties.CLASS19;
                        if (player.CharacterClass.ID == 20) advantage = ServerProperties.Properties.CLASS20;
                        if (player.CharacterClass.ID == 21) advantage = ServerProperties.Properties.CLASS21;
                        if (player.CharacterClass.ID == 22) advantage = ServerProperties.Properties.CLASS22;
                        if (player.CharacterClass.ID == 23) advantage = ServerProperties.Properties.CLASS23;
                        if (player.CharacterClass.ID == 24) advantage = ServerProperties.Properties.CLASS24;
                        if (player.CharacterClass.ID == 25) advantage = ServerProperties.Properties.CLASS25;
                        if (player.CharacterClass.ID == 26) advantage = ServerProperties.Properties.CLASS26;
                        if (player.CharacterClass.ID == 27) advantage = ServerProperties.Properties.CLASS27;
                        if (player.CharacterClass.ID == 28) advantage = ServerProperties.Properties.CLASS28;
                        if (player.CharacterClass.ID == 29) advantage = ServerProperties.Properties.CLASS29;
                        if (player.CharacterClass.ID == 30) advantage = ServerProperties.Properties.CLASS30;
                        if (player.CharacterClass.ID == 31) advantage = ServerProperties.Properties.CLASS31;
                        if (player.CharacterClass.ID == 32) advantage = ServerProperties.Properties.CLASS32;
                        if (player.CharacterClass.ID == 33) advantage = ServerProperties.Properties.CLASS33;
                        if (player.CharacterClass.ID == 34) advantage = ServerProperties.Properties.CLASS34;
                        if (player.CharacterClass.ID == 35) advantage = ServerProperties.Properties.CLASS35;
                        if (player.CharacterClass.ID == 36) advantage = ServerProperties.Properties.CLASS36;
                        if (player.CharacterClass.ID == 37) advantage = ServerProperties.Properties.CLASS37;
                        if (player.CharacterClass.ID == 38) advantage = ServerProperties.Properties.CLASS38;
                        if (player.CharacterClass.ID == 39) advantage = ServerProperties.Properties.CLASS39;
                        if (player.CharacterClass.ID == 40) advantage = ServerProperties.Properties.CLASS40;
                        if (player.CharacterClass.ID == 41) advantage = ServerProperties.Properties.CLASS41;
                        if (player.CharacterClass.ID == 42) advantage = ServerProperties.Properties.CLASS42;
                        if (player.CharacterClass.ID == 43) advantage = ServerProperties.Properties.CLASS43;
                        if (player.CharacterClass.ID == 44) advantage = ServerProperties.Properties.CLASS44;
                        if (player.CharacterClass.ID == 45) advantage = ServerProperties.Properties.CLASS45;
                        if (player.CharacterClass.ID == 46) advantage = ServerProperties.Properties.CLASS46;
                        if (player.CharacterClass.ID == 47) advantage = ServerProperties.Properties.CLASS47;
                        if (player.CharacterClass.ID == 48) advantage = ServerProperties.Properties.CLASS48;
                        if (player.CharacterClass.ID == 49) advantage = ServerProperties.Properties.CLASS49;
                        if (player.CharacterClass.ID == 50) advantage = ServerProperties.Properties.CLASS50;
                        if (player.CharacterClass.ID == 51) advantage = ServerProperties.Properties.CLASS51;
                        if (player.CharacterClass.ID == 52) advantage = ServerProperties.Properties.CLASS52;
                        if (player.CharacterClass.ID == 53) advantage = ServerProperties.Properties.CLASS53;
                        if (player.CharacterClass.ID == 54) advantage = ServerProperties.Properties.CLASS54;
                        if (player.CharacterClass.ID == 55) advantage = ServerProperties.Properties.CLASS55;
                        if (player.CharacterClass.ID == 56) advantage = ServerProperties.Properties.CLASS56;
                        if (player.CharacterClass.ID == 57) advantage = ServerProperties.Properties.CLASS57;
                        if (player.CharacterClass.ID == 58) advantage = ServerProperties.Properties.CLASS58;
                        if (player.CharacterClass.ID == 59) advantage = ServerProperties.Properties.CLASS59;
                        if (player.CharacterClass.ID == 60) advantage = ServerProperties.Properties.CLASS60;
                        damage *= (1 + advantage);
                    }
                    // END     

					if (damage < 0) damage = 0;
					ad.Damage += (int)damage;
				}

				if (m_handler is SiegeArrow == false)
				{
					ad.UncappedDamage = ad.Damage;
					ad.Damage = (int)Math.Min(ad.Damage, m_handler.DamageCap(effectiveness));
				}

				ad.Damage = (int)(ad.Damage * caster.Effectiveness);

				if (blocked == false && ad.CriticalDamage > 0)
				{
					int critMax = (target is GamePlayer) ? ad.Damage/2 : ad.Damage;
					ad.CriticalDamage = Util.Random(critMax / 10, critMax);
				}

				m_handler.SendDamageMessages(ad);
				m_handler.DamageTarget(ad, false, (blocked ? 0x02 : 0x14));
				target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, caster);
			}
		}

		// constructor
		public BoltSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
