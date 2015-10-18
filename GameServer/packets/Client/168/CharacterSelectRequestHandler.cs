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

using DOL.Events;
// WHRIA START
using System;
using System.Text.RegularExpressions;
// END
 
namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.CharacterSelectRequest, "Handles setting SessionID and the active character", eClientStatus.LoggedIn)]
	public class CharacterSelectRequestHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			int packetVersion;
			switch (client.Version)
			{
				case GameClient.eClientVersion.Version168:
				case GameClient.eClientVersion.Version169:
				case GameClient.eClientVersion.Version170:
				case GameClient.eClientVersion.Version171:
				case GameClient.eClientVersion.Version172:
				case GameClient.eClientVersion.Version173:
					packetVersion = 168;
					break;
				default:
					packetVersion = 174;
					break;
			}

			packet.Skip(4); //Skip the first 4 bytes
			if (packetVersion == 174)
			{
				packet.Skip(1);
			}

			string charName = packet.ReadString(28);

            // WHRIA START
            if (charName.Length >= 5)
            {
                if (charName.Substring(0, 4) == "slot")
                {
                    int iSelected = 0;
                    if (charName.Substring(4, 1) == "a") iSelected = 0;
                    if (charName.Substring(4, 1) == "b") iSelected = 1;
                    if (charName.Substring(4, 1) == "c") iSelected = 2;
                    if (charName.Substring(4, 1) == "d") iSelected = 3;
                    if (charName.Substring(4, 1) == "e") iSelected = 4;
                    if (charName.Substring(4, 1) == "f") iSelected = 5;
                    if (charName.Substring(4, 1) == "g") iSelected = 6;
                    if (charName.Substring(4, 1) == "h") iSelected = 7;
                    if (charName.Substring(4, 1) == "i") iSelected = 8;
                    if (charName.Substring(4, 1) == "j") iSelected = 9;

                    int sizeOfCharacter = client.Account.Characters.Length;

                    for (int j = 0; j < sizeOfCharacter; j++)
                    {
                        int iSlot = client.Account.Characters[j].AccountSlot;
                        if (iSlot >= 300) iSlot -= 300; if (iSlot >= 200) iSlot -= 200; if (iSlot >= 100) iSlot -= 100;

                        if (iSlot == iSelected)
                        {
                            Regex nameCheck = new Regex("^[A-Z][a-zA-Z]");
                            if (!nameCheck.IsMatch(client.Account.Characters[j].Name)
                                && client.Account.Characters[j] != null)
                            {
                                charName = client.Account.Characters[j].Name;
                            }
                        }
                    }
                }
            }
            //  END


			//TODO Character handling 
			if (charName.Equals("noname"))
			{
				client.Out.SendSessionID();
			}
			else
			{
				// SH: Also load the player if client player is NOT null but their charnames differ!!!
				// only load player when on charscreen and player is not loaded yet
				// packet is sent on every region change (and twice after "play" was pressed)
				if (
					(
						(client.Player == null && client.Account.Characters != null)
						|| (client.Player != null && client.Player.Name.ToLower() != charName.ToLower())
					) && client.ClientState == GameClient.eClientState.CharScreen)
				{
					bool charFound = false;
					for (int i = 0; i < client.Account.Characters.Length; i++)
					{
						if (client.Account.Characters[i] != null
						    && client.Account.Characters[i].Name == charName)
						{
							charFound = true;
							// Notify Character Selection Event, last hope to fix any bad data before Loading.
							GameEventMgr.Notify(DatabaseEvent.CharacterSelected, new CharacterEventArgs(client.Account.Characters[i], client));
							client.LoadPlayer(i);
							break;
						}
					}
					if (charFound == false)
					{
						client.Player = null;
						client.ActiveCharIndex = -1;
					}
					else
					{
						// Log character play
						AuditMgr.AddAuditEntry(client, AuditType.Character, AuditSubtype.CharacterLogin, "", charName);
					}
				}

				client.Out.SendSessionID();
			}
		}

		#endregion
	}
}