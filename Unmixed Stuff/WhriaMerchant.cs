


using System;
using System.Reflection;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.Events;
using log4net;
using DOL.AI.Brain;



namespace DOL.GS.Scripts
{
    public class WhriaMerchant : GameWhriaMerchant
    {
        #region Constructor

        public WhriaMerchant()
            : base()
        {
            SetOwnBrain(new BlankBrain());
        }

        #endregion Constructor

        #region AddToWorld

        public override bool AddToWorld()
        {

           
            Level = 60;
            Name = "Dragon Merchant";
            GuildName = "";
            Model = 1903;
         
            MaxSpeedBase = 0;
            Realm = 0;
   
            TradeItems = new MerchantTradeItems("dragon_merchant");

            return base.AddToWorld();
        }

        #endregion AddToWorld
        public override bool Interact(GamePlayer player)
        {
            TradeItems = new MerchantTradeItems("dragon_merchant");
            player.Out.SendMerchantWindow(TradeItems, eMerchantWindowType.Normal);
            return true;
        }
    }  
 }

