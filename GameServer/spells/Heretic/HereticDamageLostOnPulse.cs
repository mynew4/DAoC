using System;
using System.Collections;
using DOL.GS;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{

	[SpellHandlerAttribute("HereticDoTLostOnPulse")]
    public class HereticDoTLostOnPulse : HereticPiercingMagic
	{
        protected int m_lastdamage = 0;
        protected int m_pulsedamage = 0;
    //    protected int m_pulsecount = -1;
        
        public override void FinishSpellCast(GameLiving target)
        {
            BeginEffect();
            base.FinishSpellCast(target);
        }

        public override double GetLevelModFactor()
        {
            return 0;
        }

        public override bool IsOverwritable(GameSpellEffect compare)
        {
            if (base.IsOverwritable(compare) == false) return false;
            if (compare.Spell.Duration != Spell.Duration) return false;
            return true;
        }

        public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
        {
            AttackData ad = base.CalculateDamageToTarget(target, effectiveness);
            ad.CriticalDamage = 0;
            ad.AttackType = AttackData.eAttackType.Unknown;
            return ad;
        }

// WHRIA
        public override bool CasterIsAttacked(GameLiving attacker)
        {
            // MELEE ATTACK, attacktype is better.
            if (Caster.IsWithinRadius(attacker, 100) == false)
                return false;

            return base.CasterIsAttacked(attacker);
        }

        public override void EventAction(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving player = sender as GameLiving;

            if (player == null) return;

            

            if (args is AttackedByEnemyEventArgs
                && (Spell.ID == 14080
                || Spell.ID == 14081
                || Spell.ID == 14082
                ))
            {
                AttackData attackData = (args as AttackedByEnemyEventArgs).AttackData;
                if (attackData.AttackType == AttackData.eAttackType.Ranged
                    || attackData.AttackType == AttackData.eAttackType.Spell)
                    return;
            }

            MessageToCaster("You lose your concentration!", eChatType.CT_SpellExpires);
            RemoveEffect();
        }

// END



        public override void CalculateDamageVariance(GameLiving target, out double min, out double max)
        {
            int speclevel = 1;
            if (m_caster is GamePlayer)
            {
                speclevel = ((GamePlayer)m_caster).GetModifiedSpecLevel(m_spellLine.Spec);
            }
            min = 1;
            max = 1;

            if (target.Level > 0)
            {
                min = 0.5 + (speclevel - 1) / (double)target.Level * 0.5;
            }

            if (speclevel - 1 > target.Level)
            {
                double overspecBonus = (speclevel - 1 - target.Level) * 0.005;
                min += overspecBonus;
                max += overspecBonus;
            }

            if (min > max) min = max;
            if (min < 0) min = 0;
        }


        protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
        {
            base.CreateSpellEffect(target, effectiveness);
            // damage is not reduced with distance
            return new GameSpellEffect(this, m_spell.Duration, m_spellLine.IsBaseLine ? 3000 : 2000, 1);
        }


        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            SendEffectAnimation(effect.Owner, 0, false, 1);
        }


        public override void OnEffectPulse(GameSpellEffect effect)
        {
            GameLiving t = effect.Owner;
            GameLiving target = t.TargetObject as GameLiving;
            if (m_caster.Mana < Spell.PulsePower)
            {
                RemoveEffect();
				return;
            }

            if ( !m_caster.IsAlive || !effect.Owner.IsAlive || m_caster.Mana < Spell.PulsePower || !m_caster.IsWithinRadius( effect.Owner, Spell.Range ) || m_caster.IsMezzed || m_caster.IsStunned || ( m_caster.TargetObject is GameLiving ? effect.Owner != m_caster.TargetObject as GameLiving : true ) )
			{
				RemoveEffect();
				return;
			}
            if (!m_caster.TargetInView)
            {
                RemoveEffect();
                return;
            }
            base.OnEffectPulse(effect);

            SendEffectAnimation(effect.Owner, 0, false, 1);

            MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);

            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), eChatType.CT_YouHit, effect.Owner);
           
            OnDirectEffect(effect.Owner, effect.Effectiveness);

            m_caster.Mana -= effect.Spell.PulsePower;
        }


        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            base.OnEffectExpires(effect, noMessages);
            if (!noMessages)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);

                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }

            return 0;
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;
            if (Util.Chance(CalculateSpellResistChance(target)))
            {
                OnSpellResist(target);
                return;
            }
            AttackData ad = CalculateDamageToTarget(target, effectiveness);

            if (m_lastdamage <= 0)
            {
                m_lastdamage = ad.Damage;
            }
            else
            {
                m_pulsedamage = m_lastdamage / 4;
                if (target == focustarget) 
                m_lastdamage += m_pulsedamage;
            }

            ad.Damage = m_lastdamage;

            SendEffectAnimation(target, 0, false, 1);
            SendDamageMessages(ad);
            DamageTarget(ad);
        }

        protected virtual void OnSpellResist(GameLiving target)
        {
// WHRIA
//			m_lastdamage -= m_lastdamage / 4;
            SendEffectAnimation(target, 0, false, 0);
            if (target is GameNPC)
            {
                IControlledBrain brain = ((GameNPC)target).Brain as IControlledBrain;
                if (brain != null)
                {
                    GamePlayer owner = brain.GetPlayerOwner();
					//Worthless checks
                    if (owner != null/* && owner.ControlledNpc != null && target == owner.ControlledNpc.Body*/)
                    {
                        MessageToLiving(owner, "Your " + target.Name + " resists the effect!", eChatType.CT_SpellResisted);
                    }
                }
            }
            else
            {
                MessageToLiving(target, "You resist the effect!", eChatType.CT_SpellResisted);
            }
            MessageToCaster(target.GetName(0, true) + " resists the effect!", eChatType.CT_SpellResisted);

            if (Spell.Damage != 0)
            {
                // notify target about missed attack for spells with damage
                AttackData ad = new AttackData();
                ad.Attacker = Caster;
                ad.Target = target;
                ad.AttackType = AttackData.eAttackType.Spell;
                ad.AttackResult = GameLiving.eAttackResult.Missed;
				ad.SpellHandler = this;
                target.OnAttackedByEnemy(ad);
                target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
            }
            else if (Spell.CastTime > 0)
            {
                target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
            }

            if (target is GameNPC)
            {
                IOldAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IOldAggressiveBrain;
                if (aggroBrain != null)
                    aggroBrain.AddToAggroList(Caster, 1);
            }
			if (target.Realm == 0 || Caster.Realm == 0)
			{
				target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
				Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
			}
			else
			{
				target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
				Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
			}
        }

        public virtual void DamageTarget(AttackData ad)
        {
            ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
            ad.Target.OnAttackedByEnemy(ad);
            ad.Attacker.DealDamage(ad);
            foreach (GamePlayer player in ad.Attacker.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendCombatAnimation(null, ad.Target, 0, 0, 0, 0, 0x0A, ad.Target.HealthPercent);
            }
        }

		public HereticDoTLostOnPulse(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
