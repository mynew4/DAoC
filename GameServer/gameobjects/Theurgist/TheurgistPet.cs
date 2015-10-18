using System;
using System.Collections.Generic;
using System.Text;
using DOL.Database;

namespace DOL.GS
{
	public class TheurgistPet : GamePet
	{
		public TheurgistPet(INpcTemplate npcTemplate) : base(npcTemplate) { }

		public override void OnAttackedByEnemy(AttackData ad) { /* do nothing */ }
		
		/// <summary>
		/// not each summoned pet 'll fire ambiant sentences
		/// let's say 10%
		/// </summary>
		protected override void BuildAmbientTexts()
		{
			base.BuildAmbientTexts();
			if (ambientTexts.Count>0)
				foreach (var at in ambientTexts)
					at.Chance /= 10;
		}


        //WHRIA

        public override int MaxHealth
        {
            get
            {
                int hp = base.MaxHealth;
                double hpPercent = 1.0;

                // apply boosted hp reduction
                if (Constitution > 0 && Constitution < 30)
                {
                    hpPercent = (Constitution * 3.4) * .01;
                }

                if (this.Name.ToLower().Contains("air"))
                    return (int)((hp * hpPercent) / 3 * 1.4);
                else if (this.Name.ToLower().Contains("ice"))
                    return (int)((hp * hpPercent) / 3 * 1.2);
                return (int)((hp * hpPercent) / 3);

            }
        }
        public override double AttackDamage(InventoryItem weapon)
        {
            if (this.Name.ToLower().Contains("air"))
                return (base.AttackDamage(weapon) * 0.6 * ServerProperties.Properties.THEURGIST_PET_DAMAGE_MULTIFLIER);
            else if (this.Name.ToLower().Contains("ice"))
                return (base.AttackDamage(weapon) * 0.8 * ServerProperties.Properties.THEURGIST_PET_DAMAGE_MULTIFLIER);
            return (base.AttackDamage(weapon) * 1.0 * ServerProperties.Properties.THEURGIST_PET_DAMAGE_MULTIFLIER); 
        }
        //END


	}
}
