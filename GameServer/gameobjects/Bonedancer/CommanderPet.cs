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
using DOL.Language;
using System.Collections;
using System.Collections.Generic;

namespace DOL.GS
{
	public class CommanderPet : BDPet
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
// WHRIA
        public bool bAssist = false;

        private bool bSaveTemplateSpell = false;
        private List<Spell> m_spells_saved = new List<Spell>();

        private enum BDSpells
        {
            Empower_DreadGuardian = 601312,
            Empower_DreadArcher = 601291,
            Empower_DreadLich = 601282,
            Drain = 60134,
            Suppress = 60135,
            Snare=60136,
            Debilitating=60137,
            Damage = 60138,
            Proc_DreadArcher = 10305,
            Proc_DreadGuardian= 10304
        };


		/// <summary>
		/// Create a commander.
		/// </summary>
		/// <param name="npcTemplate"></param>
		/// <param name="owner"></param>
		public CommanderPet(INpcTemplate npcTemplate)
			: base(npcTemplate)
		{
			if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.ReturnedCommander"))
			{
				InitControlledBrainArray(0);
			}

			if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DecayedCommander") ||
			    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.SkeletalCommander"))
			{
				InitControlledBrainArray(1);
			}

			if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.BoneCommander"))
			{
				InitControlledBrainArray(2);
			}

			if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadCommander") ||
			    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian") ||
			    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich") ||
			    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadArcher"))
			{
				InitControlledBrainArray(3);
			}
		}

        void SetSpell(int iSpellID, string spellname_)
        {
            if (bSaveTemplateSpell == false)
            {
                bSaveTemplateSpell=true;
                foreach (Spell spell in Spells)
                {
                    m_spells_saved.Add(spell);
                }
            }

            GamePlayer player = (Brain as IControlledBrain).Owner as GamePlayer;
            bool found = false;

            foreach (Spell spell in Spells)
            {
                if (spell.ID == iSpellID)
                {
                    player.Out.SendMessage("Your pet stop using " + spellname_, eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    found = true;
                }
//                Spells.Remove(spell);
            }
            Spells.Clear();


//            Spells
            foreach (Spell spell in m_spells_saved)
            {
                Spells.Add(spell);
            }

            if (found==false)
            {
                Spell spell_ = SkillBase.GetSpellByID(iSpellID);
                if (spell_ != null)
                    Spells.Add(spell_);
                else
                    player.Out.SendMessage("[ERROR] Couldn't find BD pet's " + spellname_ + " [" + iSpellID + "] spell", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
            }

            foreach (Spell spell in Spells)
            {
                player.Out.SendMessage("Your pet using " + spell.Name + " [" + spell.ID + "]", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
            }
        }

		/// <summary>
		/// Called when owner sends a whisper to the pet
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></
        /// param>
		public override bool WhisperReceive(GameLiving source, string str)
		{
			GamePlayer player = source as GamePlayer;
			if (player == null || player != (Brain as IControlledBrain).Owner)
				return false;

			string[] strargs = str.ToLower().Split(' ');

			for (int i = 0; i < strargs.Length; i++)
			{
				String curStr = strargs[i];

				if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Commander"))
				{
					if (Name == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian"))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.DreadGuardian", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}

					if (Name == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich"))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.DreadLich", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}

					if (Name == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadArcher"))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.DreadArcher", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}

					if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadCommander") ||
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DecayedCommander") ||
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.ReturnedCommander") ||
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.SkeletalCommander") ||
					    Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.BoneCommander"))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.XCommander", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}

                }

#region XCOMMAND


                if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Combat"))
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Combat", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Assist"))
				{
					//TODO: implement this - I have no idea how to do that...
//					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Assist.Text"), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
// WHRIA
                    bAssist = !bAssist;
                    if (bAssist)
                        player.Out.SendMessage("I will assist you", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    else
                        player.Out.SendMessage("I do not assist you", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
// END
				}

				if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Taunt"))
				{
                    SetSpell(60127, "Taunt");
					break;
				}


                if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Empower"))
                {
                    if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian"))
                    {
                        Spell spell_ = SkillBase.GetSpellByID((int)BDSpells.Empower_DreadGuardian);
                        CastSpell(spell_, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells)); break;
                    }
                    if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich"))
                    {
                        Spell spell_ = SkillBase.GetSpellByID((int)BDSpells.Empower_DreadLich);
                        CastSpell(spell_, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells)); break;
                    }
                    if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadArcher"))
                    {
                        Spell spell_ = SkillBase.GetSpellByID((int)BDSpells.Empower_DreadArcher);
                        CastSpell(spell_, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells)); break;
                    }
                }

#endregion
		
#region COMBAT

// COMBAT

				if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Weapons"))
				{
					if (Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadCommander") &&
					    Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DecayedCommander") &&
					    Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.ReturnedCommander") &&
					    Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.SkeletalCommander") &&
					    Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.BoneCommander"))
					{
						break;
					}
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.DiffCommander", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}

                if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.One"))
                {
                    i++;
                    if (i + 1 >= strargs.Length)
                        return false;
                    CommanderSwitchWeapon(eInventorySlot.RightHandWeapon, eActiveWeaponSlot.Standard, strargs[++i]);
                }

                if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Two"))
                {
                    i++;
                    if (i + 1 >= strargs.Length)
                        return false;
                    CommanderSwitchWeapon(eInventorySlot.TwoHandWeapon, eActiveWeaponSlot.TwoHanded, strargs[++i]);
                }

#endregion

#region GUARDIAN

                // GUARDIAN
                if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Harm"))
                {
                    if (Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian"))
                        return false;

                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.DreadGuardian2", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
                if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Drain"))
                {
                    if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian"))
                    {
                        SetSpell((int)BDSpells.Drain, "Drain"); break;
                    }
                }

                if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Suppress"))
                {
                    if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian"))
                    {
                        SetSpell((int)BDSpells.Suppress, "Suppress"); break;
                    }
                }
                // GUARDIAN
#endregion

#region LICH
                // LICH
                if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Spells"))
                {
                    if (Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich"))
                        return false;

                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.DreadLich2", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }

                if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Snares"))
                {
                    if (Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich"))
                        return false;
                    SetSpell((int)BDSpells.Snare, "Snare"); break;
                }

                if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Debilitating"))
                {
                    if (Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich"))
                        return false;

                    SetSpell((int)BDSpells.Debilitating, "Debilitating"); break;
                }

                if (curStr == LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObjects.CommanderPet.WR.Const.Damage"))
                {
                    if (Name.ToLower() != LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadLich"))
                        return false;

                    SetSpell((int)BDSpells.Damage, "Damage"); break;
                }


                // LICH
#endregion

            }
			return base.WhisperReceive(source, str);
		}
		public override bool AddToWorld()
        {
            AddStatsToWeapon();
            return base.AddToWorld();
        }

        protected virtual void AddStatsToWeapon()
        {
            base.AddStatsToWeapon();
            if (Inventory != null)
            {
                InventoryItem item=Inventory.GetItem(eInventorySlot.TwoHandWeapon);
                if (item == null)
                    item = Inventory.GetItem(eInventorySlot.RightHandWeapon);

                if (item != null)
                {
                    if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadArcher"))
                    {
                        if (item.ProcSpellID==0)
                            item.ProcSpellID = (int)BDSpells.Proc_DreadArcher;
//                        item.ProcChance = 10;
                    }
                    else if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadGuardian"))
                    {
                        if (item.ProcSpellID == 0)
                            item.ProcSpellID = (int)BDSpells.Proc_DreadGuardian;
//                        item.ProcChance = 10;
                    }
                    else if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.BoneCommander"))
                    {

                    }
                    else if (Name.ToLower() == LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DreadCommander"))
                    {
                    }
                    else if (Name.ToLower()==LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.DecayedCommander"))
                    {
                    }
                    else if (Name.ToLower()==LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.ReturnedCommander"))
                    {

                    }
                    else if (Name.ToLower()==LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameObjects.CommanderPet.SkeletalCommander"))
                    {

                    }
                }
            }
        }
		/// <summary>
		/// Changes the commander's weapon to the specified type
        /// by WHRIA
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="aSlot"></param>
		/// <param name="weaponType"></param>
		protected void CommanderSwitchWeapon(eInventorySlot slot, eActiveWeaponSlot aSlot, string weaponType)
		{
            string templateID = string.Format("BD_Commander_{0}_{1}", slot.ToString(), weaponType);

            GameNpcInventoryTemplate inventoryTemplate = new GameNpcInventoryTemplate();
            if (inventoryTemplate.LoadFromDatabase(templateID))
            {
                Inventory = new GameNPCInventory(inventoryTemplate);
                if (Inventory.AllItems.Count == 0)
                {
                    if (log.IsErrorEnabled)
                        log.Error(string.Format("Unable to find Bonedancer item: {0}", templateID));
                    return;
                }
            }
            else
            {
                if (log.IsErrorEnabled)
                    log.Error(string.Format("Unable to find Bonedancer item: {0}", templateID));
                return;
            }

            SwitchWeapon(aSlot);
            AddStatsToWeapon();

            InventoryItem item_left;
            InventoryItem item_right;
            InventoryItem item_two;
            if (Inventory != null)
            {
                item_left = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                item_right = Inventory.GetItem(eInventorySlot.RightHandWeapon);
                item_two = Inventory.GetItem(eInventorySlot.TwoHandWeapon);

                switch (weaponType)
                {
                    case "one_handed_sword":
                        if (item_right != null)
                            item_right.Type_Damage = (int)eDamageType.Slash;
                        break;
                    case "two_handed_sword":
                        if (item_two != null)
                            item_right.Type_Damage = (int)eDamageType.Slash;
                        break;
                    case "one_handed_hammer":
                        if (item_right != null)
                            item_right.Type_Damage = (int)eDamageType.Crush;
                        break;
                    case "two_handed_hammer":
                        if (item_two != null)
                            item_right.Type_Damage = (int)eDamageType.Crush;
                        break;
                    case "one_handed_axe":
                        if (item_right != null)
                            item_right.Type_Damage = (int)eDamageType.Thrust;
                        break;
                    case "two_handed_axe":
                        if (item_two != null)
                            item_right.Type_Damage = (int)eDamageType.Thrust;
                        break;
                }
            }
            
            BroadcastLivingEquipmentUpdate();
		}

		/// <summary>
		/// Adds a pet to the current array of pets
		/// </summary>
		/// <param name="controlledNpc">The brain to add to the list</param>
		/// <returns>Whether the pet was added or not</returns>
		public override bool AddControlledNpc(IControlledBrain controlledNpc)
		{
			IControlledBrain[] brainlist = ControlledNpcList;
			if (brainlist == null) return false;
			foreach (IControlledBrain icb in brainlist)
			{
				if (icb == controlledNpc)
					return false;
			}

			if (controlledNpc.Owner != this)
				throw new ArgumentException("ControlledNpc with wrong owner is set (player=" + Name + ", owner=" + controlledNpc.Owner.Name + ")", "controlledNpc");

			//Find the next spot for this new pet
			int i = 0;
			for (; i < brainlist.Length; i++)
			{
				if (brainlist[i] == null)
					break;
			}
			//If we didn't find a spot return false
			if (i >= m_controlledBrain.Length)
				return false;
			m_controlledBrain[i] = controlledNpc;
			PetCount++;
			return base.AddControlledNpc(controlledNpc);
		}

		/// <summary>
		/// Removes the brain from
		/// </summary>
		/// <param name="controlledNpc">The brain to find and remove</param>
		/// <returns>Whether the pet was removed</returns>
		public override bool RemoveControlledNpc(IControlledBrain controlledNpc)
		{
			bool found = false;
			lock (ControlledNpcList)
			{
				if (controlledNpc == null) return false;
				IControlledBrain[] brainlist = ControlledNpcList;
				int i = 0;
				//Try to find the minion in the list
				for (; i < brainlist.Length; i++)
				{
					//Found it
					if (brainlist[i] == controlledNpc)
					{
						found = true;
						break;
					}
				}

				//Found it, lets remove it
				if (found)
				{
					//First lets store the brain to kill it
					IControlledBrain tempBrain = m_controlledBrain[i];
					//Lets get rid of the brain asap
					m_controlledBrain[i] = null;

					//Only decrement, we just lost one pet
					PetCount--;

					return base.RemoveControlledNpc(controlledNpc);
				}
			}

			return found;
		}
	}
}