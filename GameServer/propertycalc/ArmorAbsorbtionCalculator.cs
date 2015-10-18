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
//WHRIA
using DOL.GS.PacketHandler;
//END

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The Armor Absorption calculator
	/// 
	/// BuffBonusCategory1 is used for buffs, uncapped
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 is used for debuffs
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.ArmorAbsorption)]
	public class ArmorAbsorptionCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			int abs = Math.Min(living.BaseBuffBonusCategory[(int)property]
				- living.DebuffCategory[(int)property]
				+ living.ItemBonus[(int)property]
				+ living.AbilityBonus[(int)property], 50);


            bool bTest = false;
            if (living is GamePlayer)
            {
                GamePlayer player = (GamePlayer)living;
                if (player.TempProperties.getProperty<string>(GamePlayer.AFK_MESSAGE) == "test") bTest = true;
            }

            if (bTest)
            {
                GamePlayer player = (GamePlayer)living;
                player.Out.SendMessage("/// ArmorAbsorptionCalculator ///", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Out.SendMessage("living.BaseBuffBonusCategory[(int)property] : " + living.BaseBuffBonusCategory[(int)property], eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Out.SendMessage("living.DebuffCategory[(int)property] : " + living.DebuffCategory[(int)property], eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Out.SendMessage("living.ItemBonus[(int)property] : " + living.ItemBonus[(int)property], eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Out.SendMessage("living.AbilityBonus[(int)property] : " + living.AbilityBonus[(int)property], eChatType.CT_System, eChatLoc.CL_SystemWindow);

                player.Out.SendMessage("int abs = Math.Min(living.BaseBuffBonusCategory[(int)property] - living.DebuffCategory[(int)property] + living.ItemBonus[(int)property] + living.AbilityBonus[(int)property], 50); : " + abs, eChatType.CT_System, eChatLoc.CL_SystemWindow);

                
            }


			if (living is GameNPC)
			{
				if (living.Level >= 30) abs += 27;
				else if (living.Level >= 20) abs += 19;
				else if (living.Level >= 10) abs += 10;

				abs += (living.GetModified(eProperty.Constitution)
					+ living.GetModified(eProperty.Dexterity) - 120) / 12;
			}

			return abs;
		}
	}
}
