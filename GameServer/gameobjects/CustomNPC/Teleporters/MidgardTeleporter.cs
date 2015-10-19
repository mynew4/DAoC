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
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.Database;
using System.Collections;
using DOL.GS.Spells;
using log4net;
using System.Reflection;
using DOL.GS.Quests.Catacombs.Obelisks;

namespace DOL.GS
{
	/// <summary>
	/// Midgard teleporter.
	/// </summary>
	/// <author>Aredhel</author>
	public class MidgardTeleporter : GameTeleporter
	{
		/// <summary>
		/// Player right-clicked the teleporter.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;
			
			String intro = String.Format("Greetings. I can channel the energies of this place to send you {0} {1} {2} {3} {4}",
			                             "to far away lands. If you wish to fight in the Frontiers I can send you to [Uppland] or to the",
			                             "border keeps [Svasud Faste] and [Vindsaul Faste]. Maybe you wish to visit the [City of Aegirhamn]?",
			                             "You could explore the [Gotar] or perhaps you would prefer the comforts of the [Housing] regions.",
			                             "Perhaps the fierce [Battlegrounds] are more to your liking or do you wish to meet the citizens inside",
			                             "the great city of [Jordheim]?");
			SayTo(player, intro);
			return true;
		}

		/// <summary>
		/// Player has picked a destination.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected override void OnDestinationPicked(GamePlayer player, Teleport destination)
		{
			switch (destination.TeleportID.ToLower())
			{
				case "battlegrounds":
					if (!ServerProperties.Properties.BG_ZONES_OPENED && player.Client.Account.PrivLevel == (uint)ePrivLevel.Player)
					{
						SayTo(player, ServerProperties.Properties.BG_ZONES_CLOSED_MESSAGE);
						return;
					}
					SayTo(player, "I will teleport you to the appropriate battleground for your level and Realm Rank. If you exceed the Realm Rank for a battleground, you will not teleport. Please gain more experience to go to the next battleground.");
					break;
				case "city of aegirhamn":
					SayTo(player, "The Shrouded Isles await you.");
					break;
                case "housing":
                    break;
        		case "gotar":
					SayTo(player, "You shall soon arrive in the Gotar.");
					break;
				case "jordheim":
					SayTo(player, "The great city awaits!");
					break;
				case "svasud faste":
					SayTo(player, "Svasud Faste is what you seek, and Svasud Faste is what you shall find.");
					break;
				case "uppland":
					SayTo(player, "Now to the Frontiers for the glory of the realm!");
					break;
				case "vindsaul faste":
					SayTo(player, "Vindsaul Faste is what you seek, and Vindsaul Faste is what you shall find.");
					break;
				default:
					SayTo(player, "This destination is not yet supported.");
					return;
			}
			base.OnDestinationPicked(player, destination);
		}

		/// <summary>
		/// Teleport the player to the designated coordinates.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="destination"></param>
		protected override void OnTeleport(GamePlayer player, Teleport destination)
		{
			OnTeleportSpell(player, destination);
		}
	}
}
