/*
WHRIA
 * 
 * go and playsound
 */

using System;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;
using DOL.Language;
using System.Collections.Generic;


namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&ps", //command to handle
        ePrivLevel.Player, //minimum privelege level
        "ps [ID]", //command description
        "'/ps ID <SoundID> usable code for play sound are: player.Out.SendSoundEffect(ID, 0, 0, 0, 0, 0); ")] //usage
    public class PlaySoundIDCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length == 1)
            {
                DisplaySyntax(client);
                return;
            }

            GamePlayer player = client.Player.TargetObject as GamePlayer;

            if (player == null)
                player = client.Player;

            ushort ID;

            if (ushort.TryParse(args[1], out ID) == false)
            {
                DisplaySyntax(client);
                return;
            }

//            player.Out.SendSoundEffect(ID, 0, 0, 0, 0, 0);
            player.Out.SendRegionEnterSound((byte)ID);
        }
    }
}

namespace DOL.GS.Commands
{
	[CmdAttribute("&go",
        ePrivLevel.Player,
        "Commands.Jump.Information",
        "/go <playername> ex. /go whria"
		)]
	public class GoCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			try
			{
				#region Jump to PlayerName
				if (args.Length == 2)
				{
					GameClient clientc = null;
					clientc = WorldMgr.GetClientByPlayerName(args[1], false, true);
/*
                    if (client.Player.Level>=50)
                    {
                        client.Out.SendMessage("Only newbies (level<50) can use this command !!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
 * 
 * 
*/
                    bool bTest = false;
                    if (client.Player.Name.Length >= 4)
                    {
                        if (client.Player.Name.Substring(0, 4).ToLower() == "test")
                        {
                            bTest = true;
                        }
                    }
                    if (bTest)
                    {
                        client.Out.SendMessage("Test Character cannot use /go command !!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }



					if (clientc == null || clientc.Player.Realm != client.Player.Realm)
					{
						client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Jump.CannotBeFound", args[1]), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}

                    if (clientc.Player.CurrentRegionID == 163 || clientc.Player.CurrentRegionID == 249 || (clientc.Player.CurrentRegionID >=234 &&  clientc.Player.CurrentRegionID <=242))
                    {
                        client.Out.SendMessage("You cannot jump to RvR area by this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (client.Player.CurrentRegionID == 163 || client.Player.CurrentRegionID == 249 || (client.Player.CurrentRegionID >= 234 && client.Player.CurrentRegionID <= 242))
                    {
                        client.Out.SendMessage("You cannot use this command in RvR area.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }

					if (CheckExpansion(client, clientc, clientc.Player.CurrentRegionID))
					{
						client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Jump.JumpToX", clientc.Player.CurrentRegion.Description), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						if (clientc.Player.CurrentHouse != null && clientc.Player.InHouse)
							clientc.Player.CurrentHouse.Enter(client.Player);
						else
							client.Player.MoveTo(clientc.Player.CurrentRegionID, clientc.Player.X, clientc.Player.Y, clientc.Player.Z, client.Player.Heading);
						return;
					}

					client.Out.SendMessage("You don't have an expansion needed to jump to this location.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

					return;
				}
				#endregion Jump to PlayerName
				#region DisplaySyntax
				else
				{
					DisplaySyntax(client);
					return;
				}
				#endregion DisplaySyntax
			}

			catch (Exception ex)
			{
				DisplayMessage(client, ex.Message);
			}
		}

		public bool CheckExpansion(GameClient clientJumper, GameClient clientJumpee, ushort RegionID)
		{
			Region reg = WorldMgr.GetRegion(RegionID);
			if (reg != null && reg.Expansion > (int)clientJumpee.ClientType)
			{
				clientJumper.Out.SendMessage(LanguageMgr.GetTranslation(clientJumper, "GMCommands.Jump.CheckExpansion.CannotJump", clientJumpee.Player.Name, reg.Description), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (clientJumper != clientJumpee)
					clientJumpee.Out.SendMessage(LanguageMgr.GetTranslation(clientJumpee, "GMCommands.Jump.CheckExpansion.ClientNoSup", clientJumper.Player.Name, reg.Description), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			return true;
		}
	}
}