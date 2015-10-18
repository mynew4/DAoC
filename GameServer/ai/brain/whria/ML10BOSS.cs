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
 
 
 
 
 
  Ce script comprend l'ensemble de ce qu'il faut pour un Mob special,
  
  Est contenu le mob en lui meme (premiere partie), L'AI customisable du mob en deuxieme et le Brain customisable en derniere partie
 
 
 
 
 
 
 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using DOL.AI;
using DOL.AI.Brain;

using DOL.Events;
using DOL.Database;

using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using DOL.GS.SkillHandler;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.GS.Utils;
using DOL.GS.RealmAbilities;

using DOL.Language;

using log4net;




/* ========================================================================================== CUSTOM MOB ========================================================================================== */

/* /mob create DOL.GS.ML6BossMob
	*/
namespace DOL.GS
{
    /// <summary>
    /// New Custom Mob 
    /// </summary>
    public class ML10BossMob : GameNPC // on indique ici que le mob fera partie des GameNPC classic
    {
        private static int TauntID = 167;
        private static int TauntClassID = 22;
        Style taunt = SkillBase.GetStyleByID(TauntID, TauntClassID);

        //private static int RevengeID = 169;
        //private static int RevengeClassID = 22;

        // private static int ThorID = 172;
        //private static int ThorClassID = 22;

        private static int SlamID = 228;
        private static int SlamClassID = 2;
        Style slam = SkillBase.GetStyleByID(SlamID, SlamClassID);

        private static int ColdHammerID = 160;
        private static int ColdHammerClassID = 31;
        Style SideStyle = SkillBase.GetStyleByID(ColdHammerID, ColdHammerClassID);





        private int dmgmulti = 1; // Variable pour la multiplication des dommages engendré par le mob


        // Gestion des Adds
        ML10POPMob1Brain popbrain = null; //on declare que le mot popbrain sera = a un ML6POPMob1Brain (le script)
        ML10POPMob1 MOB1 = null; //on declare que le mot MOB1 sera = a un ML6POPMob1 (le script)



        public override bool AddToWorld()
        {
            if (base.AddToWorld())
            {
                return true;
            }
            return false;
        }

        protected override Style GetStyleToUse()
        {
            if (this.TargetObject == null)
            {
                return base.GetStyleToUse();
            }
            if (this.TargetObject is GameLiving) // on definie la position par rapport a la cible
            {
                float angle = this.TargetObject.GetAngle(this);
                GameLiving living = this.TargetObject as GameLiving;


                if (Util.Chance(100))
                {
                    if ((((angle >= 45 && angle < 150) || (angle >= 210 && angle < 315)) && IsLivingStunned(living) == false) && this.GetDistanceTo(living) >= 60) // side
                    {
                        Style GetSideStyle = DoSideStyle(living);
                        if (GetSideStyle != null)
                        {
                            //  Console.WriteLine("GetSideStyle returned");
                            return GetSideStyle;
                        }
                    }
                    else if (angle >= 150 && angle < 210) // BACK 
                    {
                        Style GetSideStyle = DoSideStyle(living);
                        if (GetSideStyle != null)
                        {
                            //  Console.WriteLine("GetBackStyle returned");
                            return GetSideStyle;
                        }
                    }
                    else //we can not do back style.. we can not do side style.. !
                    {
                        if (IsLivingStunned(living)) // if living is stunned !! (ONly does it if the living is actually stunned!))
                        {
                            //move to the side
                            Point2D positionalPoint;
                            positionalPoint = living.GetPointFromHeading((ushort)((living.Heading + 2048) & 0xFFF), 75);
                            this.WalkTo(positionalPoint.X, positionalPoint.Y, living.Z, 1250);
                        }
                        else if (HasLivingGotStunImmunity(living) == false)
                        {
                            //slam time !

                            //  Console.WriteLine(this.Name + " Slam");
                            Style slam = SkillBase.GetStyleByID(SlamID, SlamClassID); // slam
                            if (slam != null)
                            {
                                this.SwitchWeapon(eActiveWeaponSlot.Standard);

                                return taunt;
                            }
                        }
                        else
                        {
                            Style taunt = SkillBase.GetStyleByID(TauntID, TauntClassID); // taunt
                            if (taunt != null)
                            {
                                this.SwitchWeapon(eActiveWeaponSlot.Standard);

                                return taunt;
                            }

                        }
                    }
                }
            }
            //  Console.WriteLine(this.Name + " RETURN NORMAL");
            return base.GetStyleToUse();
        }
        public Style DoSideStyle(GameLiving living)
        {
            //side style!
            if (Inventory != null)  // on verifie dans l'inventaire du mob voir si il pocede une arme a 2 main ou non. si oui il l'equipe
            {
                InventoryItem twoHandSlot = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
                if (twoHandSlot != null)
                {
                    this.SwitchWeapon(eActiveWeaponSlot.TwoHanded);
                    //  Console.WriteLine(this.Name + " twohanded");
                }
                else
                {
                    this.SwitchWeapon(eActiveWeaponSlot.Standard);
                    //  Console.WriteLine(this.Name + " Standard");
                }
            }
            //  Console.WriteLine(this.Name + " ColdHammer");
            Style sideStyle = SkillBase.GetStyleByID(ColdHammerID, ColdHammerClassID);
            if (sideStyle != null)
            {

                return sideStyle;

            }


            return null;
        }
        public Style DoBackStyle(GameLiving living)
        {
            if (HasLivingGotStunImmunity(living) == true) // on verifie si la cible a un immune stun
            {
                //  Console.WriteLine(this.Name + " Taunt");
                Style taunt = SkillBase.GetStyleByID(TauntID, TauntClassID); // taunt
                if (taunt != null)
                {
                    this.SwitchWeapon(eActiveWeaponSlot.Standard);

                    return SideStyle;
                }
            }
            else
            {
                // Console.WriteLine(this.Name + " Slam");
                Style slam = SkillBase.GetStyleByID(SlamID, SlamClassID); // slam
                if (slam != null)
                {
                    this.SwitchWeapon(eActiveWeaponSlot.Standard);

                    // return slam;
                }
            }


            return null;
        }

        public static bool IsLivingStunned(GameLiving living)
        {
            lock (living.EffectList)
            {
                foreach (IGameEffect effect in living.EffectList)
                {
                    if (effect is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)effect).ImmunityState)
                    {
                        continue;
                    }


                    if (effect is GameSpellEffect && ((effect as GameSpellEffect).SpellHandler is StunSpellHandler || (effect as GameSpellEffect).SpellHandler is UnresistableStunSpellHandler || (effect as GameSpellEffect).SpellHandler is UnrresistableNonImunityStun))
                    {

                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HasLivingGotStunImmunity(GameLiving living)
        {
            lock (living.EffectList)
            {
                foreach (IGameEffect effect in living.EffectList)
                {
                    if (effect is GameSpellEffect && ((effect as GameSpellEffect).SpellHandler is StunSpellHandler || (effect as GameSpellEffect).SpellHandler is UnresistableStunSpellHandler || (effect as GameSpellEffect).SpellHandler is UnrresistableNonImunityStun))
                    {
                        if (effect is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)effect).ImmunityState)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public ML10BossMob()
            : base()
        {

            SetOwnBrain(new ML10BossMobBrain()); // on attribut l'AI du Mob

        }



        public void POP1() // POP1 sera un mot public ... en gros , il suffira de placer POP1(); pour executer ce qu'il contient.
        {
            MOB1 = new ML10POPMob1(); // on execute le script DOL.GS.ML6POPMob1 et on lui donne les valeurs qui suivent et que tu connais
            MOB1.X = 30796;
            MOB1.Y = 31015;
            MOB1.Z = 16067;
            MOB1.CurrentRegion = CurrentRegion;
            MOB1.Heading = 3340;
            MOB1.Level = 1;
            MOB1.Realm = 0;
            MOB1.Name = "";
            MOB1.Model = 1;
            MOB1.CurrentSpeed = 0;
            MOB1.MaxSpeedBase = 350;
            MOB1.GuildName = "";
            MOB1.Size = 1;
            popbrain = new ML10POPMob1Brain();
            popbrain.AggroLevel = 100;
            popbrain.AggroRange = 3500;
            MOB1.SetOwnBrain(popbrain);
            MOB1.AddToWorld();
        }
        public void POP2() // POP1 sera un mot public ... en gros , il suffira de placer POP1(); pour executer ce qu'il contient.
        {
            MOB1 = new ML10POPMob1(); // on execute le script DOL.GS.ML6POPMob1 et on lui donne les valeurs qui suivent et que tu connais
            MOB1.X = 30787;
            MOB1.Y = 33242;
            MOB1.Z = 16067;
            MOB1.CurrentRegion = CurrentRegion;
            MOB1.Heading = 2492;
            MOB1.Level = 1;
            MOB1.Realm = 0;
            MOB1.Name = "";
            MOB1.Model = 1;
            MOB1.CurrentSpeed = 0;
            MOB1.MaxSpeedBase = 350;
            MOB1.GuildName = "";
            MOB1.Size = 1;
            popbrain = new ML10POPMob1Brain();
            popbrain.AggroLevel = 100;
            popbrain.AggroRange = 3500;
            MOB1.SetOwnBrain(popbrain);
            MOB1.AddToWorld();
        }
        public void POP3() // POP1 sera un mot public ... en gros , il suffira de placer POP1(); pour executer ce qu'il contient.
        {
            MOB1 = new ML10POPMob1(); // on execute le script DOL.GS.ML6POPMob1 et on lui donne les valeurs qui suivent et que tu connais
            MOB1.X = 33043;
            MOB1.Y = 30752;
            MOB1.Z = 16064;
            MOB1.CurrentRegion = CurrentRegion;
            MOB1.Heading = 103;
            MOB1.Level = 1;
            MOB1.Realm = 0;
            MOB1.Name = "";
            MOB1.Model = 1;
            MOB1.CurrentSpeed = 0;
            MOB1.MaxSpeedBase = 350;
            MOB1.GuildName = "";
            MOB1.Size = 1;
            popbrain = new ML10POPMob1Brain();
            popbrain.AggroLevel = 100;
            popbrain.AggroRange = 3500;
            MOB1.SetOwnBrain(popbrain);
            MOB1.AddToWorld();
        }
        public void POP4() // POP1 sera un mot public ... en gros , il suffira de placer POP1(); pour executer ce qu'il contient.
        {
            MOB1 = new ML10POPMob1(); // on execute le script DOL.GS.ML6POPMob1 et on lui donne les valeurs qui suivent et que tu connais
            MOB1.X = 34191;
            MOB1.Y = 32000;
            MOB1.Z = 16064;
            MOB1.CurrentRegion = CurrentRegion;
            MOB1.Heading = 996;
            MOB1.Level = 1;
            MOB1.Realm = 0;
            MOB1.Name = "";
            MOB1.Model = 1;
            MOB1.CurrentSpeed = 0;
            MOB1.MaxSpeedBase = 350;
            MOB1.GuildName = "";
            MOB1.Size = 1;
            popbrain = new ML10POPMob1Brain();
            popbrain.AggroLevel = 100;
            popbrain.AggroRange = 3500;
            MOB1.SetOwnBrain(popbrain);
            MOB1.AddToWorld();
        }
        public void POP5() // POP1 sera un mot public ... en gros , il suffira de placer POP1(); pour executer ce qu'il contient.S
        {
            MOB1 = new ML10POPMob1(); // on execute le script DOL.GS.ML6POPMob1 et on lui donne les valeurs qui suivent et que tu connais
            MOB1.X = 33032;
            MOB1.Y = 33149;
            MOB1.Z = 16064;
            MOB1.CurrentRegion = CurrentRegion;
            MOB1.Heading = 2049;
            MOB1.Level = 1;
            MOB1.Realm = 0;
            MOB1.Name = "";
            MOB1.Model = 1;
            MOB1.CurrentSpeed = 0;
            MOB1.MaxSpeedBase = 350;
            MOB1.GuildName = "";
            MOB1.Size = 1;
            popbrain = new ML10POPMob1Brain();
            popbrain.AggroLevel = 100;
            popbrain.AggroRange = 3500;
            MOB1.SetOwnBrain(popbrain);
            MOB1.AddToWorld();
        }
        public void POP6() // POP1 sera un mot public ... en gros , il suffira de placer POP1(); pour executer ce qu'il contient.S
        {
            MOB1 = new ML10POPMob1(); // on execute le script DOL.GS.ML6POPMob1 et on lui donne les valeurs qui suivent et que tu connais
            MOB1.X = 33464;
            MOB1.Y = 31943;
            MOB1.Z = 15996;
            MOB1.CurrentRegion = CurrentRegion;
            MOB1.Heading = 758;
            MOB1.Level = 1;
            MOB1.Realm = 0;
            MOB1.Name = "";
            MOB1.Model = 1;
            MOB1.CurrentSpeed = 0;
            MOB1.MaxSpeedBase = 350;
            MOB1.GuildName = "";
            MOB1.Size = 1;
            popbrain = new ML10POPMob1Brain();
            popbrain.AggroLevel = 100;
            popbrain.AggroRange = 3500;
            MOB1.SetOwnBrain(popbrain);
            MOB1.AddToWorld();
        }
        public void POP7() // POP1 sera un mot public ... en gros , il suffira de placer POP1(); pour executer ce qu'il contient.S
        {
            MOB1 = new ML10POPMob1(); // on execute le script DOL.GS.ML6POPMob1 et on lui donne les valeurs qui suivent et que tu connais
            MOB1.X = 33302;
            MOB1.Y = 32287;
            MOB1.Z = 15996;
            MOB1.CurrentRegion = CurrentRegion;
            MOB1.Heading = 1197;
            MOB1.Level = 1;
            MOB1.Realm = 0;
            MOB1.Name = "";
            MOB1.Model = 1;
            MOB1.CurrentSpeed = 0;
            MOB1.MaxSpeedBase = 350;
            MOB1.GuildName = "";
            MOB1.Size = 1;
            popbrain = new ML10POPMob1Brain();
            popbrain.AggroLevel = 100;
            popbrain.AggroRange = 3500;
            MOB1.SetOwnBrain(popbrain);
            MOB1.AddToWorld();
        }
        public void POP8() // POP1 sera un mot public ... en gros , il suffira de placer POP1(); pour executer ce qu'il contient.S
        {
            MOB1 = new ML10POPMob1(); // on execute le script DOL.GS.ML6POPMob1 et on lui donne les valeurs qui suivent et que tu connais
            MOB1.X = 32426;
            MOB1.Y = 32406;
            MOB1.Z = 16000;
            MOB1.CurrentRegion = CurrentRegion;
            MOB1.Heading = 2052;
            MOB1.Level = 1;
            MOB1.Realm = 0;
            MOB1.Name = "";
            MOB1.Model = 1;
            MOB1.CurrentSpeed = 0;
            MOB1.MaxSpeedBase = 350;
            MOB1.GuildName = "";
            MOB1.Size = 1;
            popbrain = new ML10POPMob1Brain();
            popbrain.AggroLevel = 100;
            popbrain.AggroRange = 3500;
            MOB1.SetOwnBrain(popbrain);
            MOB1.AddToWorld();
        }
        public void POP9() // POP1 sera un mot public ... en gros , il suffira de placer POP1(); pour executer ce qu'il contient.S
        {
            MOB1 = new ML10POPMob1(); // on execute le script DOL.GS.ML6POPMob1 et on lui donne les valeurs qui suivent et que tu connais
            MOB1.X = 32422;
            MOB1.Y = 31779;
            MOB1.Z = 15998;
            MOB1.CurrentRegion = CurrentRegion;
            MOB1.Heading = 4;
            MOB1.Level = 1;
            MOB1.Realm = 0;
            MOB1.Name = "";
            MOB1.Model = 1;
            MOB1.CurrentSpeed = 0;
            MOB1.MaxSpeedBase = 350;
            MOB1.GuildName = "";
            MOB1.Size = 1;
            popbrain = new ML10POPMob1Brain();
            popbrain.AggroLevel = 100;
            popbrain.AggroRange = 3500;
            MOB1.SetOwnBrain(popbrain);
            MOB1.AddToWorld();
        }
        public void POP10() // POP1 sera un mot public ... en gros , il suffira de placer POP1(); pour executer ce qu'il contient.S
        {
            MOB1 = new ML10POPMob1(); // on execute le script DOL.GS.ML6POPMob1 et on lui donne les valeurs qui suivent et que tu connais
            MOB1.X = 30005;
            MOB1.Y = 32127;
            MOB1.Z = 16067;
            MOB1.CurrentRegion = CurrentRegion;
            MOB1.Heading = 233;
            MOB1.Level = 1;
            MOB1.Realm = 0;
            MOB1.Name = "";
            MOB1.Model = 1;
            MOB1.CurrentSpeed = 0;
            MOB1.MaxSpeedBase = 350;
            MOB1.GuildName = "";
            MOB1.Size = 1;
            popbrain = new ML10POPMob1Brain();
            popbrain.AggroLevel = 100;
            popbrain.AggroRange = 3500;
            MOB1.SetOwnBrain(popbrain);
            MOB1.AddToWorld();
        }
        /*----------------------------------------------------------*/



        public override void Die(GameObject killer)
        {
            base.Die(killer);
        }

        public override int AttackRange
        {
            get
            {
                return base.AttackRange;
            }
            set
            {
                base.AttackRange = value;
            }
        }

        public override double AttackDamage(InventoryItem weapon)
        {
            if (this.TargetObject != null) // controle securitaire : on verifie que le mob a bien une cible
            {
                if (this.TargetObject is GamePet) // Si la cible est un Pet
                {
                    if (this.Level >= 75) // si le level du mob est Sup ou egal a 75 on multiplie les degats par 10 sinon par 2 (mais toujours pour les Pet uniquement
                    {
                        return base.AttackDamage(weapon) * (dmgmulti * 15);
                    }
                    return base.AttackDamage(weapon) * (dmgmulti * 1.5);
                }
            }

            if (this.Name != this.Name.ToLower())
            {
                return (int)(base.AttackDamage(weapon) * (dmgmulti * 3.1));
            }

            if (this.Level <= 5) // en dessous du level 6 on tape degat * 1 
            {
                return base.AttackDamage(weapon)*(dmgmulti/3);
            }

            if (this.Level >= 50) // a partir du level 50 on multipli les degats par : 2 * la valeur de la variable dmgmulti
            {
                return base.AttackDamage(weapon) * (dmgmulti * 1.5);
            }

            return (base.AttackDamage(weapon) * dmgmulti); // degats envoyés si : la cible n'est pas un Pet et que le level du mob est superieur a 5 et inferieur a 50
        }

        public override int MaxHealth  // Maximum de vie du Mob
        {
            get
            {
                if (this.Name != this.Name.ToLower() && this.Level >= 65) // si le mob a un level 65 ou plus :
                {
                    return 150000; //(int)(base.MaxHealth * 4.30); // au level 71 = 150 000 pv
                }
                return (int)(base.MaxHealth); // si il se situe entre le level 10 et 50 ... on ne multiplie rien et on retourne la vie par defaut.
            }
        }

        public override int AttackSpeed(params InventoryItem[] weapon) // Vitesse de l'arme
        {
            return base.AttackSpeed(weapon);
        }




       

    }
}








/* ==========================================================================================   AI du MOB  ========================================================================================== */

/* ========================================================================================== CUSTOM BRAIN ========================================================================================== */

// voila , ici on a le Brain Final celui ou le programme ira cherché les infos qui n'ont pas été abordé dans les "post-Brain" precedent ... on va y retouver le think , le think interval et tout le merdié...



namespace DOL.AI.Brain
{
    /// <summary>
    /// Standard brain for standard mobs
    /// </summary>
    public class ML10BossMobBrain : APlayerVicinityBrain
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public const int MAX_AGGRO_DISTANCE = 3600;
        public const int MAX_AGGRO_LIST_DISTANCE = 6000;
        public const int MAX_PET_AGGRO_DISTANCE = 512; // Tolakram - Live test with caby pet - I was extremely close before auto aggro

        public bool think_custom_interval = false; // laisser cette valeur a false fera que le mob aura un think interval de 1500 ... mettre true pour le calcul selon le level du mob
        public int thinkInt = 1500;


        // ici on declare quelle type de joueur sera la priorité du mob ... une seul type peut etre prioritaire ... passer sa valeur a 1 et les autres devront rester a zero.
        public int cibler_healer = 0;
        public int cibler_caster = 0;
        public int cibler_Pet = 0;



        public bool cibler_heal = false;
        public bool cibler_cast = false;
        public bool cibler_pet = false;
        public bool cibler_all = true;

        public bool phase1 = true;
        public bool phase2 = true;
        public bool phase3 = true;
        public bool phase4 = true;
        public bool phase5 = true;
        public bool phase6 = true;
        public bool phase7 = true;
        public bool phase8 = true;
        public bool phase9 = true;
        public bool phase10 = true;

        public bool haste1 = true;
        public bool haste2 = false;
        public bool haste3 = false;
        public bool haste4 = false;
        public bool haste5 = false;

        public bool gotstunted = false;


        /// <summary>
        /// Constructs a new ML6BossMobBrain
        /// </summary>
        public ML10BossMobBrain()
            : base()
        {
            m_aggroLevel = 100;
            m_aggroMaxRange = 1500;

        }

        // system de selection du joueur cible selon les classe, on lance avec : GameLiving target = faithPickTarget((ushort)this.AggroRange);    ou    GameLiving target = faithPickTarget((ushort)500); par exemple
        public GameLiving faithPickTarget(ushort radius) // ici on defini la cible
        {
            foreach (GamePlayer player in Body.GetPlayersInRadius(radius)) // controle dans un rayon de (radius) la presence de joueur (par contre pour radius je vois pas ou il est defini mais c'est pas tres grave)
            {
                if (cibler_healer > cibler_caster && cibler_caster == cibler_Pet)
                {
                    cibler_heal = true;
                    cibler_all = false;
                }
                if (cibler_caster > cibler_healer && cibler_healer == cibler_Pet)
                {
                    cibler_cast = true;
                    cibler_all = false;
                }
                if (cibler_Pet > cibler_healer && cibler_healer == cibler_caster)
                {
                    cibler_pet = true;
                    cibler_all = false;
                }




               
            }

            foreach (GameNPC npc in Body.GetNPCsInRadius(radius)) // controle de mob present
            {
                if (npc == null) continue; // pas de mob
                if (npc.IsAlive == false) continue; //pas de mob vivant
                if (GameServer.ServerRules.IsAllowedToAttack(this.Body, npc, false)) // attaquable
                {
                    if (npc is GamePet && (!cibler_pet || cibler_all)) // et uniquement si c'est un pet
                    {
                        return npc; // le pet devient la cible (en gros les Pet sont toujours les derniers attaqués ... )
                    }
                }
            }
            cibler_Pet = 0;
            return null; // si aucune verification n'abouti on retourne rien
        }


        /// <summary>
        /// The interval for thinking, min 1.5 seconds
        /// 10 seconds for 0 aggro mobs
        /// </summary>
        public override int ThinkInterval
        {
            get
            {
                if (think_custom_interval)
                {
                    thinkInt = this.Body.Level * 100;

                    if (thinkInt >= 3500)
                    {
                        thinkInt = 1500;
                    }
                }
                return thinkInt;
            }
        }

        /// <summary>
        /// Do the mob AI
        /// </summary>
        public override void Think()
        {
            ML10BossMob MOB = Body as ML10BossMob; // a partir de maintenant on pourra ecrire MOB au lieu de Body . cela designe dans les deux cas le mob   */

            if (MOB.HealthPercent == 100)
            {
                phase1 = true;
                phase2 = true;
                phase3 = true;
                phase4 = true;
                phase5 = true;
                phase6 = true;
                phase7 = true;
                phase8 = true;
                phase9 = true;
                phase10 = true;
            }
            if (MOB.Model != 2121) // controle securitaire : Si le mob n'a pas le bon model on lui donne immediatement. (j'avait fait le mob Merlin donc ca devrait etre un gus ^^ )
            {
                MOB.Model = 2121; // je decrit pas car ca tu connais
                MOB.Size = 90;
                MOB.Level = 71;
                MOB.Realm = eRealm.None;
                MOB.Name = "Draco's Slave";
                MOB.GuildName = "";
                MOB.Race = 71;
                MOB.CurrentSpeed = 0;
                MOB.MaxSpeedBase = 200;                
                MOB.Mana = 10000;
                MOB.RespawnInterval = -1;
            }

            if (MOB.HealthPercent >= 90 && MOB.HealthPercent < 100) // phase 1
            {
                if (Body.IsStunned)
                {
                    Console.WriteLine("phase 1 activer je suis stun");
                    phase1 = true;
                    gotstunted = true;
                }

                if (phase1 == true && gotstunted == true)
                {
                    if (!Body.IsStunned)
                    {
                        Console.WriteLine("je suis plus stun je land mon aoe maladie");
                        Body.CastSpell(MLAoEDisease, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        phase1 = false;
                        gotstunted = false;
                    }
                }
            }


            if (MOB.HealthPercent >= 80 && MOB.HealthPercent <= 90) // phase 2
            {
                if (phase2 == true) // MLStunSpell
                {
                    Console.WriteLine("phase 2 j invoque mes mobs");
                    Body.CastSpell(MLStunSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    MOB.POP1();
                    MOB.POP2();
                    MOB.POP3();
                    MOB.POP4();
                    MOB.POP5();
                    MOB.POP6();
                    MOB.POP7();
                    MOB.POP8();
                    MOB.POP9();
                    MOB.POP10();
                    phase2 = false;
                }
            }

            if (MOB.HealthPercent >= 70 && MOB.HealthPercent <= 80) // phase 3
            {
                if (Body.IsStunned)
                {
                    Console.WriteLine("je suis stun, phase 3 active");
                    phase3 = true;
                    gotstunted = true;
                }

                if (phase3 == true && gotstunted == true)
                {
                    if (!Body.IsStunned)
                    {
                        Console.WriteLine("je suis plus stun, je maladie de zone");
                        Body.CastSpell(MLAoEDisease, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        phase3 = false;
                        gotstunted = false;
                    }
                }

            }

            if (MOB.HealthPercent >= 60 && MOB.HealthPercent <= 70) // phase 4
            {
                if (phase4 == true) // MLStunSpell
                {

                    if (!Body.IsStunned)
                    {
                        Console.WriteLine("phase 4 je stun et j'invoque mes mob");
                        Body.CastSpell(MLStunSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        MOB.POP1();
                        MOB.POP2();
                        MOB.POP3();
                        MOB.POP4();
                        MOB.POP5();
                        MOB.POP6();
                        MOB.POP7();
                        MOB.POP8();
                        MOB.POP9();
                        MOB.POP10();
                        phase4 = false;
                    }
                }
            }

            if (MOB.HealthPercent >= 50 && MOB.HealthPercent <= 60) // phase 5
            {
                if (Body.IsStunned)
                {
                    Console.WriteLine("je suis stun et phase 5 active");
                    phase5 = true;
                    gotstunted = true;
                }

                if (phase5 == true && gotstunted == true)
                {
                    if (!Body.IsStunned)
                    {
                        Console.WriteLine("je suis plus stun et aoe maladie");
                        Body.CastSpell(MLAoEDisease, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        phase5 = false;
                        gotstunted = false;
                    }
                }
            }

            if (MOB.HealthPercent >= 40 && MOB.HealthPercent <= 50) // phase 6   
            {
                if (Body.IsStunned)
                {
                    Console.WriteLine("je suis stun et phase 5 active");
                    phase6 = true;
                    gotstunted = true;
                }

                if (phase6 == true && gotstunted == true)
                {
                    if (!Body.IsStunned)
                    {
                        Console.WriteLine("phase 6 j aoe dot");
                        Body.CastSpell(MLDotAoeSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        phase6 = false;
                        gotstunted = false;
                    }
                }
            }

            if (MOB.HealthPercent >= 30 && MOB.HealthPercent <= 40) // phase 7
            {
                if (phase7 == true) // MLStunSpell
                {

                    if (!Body.IsStunned)
                    {
                        Console.WriteLine("phase 7 aoe stun + invoque des mob");
                        Body.CastSpell(MLStunSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        MOB.POP1();
                        MOB.POP2();
                        MOB.POP3();
                        MOB.POP4();
                        MOB.POP5();
                        MOB.POP6();
                        MOB.POP7();
                        MOB.POP8();
                        MOB.POP9();
                        MOB.POP10();
                        phase7 = false;
                    }
                }
            }

            if (MOB.HealthPercent >= 20 && MOB.HealthPercent <= 30) // phase 8
            {

                if (Body.IsStunned)
                {
                    Console.WriteLine("phate 8 et je suis stun");
                    phase8 = true;
                    gotstunted = true;
                    if (haste1 == false)
                    {
                        haste1 = true;
                    }



                }

                if (haste1 == true && phase8 == true && gotstunted == true)
                {
                    if (!Body.IsStunned)
                    {

                        Console.WriteLine("je suis plus stun et je me buff hate");
                        Body.Yell("GOOD !! You think you can Stun me like that ?!!");
                        Body.CastSpell(MLHaste1Spell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        haste1 = false;
                        gotstunted = false;
                        return;
                    }
                }
            }

            if (MOB.HealthPercent >= 10 && MOB.HealthPercent <= 20) // phase 9
            {
                if (phase9 == true)
                {
                    if (!Body.IsStunned)
                    {
                        Console.WriteLine("phase 9 je me buff damage shield");
                        Body.CastSpell(MLDSSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        MOB.POP1();
                        MOB.POP2();
                        MOB.POP3();
                        MOB.POP4();
                        MOB.POP5();
                        MOB.POP6();
                        MOB.POP7();
                        MOB.POP8();
                        MOB.POP9();
                        MOB.POP10();
                        phase9 = false;
                    }
                }
            }

            if (MOB.HealthPercent >= 0 && MOB.HealthPercent <= 10) // phase 10
            {
                if (phase10 == true)
                {
                    if (!Body.IsStunned)
                    {
                        Console.WriteLine("phase 10 je me buff damage add");
                        Body.CastSpell(MLAddSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        MOB.POP1();
                        MOB.POP2();
                        MOB.POP3();
                        MOB.POP4();
                        MOB.POP5();
                        MOB.POP6();
                        MOB.POP7();
                        MOB.POP8();
                        MOB.POP9();
                        MOB.POP10();
                        phase10 = false;
                    }
                }
            }

            // Si on souhaite un type de mob predefini c'est ici

            /* if (MOB.Model != 40) // controle securitaire : Si le mob n'a pas le bon model on lui donne immediatement. (j'avait fait le mob Merlin donc ca devrait etre un gus ^^ )
            {
                MOB.Model = 40; // je decrit pas car ca tu connais
                MOB.Size = 120;
                MOB.Level = 100;
                MOB.Realm = eRealm.None;
                MOB.Name = "Merlin";
                MOB.GuildName = "";
                MOB.CurrentSpeed = 0;
                MOB.MaxSpeedBase = 100;
                MOB.ChangeBaseStat(eStat.CON, 1500);
                MOB.ChangeBaseStat(eStat.STR, 1200);
                MOB.ChangeBaseStat(eStat.DEX, 300);
                MOB.ChangeBaseStat(eStat.QUI, 300);
                MOB.ChangeBaseStat(eStat.INT, 500);
                MOB.ChangeBaseStat(eStat.PIE, 500);
                MOB.Mana = 10000;
                MOB.RespawnInterval = 4000000;
            }

            */




            if (Body.IsOutOfTetherRange && !Body.InCombat) // Si le mob n'est pas en combat et en dehors de son range max 
            {
                Body.WalkToSpawn();
                // il retourne a son point de depart
                phase1 = true;
                phase2 = true;
                phase3 = true;
                phase4 = true;
                phase5 = true;
                phase6 = true;
                phase7 = true;
                phase8 = true;
                phase9 = true;
                phase10 = true;
                if (this.Body.InCombatInLast(30 * 1000) == false && this.Body.InCombatInLast(45 * 1000)) // if not in combat in last 10s.. but in combat in the last 15s.. heal to max..
                {
                    this.Body.Health = this.Body.MaxHealth;
                }
                return;
            }

            //Instead - lets just let CheckSpells() make all the checks for us
            //Check for just positive spells
            CheckSpells(eCheckSpellType.Defensive);

            // Note: Offensive spells are checked in GameNPC:SpellAction timer

            if (Body.MaxDistance != 0 && !Body.IsReturningHome) // si sa distance ma et qu'il n'est pas en train de revenir a son point de pop
            {
                int distance = Body.GetDistanceTo(Body.SpawnPoint);
                int maxdistance = Body.MaxDistance > 0 ? Body.MaxDistance : -Body.MaxDistance * AggroRange / 100;
                if (maxdistance > 0 && distance > maxdistance)
                {
                    Body.WalkToSpawn(); // il y retourne
                    return;
                }
            }

            //If this NPC can randomly walk around, we allow it to walk around
            if (!Body.AttackState && CanRandomWalk && Util.Chance(DOL.GS.ServerProperties.Properties.GAMENPC_RANDOMWALK_CHANCE))
            {
                IPoint3D target = CalcRandomWalkTarget();
                if (target != null)
                {
                    if (Util.IsNearDistance(target.X, target.Y, target.Z, Body.X, Body.Y, Body.Z, GameNPC.CONST_WALKTOTOLERANCE))
                    {
                        Body.TurnTo(Body.GetHeading(target));
                    }
                    else
                    {
                        Body.WalkTo(target, 50);
                    }

                    Body.FireAmbientSentence(GameNPC.eAmbientTrigger.roaming);
                }
            }
            else if (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving && !Body.AttackState && !Body.InCombat)
            {
                //If the npc is not at it's spawn position, we tell it to walk to it's spawn position
                //Satyr: If we use a tolerance to stop their Way back home we also need the same
                //Tolerance to check if we need to go home AGAIN, otherwise we might be told to go home
                //for a few units only and this may end before the next Arrive-At-Target Event is fired and in this case
                //We would never lose the state "IsReturningHome", which is then followed by other erros related to agro again to players
                if (!Util.IsNearDistance(Body.X, Body.Y, Body.Z, Body.SpawnPoint.X, Body.SpawnPoint.Y, Body.SpawnPoint.Z, GameNPC.CONST_WALKTOTOLERANCE))
                    Body.WalkToSpawn();
                else if (Body.Heading != Body.SpawnHeading)
                    Body.Heading = Body.SpawnHeading;
            }

            //Mob will now always walk on their path
            if (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving
                && !Body.AttackState && !Body.InCombat && !Body.IsMovingOnPath
                && Body.PathID != null && Body.PathID != "" && Body.PathID != "NULL")
            {
                PathPoint path = MovementMgr.LoadPath(Body.PathID);
                if (path != null)
                {
                    Body.CurrentWayPoint = path;
                    Body.MoveOnPath((short)path.MaxSpeed);
                }
                else
                {
                    log.ErrorFormat("Path {0} not found for mob {1}.", Body.PathID, Body.Name);
                }
            }

            //If we are not attacking, and not casting, and not moving, and we aren't facing our spawn heading, we turn to the spawn heading
            if (!Body.InCombat && !Body.AttackState && !Body.IsCasting && !Body.IsMoving && Body.IsWithinRadius(Body.SpawnPoint, 500) == false)
            {
                Body.WalkToSpawn(); // Mobs do not walk back at 2x their speed..
                Body.IsReturningHome = false; // We are returning to spawn but not the long walk home, so aggro still possible
            }

            if (Body.IsReturningHome == false)
            {
                //If we have an aggrolevel above 0, we check for players and npcs in the area to attack
                if (!Body.AttackState && AggroLevel > 0)
                {
                    CheckPlayerAggro();
                    CheckNPCAggro();
                }

                if (HasAggro)
                {
                    Body.FireAmbientSentence(GameNPC.eAmbientTrigger.fighting);
                    AttackMostWanted();
                    return;
                }
                else
                {
                    if (Body.AttackState)
                        Body.StopAttack();

                    Body.TargetObject = null;
                }
            }
        }

        // declacration du sort !!!!! tres important pour creer tes mob custom !
        // pour le lancer il suffira de placer ceci : Body.CastSpell(Nuke, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells)); // Le mob cast son nuke

        /// <summary>
        /// The stun spell
        /// </summary>
        public Spell MLAoEDisease
        {
            get
            {
                DBSpell spell = new DBSpell();
                spell.AllowAdd = false;
                spell.CastTime = 0;
                spell.ClientEffect = 3425;
                spell.Icon = 3425;
                spell.Value = 0;
                spell.Duration = 180;
                spell.Name = "The Disease of God";
                spell.Range = 2000;
                spell.SpellID = 1000001;
                spell.Target = "Enemy";
                spell.Type = "Disease";
                spell.EffectGroup = 30;
                spell.Radius = 1500;
                spell.Uninterruptible = true;
                return new Spell(spell, 50);
            }
        }

        /// <summary>
        /// The stun spell
        /// </summary>
        public Spell MLStunSpell
        {
            get
            {

                DBSpell spell = new DBSpell();
                spell.AllowAdd = false;
                spell.CastTime = 0;
                spell.ClientEffect = 3379;
                spell.Icon = 3379;
                spell.Value = 0;
                spell.Duration = 4;
                spell.Name = "The Stun og God";
                spell.Range = 450;
                spell.SpellID = 1000002;
                spell.Target = "Enemy";
                spell.Type = "Stun";
                spell.Radius = 300;
                spell.EffectGroup = 20;
                spell.Uninterruptible = true;
                return new Spell(spell, 50);

            }
        }

        /// <summary>
        /// The stun spell
        /// </summary>
        public Spell MLHaste1Spell
        {
            get
            {

                DBSpell spell = new DBSpell();
                spell.AllowAdd = false;
                spell.CastTime = 0;
                spell.ClientEffect = 1723;
                spell.Icon = 0;
                spell.Value = 80;
                spell.Duration = 40;
                spell.Name = "The Hast of God 1";
                spell.Range = 0;
                spell.SpellID = 1000003;
                spell.Target = "Self";
                spell.Type = "CombatSpeedBuff";
                spell.EffectGroup = 20;
                spell.Uninterruptible = true;
                return new Spell(spell, 50);

            }
        }

        /// <summary>
        /// The stun spell
        /// </summary>
        public Spell MLDSSpell
        {
            get
            {

                DBSpell spell = new DBSpell();
                spell.AllowAdd = false;
                spell.CastTime = 0;
                spell.ClientEffect = 576;
                spell.Icon = 576;
                spell.Damage = 16;
                spell.Duration = 60;
                spell.Name = "The Shield of God";
                spell.Range = 0;
                spell.SpellID = 1000004;
                spell.Target = "Self";
                spell.Type = "DamageShield";
                spell.EffectGroup = 12;
                spell.Uninterruptible = true;
                return new Spell(spell, 50);

            }
        }

        /// <summary>
        /// The stun spell
        /// </summary>
        public Spell MLAddSpell
        {
            get
            {

                DBSpell spell = new DBSpell();
                spell.AllowAdd = false;
                spell.CastTime = 0;
                spell.ClientEffect = 3259;
                spell.Icon = 0;
                spell.Damage = 5;
                spell.Duration = 60;
                spell.Name = "The Power of God 3";
                spell.Range = 0;
                spell.SpellID = 1000005;
                spell.Target = "Self";
                spell.Type = "DamageAdd";
                spell.EffectGroup = 20;
                spell.Uninterruptible = true;
                return new Spell(spell, 50);

            }
        }



        /// <summary>
        /// The stun spell
        /// </summary>
        public Spell MLHaste6Spell
        {
            get
            {

                DBSpell spell = new DBSpell();
                spell.AllowAdd = false;
                spell.CastTime = 0;
                spell.ClientEffect = 1723;
                spell.Icon = 0;
                spell.Value = 17;
                spell.Duration = 20;
                spell.Name = "The Hast of God 5";
                spell.Range = 0;
                spell.SpellID = 1000008;
                spell.Target = "Self";
                spell.Type = "CombatSpeedBuff";
                spell.EffectGroup = 20;
                spell.Uninterruptible = true;
                return new Spell(spell, 50);

            }
        }

        /// <summary>
        /// The stun spell
        /// </summary>
        public Spell MLDotAoeSpell
        {
            get
            {
                DBSpell spell = new DBSpell();
                spell.AllowAdd = false;
                spell.CastTime = 0;
                spell.ClientEffect = 3475;
                spell.Icon = 3475;
                spell.Damage = this.Body.Level * 2;
                spell.Duration = 12;
                spell.Frequency = 20;
                spell.Name = "The DoT of God";
                spell.Range = 2000;
                spell.SpellID = 1000008;
                spell.Target = "Enemy";
                spell.Type = "DamageOverTime";
                spell.EffectGroup = 1292;
                spell.SpellGroup = 9192;
                spell.Radius = 1500;
                spell.Message1 = "Your skin erupts in open wounds!";
                spell.Message2 = "{0}'s skin erupts in open wounds!";
                spell.Message3 = "The destructive energy around you fades.";
                spell.Message4 = "The destructive energy around {0} fades.";
                //spell.Interruptable = 1;
                return new Spell(spell, 50);
            }
        }

        /// <summary>
        /// Returns the string representation of the ML6BossMobBrain
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString() + ", m_aggroLevel=" + m_aggroLevel.ToString() + ", m_aggroMaxRange=" + m_aggroMaxRange.ToString();
        }



        public override bool Stop()
        {
            // tolakram - when the brain stops, due to either death or no players in the vicinity, clear the aggro list
            if (base.Stop())
            {
                ClearAggroList();
                phase1 = true;
                phase2 = true;
                phase3 = true;
                phase4 = true;
                phase5 = true;
                phase6 = true;
                phase7 = true;
                phase8 = true;
                phase9 = true;
                phase10 = true;
                return true;
            }

            return false;
        }

        #region AI


        /// <summary>
        /// Check for aggro against close NPCs
        /// </summary>
        protected virtual void CheckNPCAggro()
        {
            if (Body.AttackState)
                return;

            foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)AggroRange, Body.CurrentRegion.IsDungeon ? false : true))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(Body, npc, true)) continue;
                if (m_aggroTable.ContainsKey(npc))
                    continue; // add only new NPCs
                if (!npc.IsAlive || npc.ObjectState != GameObject.eObjectState.Active)
                    continue;
                if (npc is GameTaxi)
                    continue; //do not attack horses

                if (CalculateAggroLevelToTarget(npc) > 0)
                {
                    AddToAggroList(npc, (npc.Level + 1) << 1);
                }
            }
        }

        /// <summary>
        /// Check for aggro against players
        /// </summary>
        protected virtual void CheckPlayerAggro()
        {
            //Check if we are already attacking, return if yes
            if (Body.AttackState)
                return;

            foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange, true))
            {
                if (!GameServer.ServerRules.IsAllowedToAttack(Body, player, true)) continue;
                // Don't aggro on immune players.

                if (player.EffectList.GetOfType(typeof(NecromancerShadeEffect)) != null)
                    continue;

                int aggrolevel = 0;

                if (Body.Faction != null)
                {
                    aggrolevel = Body.Faction.GetAggroToFaction(player);
                    if (aggrolevel < 0)
                        aggrolevel = 0;
                }

                if (aggrolevel <= 0 && AggroLevel <= 0)
                    return;

                if (m_aggroTable.ContainsKey(player))
                    continue; // add only new players
                if (!player.IsAlive || player.ObjectState != GameObject.eObjectState.Active || player.IsStealthed)
                    continue;
                if (player.Steed != null)
                    continue; //do not attack players on steed

                if (CalculateAggroLevelToTarget(player) > 0)
                {
                    AddToAggroList(player, player.EffectiveLevel << 1, true);
                }
            }
        }


        /// <summary>
        /// Method to launch a spell
        /// </summary>
        /// <param name="spellLevel">The spell level</param>
        /// <param name="spellLineName">The spell line</param>
        /// <param name="guard">The guard caster</param>
        public static void LaunchSpell(int spellLevel, string spellLineName, GameNPC caster)
        {
            if (caster.TargetObject == null)
                return;

            Spell castSpell = null;
            SpellLine castLine = null;

            castLine = SkillBase.GetSpellLine(spellLineName);
            List<Spell> spells = SkillBase.GetSpellList(castLine.KeyName);

            foreach (Spell spell in spells)
            {
                if (spell.Level == spellLevel)
                {
                    castSpell = spell;
                    break;
                }
            }
            if (caster.AttackState)
                caster.StopAttack();
            if (caster.IsMoving)
                caster.StopFollowing();
            caster.TurnTo(caster.TargetObject);
            caster.CastSpell(castSpell, castLine);
        }


        /// <summary>
        /// If this brain is part of a formation, it edits it's values accordingly.
        /// </summary>
        /// <param name="x">The x-coordinate to refer to and change</param>
        /// <param name="y">The x-coordinate to refer to and change</param>
        /// <param name="z">The x-coordinate to refer to and change</param>
        public virtual bool CheckFormation(ref int x, ref int y, ref int z)
        {
            return false;
        }

        /// <summary>
        /// Checks the Abilities
        /// </summary>
        public virtual void CheckAbilities()
        {
            //See CNPC
        }

        #endregion

        #region Aggro

        /// <summary>
        /// Max Aggro range in that this npc searches for enemies
        /// </summary>
        protected int m_aggroMaxRange;
        /// <summary>
        /// Aggressive Level of this npc
        /// </summary>
        protected int m_aggroLevel;
        /// <summary>
        /// List of livings that this npc has aggro on, living => aggroamount
        /// </summary>
        protected readonly Hashtable m_aggroTable = new Hashtable();

        /// <summary>
        /// The aggression table for this mob
        /// </summary>
        public Hashtable AggroTable
        {
            get { return m_aggroTable; }
        }

        /// <summary>
        /// Aggressive Level in % 0..100, 0 means not Aggressive
        /// </summary>
        public virtual int AggroLevel
        {
            get { return m_aggroLevel; }
            set { m_aggroLevel = value; }
        }

        /// <summary>
        /// Range in that this npc aggros
        /// </summary>
        public virtual int AggroRange
        {
            get { return m_aggroMaxRange; }
            set { m_aggroMaxRange = value; }
        }

        /// <summary>
        /// Checks whether living has someone on its aggrolist
        /// </summary>
        public virtual bool HasAggro
        {
            get
            {
                bool hasAggro = false;
                lock (m_aggroTable.SyncRoot)
                {
                    hasAggro = (m_aggroTable.Count > 0);
                }
                return hasAggro;
            }
        }

        /// <summary>
        /// Add aggro table of this brain to that of another living.
        /// </summary>
        /// <param name="brain">The target brain.</param>
        public void AddAggroListTo(ML6BossMobBrain brain)
        {
            // TODO: This should actually be the other way round, but access
            // to m_aggroTable is restricted and needs to be threadsafe.

            // do not modify aggro list if dead
            if (!brain.Body.IsAlive) return;

            lock (m_aggroTable.SyncRoot)
            {
                IDictionaryEnumerator dictEnum = m_aggroTable.GetEnumerator();
                while (dictEnum.MoveNext())
                    brain.AddToAggroList((GameLiving)dictEnum.Key, Body.MaxHealth);
            }
        }

        // LOS Check on natural aggro (aggrorange & aggrolevel)
        // This part is here due to los check constraints;
        // Otherwise, it should be in CheckPlayerAggro() method.
        private bool m_AggroLOS;
        public virtual bool AggroLOS
        {
            get { return m_AggroLOS; }
            set { m_AggroLOS = value; }
        }
        private void CheckAggroLOS(GamePlayer player, ushort response, ushort targetOID)
        {
            if ((response & 0x100) == 0x100)
                AggroLOS = true;
            else
                AggroLOS = false;
        }

        /// <summary>
        /// Add living to the aggrolist
        /// aggroamount can be negative to lower amount of aggro
        /// </summary>
        /// <param name="living"></param>
        /// <param name="aggroamount"></param>
        public virtual void AddToAggroList(GameLiving living, int aggroamount)
        {
            AddToAggroList(living, aggroamount, false);
        }

        public virtual void AddToAggroList(GameLiving living, int aggroamount, bool NaturalAggro)
        {
            if (m_body.IsConfused) return;

            // tolakram - duration spell effects will attempt to add to aggro after npc is dead
            if (!m_body.IsAlive) return;

            if (living == null) return;

            //Handle trigger to say sentance on first aggro.
            if (m_aggroTable.Count < 1)
                Body.FireAmbientSentence(GameNPC.eAmbientTrigger.aggroing, living);

            // Check LOS (walls, pits, etc...) before  attacking, player + pet
            // Be sure the aggrocheck is triggered by the brain on Think() method
            if (DOL.GS.ServerProperties.Properties.ALWAYS_CHECK_LOS && NaturalAggro)
            {
                GamePlayer thisLiving = null;
                if (living is GamePlayer)
                    thisLiving = (GamePlayer)living;

                if (living is GamePet)
                {
                    IControlledBrain brain = ((GamePet)living).Brain as IControlledBrain;
                    thisLiving = brain.GetPlayerOwner();
                }

                if (thisLiving != null)
                {
                    thisLiving.Out.SendCheckLOS(Body, living, new CheckLOSResponse(CheckAggroLOS));
                    if (!AggroLOS) return;
                }
            }

            // only protect if gameplayer and aggroamout > 0
            if (living is GamePlayer && aggroamount > 0)
            {
                GamePlayer player = (GamePlayer)living;

                if (player.Group != null)
                { // player is in group, add whole group to aggro list
                    lock (m_aggroTable.SyncRoot)
                    {
                        foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                        {
                            if (m_aggroTable[p] == null)
                            {
                                m_aggroTable[p] = 1L;	// add the missing group member on aggro table
                            }
                        }
                    }
                }

                //ProtectEffect protect = (ProtectEffect) player.EffectList.GetOfType(typeof(ProtectEffect));
                foreach (ProtectEffect protect in player.EffectList.GetAllOfType(typeof(ProtectEffect)))
                {
                    // if no aggro left => break
                    if (aggroamount <= 0) break;

                    //if (protect==null) continue;
                    if (protect.ProtectTarget != living) continue;
                    if (protect.ProtectSource.IsStunned) continue;
                    if (protect.ProtectSource.IsMezzed) continue;
                    if (protect.ProtectSource.IsSitting) continue;
                    if (protect.ProtectSource.ObjectState != GameObject.eObjectState.Active) continue;
                    if (!protect.ProtectSource.IsAlive) continue;
                    if (!protect.ProtectSource.InCombat) continue;

                    if (!living.IsWithinRadius(protect.ProtectSource, ProtectAbilityHandler.PROTECT_DISTANCE))
                        continue;
                    // P I: prevents 10% of aggro amount
                    // P II: prevents 20% of aggro amount
                    // P III: prevents 30% of aggro amount
                    // guessed percentages, should never be higher than or equal to 50%
                    int abilityLevel = protect.ProtectSource.GetAbilityLevel(Abilities.Protect);
                    int protectAmount = (int)((abilityLevel * 0.10) * aggroamount);

                    if (protectAmount > 0)
                    {
                        aggroamount -= protectAmount;
                        protect.ProtectSource.Out.SendMessage(LanguageMgr.GetTranslation(protect.ProtectSource.Client, "AI.Brain.ML6BossMobBrain.YouProtDist", player.GetName(0, false), Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        //player.Out.SendMessage("You are protected by " + protect.ProtectSource.GetName(0, false) + " from " + Body.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        lock (m_aggroTable.SyncRoot)
                        {
                            if (m_aggroTable[protect.ProtectSource] != null)
                            {
                                long amount = (long)m_aggroTable[protect.ProtectSource];
                                amount += protectAmount;
                                m_aggroTable[protect.ProtectSource] = amount;
                            }
                            else
                            {
                                m_aggroTable[protect.ProtectSource] = (long)protectAmount;
                            }
                        }
                    }
                }
            }

            lock (m_aggroTable.SyncRoot)
            {
                if (m_aggroTable[living] != null)
                {
                    long amount = (long)m_aggroTable[living];
                    amount += aggroamount;

                    // can't be removed this way, set to minimum
                    if (amount <= 0)
                        amount = 1L;

                    m_aggroTable[living] = amount;
                }
                else
                {
                    if (aggroamount > 0)
                    {
                        m_aggroTable[living] = (long)aggroamount;
                    }
                    else
                    {
                        m_aggroTable[living] = 1L;
                    }

                }

                if (DOL.GS.ServerProperties.Properties.ENABLE_DEBUG && (this is IControlledBrain) == false)
                {
                    foreach (GameLiving aliv in m_aggroTable.Keys)
                    {
                        Body.Yell(aliv.Name + ": " + m_aggroTable[aliv]);
                    }
                }
            }
        }

        /// <summary>
        /// Get current amount of aggro on aggrotable
        /// </summary>
        /// <param name="living"></param>
        /// <returns></returns>
        public virtual long GetAggroAmountForLiving(GameLiving living)
        {
            lock (m_aggroTable.SyncRoot)
            {
                if (m_aggroTable[living] != null)
                {
                    return (long)m_aggroTable[living];
                }
                return 0;
            }
        }

        /// <summary>
        /// Remove one living from aggro list
        /// </summary>
        /// <param name="living"></param>
        public virtual void RemoveFromAggroList(GameLiving living)
        {
            lock (m_aggroTable.SyncRoot)
            {
                m_aggroTable.Remove(living);
            }
        }

        /// <summary>
        /// Remove all livings from the aggrolist
        /// </summary>
        public virtual void ClearAggroList()
        {
            lock (m_aggroTable.SyncRoot)
            {
                m_aggroTable.Clear();
                Body.TempProperties.removeProperty(Body.Attackers);
            }
        }

        /// <summary>
        /// Makes a copy of current aggro list
        /// </summary>
        /// <returns></returns>
        public virtual Hashtable CloneAggroList()
        {
            lock (m_aggroTable.SyncRoot)
            {
                return (Hashtable)m_aggroTable.Clone();
            }
        }

        /// <summary>
        /// Selects and attacks the next target or does nothing
        /// </summary>
        protected virtual void AttackMostWanted()
        {
            if (!IsActive)
                return;

            Body.TargetObject = CalculateNextAttackTarget();
            if (Body.TargetObject != null)
            {
                if (!CheckSpells(eCheckSpellType.Offensive))
                {
                    Body.StartAttack(Body.TargetObject);
                }
            }
        }

        /// <summary>
        /// Returns the best target to attack
        /// </summary>
        /// <returns>the best target</returns>
        protected virtual GameLiving CalculateNextAttackTarget()
        {
            GameLiving maxAggroObject = null;
            lock (m_aggroTable.SyncRoot)
            {
                double maxAggro = 0;
                IDictionaryEnumerator aggros = m_aggroTable.GetEnumerator();
                List<GameLiving> removable = new List<GameLiving>();
                while (aggros.MoveNext())
                {
                    GameLiving living = (GameLiving)aggros.Key;

                    // check to make sure this target is still valid
                    if (living.IsAlive == false ||
                        living.ObjectState != GameObject.eObjectState.Active ||
                        living.IsStealthed ||
                        Body.GetDistanceTo(living, 0) > MAX_AGGRO_LIST_DISTANCE)
                    {
                        removable.Add(living);
                        continue;
                    }

                    // Don't bother about necro shade, can't attack it anyway.
                    if (living.EffectList.GetOfType(typeof(NecromancerShadeEffect)) != null)
                        continue;
                    long amount = (long)aggros.Value;
                    if (living.IsAlive
                        && amount > maxAggro
                        && living.CurrentRegion == Body.CurrentRegion
                        && living.ObjectState == GameObject.eObjectState.Active)
                    {
                        int distance = Body.GetDistanceTo(living);
                        int maxAggroDistance = (this is IControlledBrain) ? MAX_PET_AGGRO_DISTANCE : MAX_AGGRO_DISTANCE;
                        if (distance <= maxAggroDistance)
                        {
                            double aggro = amount * Math.Min(500.0 / distance, 1);
                            if (aggro > maxAggro)
                            {
                                maxAggroObject = living;
                                maxAggro = aggro;
                            }
                        }
                    }
                }

                foreach (GameLiving l in removable)
                {
                    RemoveFromAggroList(l);
                    Body.RemoveAttacker(l);
                }
            }

            if (maxAggroObject == null)
            {
                m_aggroTable.Clear();
            }
            return maxAggroObject;
        }

        /// <summary>
        /// calculate the aggro of this npc against another living
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual int CalculateAggroLevelToTarget(GameLiving target)
        {
            if (GameServer.ServerRules.IsAllowedToAttack(Body, target, true) == false)
                return 0;

            // related to the pet owner if applicable
            if (target is GamePet)
            {
                GamePlayer thisLiving = (((GamePet)target).Brain as IControlledBrain).GetPlayerOwner();
                if (thisLiving != null)
                    if (thisLiving.IsObjectGreyCon(Body))
                        return 0;
            }

            if (target.IsObjectGreyCon(Body)) return 0;	// only attack if green+ to target

            if (Body.Faction != null && target is GamePlayer)
            {
                GamePlayer player = (GamePlayer)target;
                AggroLevel = Body.Faction.GetAggroToFaction(player);
            }
            if (AggroLevel >= 100) return 100;
            return AggroLevel;
        }

        /// <summary>
        /// Receives all messages of the body
        /// </summary>
        /// <param name="e">The event received</param>
        /// <param name="sender">The event sender</param>
        /// <param name="args">The event arguments</param>
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            base.Notify(e, sender, args);

            if (!IsActive) return;

            if (sender == Body)
            {
                if (e == GameObjectEvent.TakeDamage)
                {
                    TakeDamageEventArgs eArgs = args as TakeDamageEventArgs;
                    if (eArgs == null || eArgs.DamageSource is GameLiving == false) return;

                    int aggro = eArgs.DamageAmount + eArgs.CriticalAmount;
                    if (eArgs.DamageSource is GameNPC)
                    {
                        // owner gets 25% of aggro
                        IControlledBrain brain = ((GameNPC)eArgs.DamageSource).Brain as IControlledBrain;
                        if (brain != null)
                        {
                            AddToAggroList(brain.Owner, (int)Math.Max(1, aggro * 0.25));
                            aggro = (int)Math.Max(1, aggro * 0.75);
                        }
                    }
                    AddToAggroList((GameLiving)eArgs.DamageSource, aggro);
                    return;
                }
                else if (e == GameLivingEvent.AttackedByEnemy)
                {
                    AttackedByEnemyEventArgs eArgs = args as AttackedByEnemyEventArgs;
                    if (eArgs == null) return;
                    OnAttackedByEnemy(eArgs.AttackData);
                    return;
                }
                else if (e == GameLivingEvent.Dying)
                {
                    // clean aggro table
                    ClearAggroList();
                    return;
                }
                else if (e == GameNPCEvent.FollowLostTarget) // this means we lost the target
                {
                    FollowLostTargetEventArgs eArgs = args as FollowLostTargetEventArgs;
                    if (eArgs == null) return;
                    OnFollowLostTarget(eArgs.LostTarget);
                    return;
                }
                else if (e == GameLivingEvent.CastFailed)
                {
                    CastFailedEventArgs realArgs = args as CastFailedEventArgs;
                    if (realArgs == null || realArgs.Reason == CastFailedEventArgs.Reasons.AlreadyCasting || realArgs.Reason == CastFailedEventArgs.Reasons.CrowdControlled)
                        return;
                    Body.StartAttack(Body.TargetObject);
                }
            }

            if (e == GameLivingEvent.EnemyHealed)
            {
                EnemyHealedEventArgs eArgs = args as EnemyHealedEventArgs;
                if (eArgs != null && eArgs.HealSource is GameLiving)
                {
                    // first check to see if the healer is in our aggrolist so we don't go attacking anyone who heals
                    if (m_aggroTable[(GameLiving)eArgs.HealSource] != null)
                    {
                        if (eArgs.HealSource is GamePlayer || (eArgs.HealSource is GameNPC && (((GameNPC)eArgs.HealSource).Flags & GameNPC.eFlags.PEACE) == 0))
                        {
                            AddToAggroList((GameLiving)eArgs.HealSource, eArgs.HealAmount);
                        }
                    }
                }
                return;
            }
            else if (e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs eArgs = args as EnemyKilledEventArgs;
                if (eArgs != null)
                {
                    // transfer all controlled target aggro to the owner
                    if (eArgs.Target is GameNPC)
                    {
                        IControlledBrain controlled = ((GameNPC)eArgs.Target).Brain as IControlledBrain;
                        if (controlled != null)
                        {
                            long contrAggro = GetAggroAmountForLiving(controlled.Body);
                            AddToAggroList(controlled.Owner, (int)contrAggro);
                        }
                    }

                    Body.Attackers.Remove(eArgs.Target);
                    AttackMostWanted();
                }
                return;
            }

        }

        /// <summary>
        /// Lost follow target event
        /// </summary>
        /// <param name="target"></param>
        protected virtual void OnFollowLostTarget(GameObject target)
        {
            AttackMostWanted();
            if (!Body.AttackState)
                Body.WalkToSpawn();
        }

        /// <summary>
        /// Attacked by enemy event
        /// </summary>
        /// <param name="ad"></param>
        protected virtual void OnAttackedByEnemy(AttackData ad)
        {
            if (!Body.AttackState
                && Body.IsAlive
                && Body.ObjectState == GameObject.eObjectState.Active)
            {
                if (ad.AttackResult == GameLiving.eAttackResult.Missed)
                {
                    AddToAggroList(ad.Attacker, 1);
                }

                Body.StartAttack(ad.Attacker);
                BringFriends(ad);
            }
        }

        #endregion

        #region Bring a Friend

        /// <summary>
        /// Mobs within this range will be called upon to add on a group
        /// of players inside of a dungeon.
        /// </summary>
        protected static ushort m_BAFReinforcementsRange = 1000; //2000

        /// <summary>
        /// Players within this range around the puller will be subject
        /// to attacks from adds.
        /// </summary>
        protected static ushort m_BAFTargetPlayerRange = 1500; //3000

        /// <summary>
        /// BAF range for adds close to the pulled mob.
        /// </summary>
        public virtual ushort BAFCloseRange
        {
            get { return (ushort)((AggroRange * 2) / 5); }
        }

        /// <summary>
        /// BAF range for group adds in dungeons.
        /// </summary>
        public virtual ushort BAFReinforcementsRange
        {
            get { return m_BAFReinforcementsRange; }
            set { m_BAFReinforcementsRange = (value > 0) ? (ushort)value : (ushort)0; }
        }

        /// <summary>
        /// Range for potential targets around the puller.
        /// </summary>
        public virtual ushort BAFTargetPlayerRange
        {
            get { return m_BAFTargetPlayerRange; }
            set { m_BAFTargetPlayerRange = (value > 0) ? (ushort)value : (ushort)0; }
        }

        /// <summary>
        /// Bring friends when this living is attacked. There are 2
        /// different mechanisms for BAF:
        /// 1) Any mobs of the same faction within a certain (short) range
        ///    around the pulled mob will add on the puller, anywhere.
        /// 2) In dungeons, group size is taken into account as well, the
        ///    bigger the group, the more adds will come, even if they are
        ///    not close to the pulled mob.
        /// </summary>
        /// <param name="attackData">The data associated with the puller's attack.</param>
        protected virtual void BringFriends(AttackData attackData)
        {
            // Only add on players.

            GameLiving attacker = attackData.Attacker;
            if (attacker is GamePlayer)
            {
                BringCloseFriends(attackData);
                if (attacker.CurrentRegion.IsDungeon)
                    BringReinforcements(attackData);
            }
        }

        /// <summary>
        /// Get mobs close to the pulled mob to add on the puller and his
        /// group as well.
        /// </summary>
        /// <param name="attackData">The data associated with the puller's attack.</param>
        protected virtual void BringCloseFriends(AttackData attackData)
        {
            // Have every friend within close range add on the attacker's
            // group.

            GamePlayer attacker = (GamePlayer)attackData.Attacker;

            foreach (GameNPC npc in Body.GetNPCsInRadius(BAFCloseRange))
            {
                if (npc.IsFriend(Body) && npc.IsAvailable && npc.IsAggressive)
                {
                    ML10BossMobBrain brain = (ML10BossMobBrain)npc.Brain;
                    brain.AddToAggroList(PickTarget(attacker), 1);
                    brain.AttackMostWanted();
                }
            }
        }

        /// <summary>
        /// Get mobs to add on the puller's group, their numbers depend on the
        /// group's size.
        /// </summary>
        /// <param name="attackData">The data associated with the puller's attack.</param>
        protected virtual void BringReinforcements(AttackData attackData)
        {
            // Determine how many friends to bring, as a rule of thumb, allow for
            // max 2 players dealing with 1 mob. Only players from the group the
            // original attacker is in will be taken into consideration.
            // Example: A group of 3 or 4 players will get 1 add, a group of 7 or 8
            // players will get 3 adds.

            GamePlayer attacker = (GamePlayer)attackData.Attacker;
            Group attackerGroup = attacker.Group;
            int numAttackers = (attackerGroup == null) ? 1 : attackerGroup.MemberCount;
            int maxAdds = (numAttackers + 1) / 2 - 1;
            if (maxAdds > 0)
            {
                // Bring friends, try mobs in the neighbourhood first. If there
                // aren't any, try getting some from farther away.

                int numAdds = 0;
                ushort range = 250;

                while (numAdds < maxAdds && range <= BAFReinforcementsRange)
                {
                    foreach (GameNPC npc in Body.GetNPCsInRadius(range))
                    {
                        if (numAdds >= maxAdds) break;

                        // If it's a friend, have it attack a random target in the
                        // attacker's group.

                        if (npc.IsFriend(Body) && npc.IsAggressive && npc.IsAvailable)
                        {
                            ML10BossMobBrain brain = (ML10BossMobBrain)npc.Brain;
                            brain.AddToAggroList(PickTarget(attacker), 1);
                            brain.AttackMostWanted();
                            ++numAdds;
                        }
                    }

                    // Increase the range for finding friends to join the fight.

                    range *= 2;
                }
            }
        }

        /// <summary>
        /// Pick a random target from the attacker's group that is within a certain
        /// range of the original puller.
        /// </summary>
        /// <param name="attacker">The original attacker.</param>
        /// <returns></returns>
        protected virtual GamePlayer PickTarget(GamePlayer attacker)
        {
            Group attackerGroup = attacker.Group;

            // If no group, pick the attacker himself.

            if (attackerGroup == null) return attacker;

            // Make a list of all players in the attacker's group within
            // a certain range around the puller.

            ArrayList attackersInRange = new ArrayList();

            foreach (GamePlayer player in attackerGroup.GetPlayersInTheGroup())
                if (attacker.IsWithinRadius(player, BAFTargetPlayerRange))
                    attackersInRange.Add(player);

            // Pick a random player from the list.

            return (GamePlayer)(attackersInRange[Util.Random(1, attackersInRange.Count) - 1]);
        }

        #endregion

        #region Spells

        public enum eCheckSpellType
        {
            Offensive,
            Defensive
        }

        /// <summary>
        /// Checks if any spells need casting
        /// </summary>
        /// <param name="type">Which type should we go through and check for?</param>
        /// <returns></returns>
        public virtual bool CheckSpells(eCheckSpellType type)
        {
            if (this.Body != null && this.Body.Spells != null && this.Body.Spells.Count > 0 && !Body.IsCasting)
            {
                bool casted = false;
                if (type == eCheckSpellType.Defensive)
                {
                    foreach (Spell spell in Body.Spells)
                    {
                        if (!Body.IsBeingInterrupted && Body.GetSkillDisabledDuration(spell) == 0 && CheckDefensiveSpells(spell))
                        {
                            casted = true;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (Spell spell in Body.Spells)
                    {
                        if (Body.GetSkillDisabledDuration(spell) == 0)
                        {
                            if (spell.CastTime > 0)
                            {
                                if (!Body.IsBeingInterrupted && Body.IsCasting == false && Body.GetSkillDisabledDuration(spell) == 0 && CheckOffensiveSpells(spell))
                                {
                                    casted = true;
                                    break;
                                }
                            }
                            else
                            {
                                CheckInstantSpells(spell);
                            }
                        }
                    }
                }
                if (this is IControlledBrain && !Body.AttackState)
                    ((IControlledBrain)this).Follow(((IControlledBrain)this).Owner);
                return casted;
            }
            return false;
        }

        /// <summary>
        /// Checks defensive spells.  Handles buffs, heals, etc.
        /// </summary>
        protected virtual bool CheckDefensiveSpells(Spell spell)
        {
            if (spell == null) return false;
            if (Body.GetSkillDisabledDuration(spell) > 0) return false;
            GameObject lastTarget = Body.TargetObject;
            Body.TargetObject = null;
            switch (spell.SpellType)
            {
                #region Buffs
                case "StrengthConstitutionBuff":
                case "DexterityQuicknessBuff":
                case "StrengthBuff":
                case "DexterityBuff":
                case "ConstitutionBuff":
                case "ArmorFactorBuff":
                case "ArmorAbsorptionBuff":
                case "CombatSpeedBuff":
                case "MeleeDamageBuff":
                case "AcuityBuff":
                case "HealthRegenBuff":
                case "DamageAdd":
                case "DamageShield":
                case "BodyResistBuff":
                case "ColdResistBuff":
                case "EnergyResistBuff":
                case "HeatResistBuff":
                case "MatterResistBuff":
                case "SpiritResistBuff":
                case "BodySpiritEnergyBuff":
                case "HeatColdMatterBuff":
                case "CrushSlashThrustBuff":
                case "AllMagicResistsBuff":
                case "AllMeleeResistsBuff":
                case "OffensiveProc":
                case "DefensiveProc":
                case "Bladeturn":
                case "ToHitBuff":
                    {
                        // Buff self, if not in melee, but not each and every mob
                        // at the same time, because it looks silly.
                        if (!LivingHasEffect(Body, spell) && !Body.AttackState && Util.Chance(40))
                        {
                            Body.TargetObject = Body;
                            break;
                        }
                        break;
                    }
                #endregion Buffs

                #region Disease Cure/Poison Cure/Summon
                case "CureDisease":
                    if (!Body.IsDiseased)
                        break;
                    Body.TargetObject = Body;
                    break;
                case "CurePoison":
                    if (!LivingIsPoisoned(Body))
                        break;
                    Body.TargetObject = Body;
                    break;
                case "Summon":
                    Body.TargetObject = Body;
                    break;
                case "SummonMinion":
                    //If the list is null, lets make sure it gets initialized!
                    if (Body.ControlledNpcList == null)
                        Body.InitControlledBrainArray(2);
                    else
                    {
                        //Let's check to see if the list is full - if it is, we can't cast another minion.
                        //If it isn't, let them cast.
                        IControlledBrain[] icb = Body.ControlledNpcList;
                        int numberofpets = 0;
                        for (int i = 0; i < icb.Length; i++)
                        {
                            if (icb[i] != null)
                                numberofpets++;
                        }
                        if (numberofpets >= icb.Length)
                            break;
                    }
                    Body.TargetObject = Body;
                    break;
                #endregion Disease Cure/Poison Cure/Summon

                #region Heals
                case "Heal":
                    // Chance to heal self when dropping below 30%, do NOT spam it.

                    if (Body.HealthPercent < 30 && Util.Chance(10))
                    {
                        Body.TargetObject = Body;
                        break;
                    }

                    break;
                #endregion
            }

            if (Body.TargetObject != null)
            {
                if (Body.IsMoving && spell.CastTime > 0)
                    Body.StopFollowing();

                if (Body.TargetObject != Body && spell.CastTime > 0)
                    Body.TurnTo(Body.TargetObject);

                Body.CastSpell(spell, m_mobSpellLine);

                Body.TargetObject = lastTarget;
                return true;
            }

            Body.TargetObject = lastTarget;

            return false;
        }

        /// <summary>
        /// Checks offensive spells.  Handles dds, debuffs, etc.
        /// </summary>
        protected virtual bool CheckOffensiveSpells(Spell spell)
        {
            if (spell.Target.ToLower() != "enemy" && spell.Target.ToLower() != "area" && spell.Target.ToLower() != "cone")
                return false;

            if (Body.TargetObject != null)
            {
                if (Body.IsMoving && spell.CastTime > 0)
                    if (Body.TargetObject != Body && spell.CastTime > 0)
                        Body.TurnTo(Body.TargetObject);

                Body.CastSpell(spell, m_mobSpellLine);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks Instant Spells.  Handles Taunts, shouts, stuns, etc.
        /// </summary>
        protected virtual bool CheckInstantSpells(Spell spell)
        {
            GameObject lastTarget = Body.TargetObject;
            Body.TargetObject = null;

            switch (spell.SpellType)
            {
                #region Enemy Spells
                case "DirectDamage":
                case "Lifedrain":
                case "DexterityDebuff":
                case "StrengthConstitutionDebuff":
                case "CombatSpeedDebuff":
                case "DamageOverTime":
                case "MeleeDamageDebuff":
                case "AllStatsPercentDebuff":
                case "CrushSlashThrustDebuff":
                case "EffectivenessDebuff":
                case "Disease":
                case "Stun":
                case "Mez":
                case "Taunt":
                    if (!LivingHasEffect(lastTarget as GameLiving, spell))
                    {
                        Body.TargetObject = lastTarget;
                    }
                    break;
                #endregion

                #region Combat Spells
                case "CombatHeal":
                case "DamageAdd":
                case "ArmorFactorBuff":
                case "DexterityQuicknessBuff":
                case "EnduranceRegenBuff":
                case "CombatSpeedBuff":
                case "AblativeArmor":
                case "Bladeturn":
                case "OffensiveProc":
                    if (!LivingHasEffect(Body, spell))
                    {
                        Body.TargetObject = Body;
                    }
                    break;
                #endregion
            }

            if (Body.TargetObject != null)
            {
                Body.CastSpell(spell, m_mobSpellLine);
                Body.TargetObject = lastTarget;
                return true;
            }

            Body.TargetObject = lastTarget;
            return false;
        }

        public static bool IsLivingStunned(GameLiving living)
        {
            lock (living.EffectList)
            {
                foreach (IGameEffect effect in living.EffectList)
                {
                    if (effect is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)effect).ImmunityState)
                    {
                        continue;
                    }


                    if (effect is GameSpellEffect && ((effect as GameSpellEffect).SpellHandler is StunSpellHandler || (effect as GameSpellEffect).SpellHandler is UnresistableStunSpellHandler || (effect as GameSpellEffect).SpellHandler is UnrresistableNonImunityStun))
                    {

                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HasLivingGotStunImmunity(GameLiving living)
        {
            lock (living.EffectList)
            {
                foreach (IGameEffect effect in living.EffectList)
                {
                    if (effect is GameSpellEffect && ((effect as GameSpellEffect).SpellHandler is StunSpellHandler || (effect as GameSpellEffect).SpellHandler is UnresistableStunSpellHandler || (effect as GameSpellEffect).SpellHandler is UnrresistableNonImunityStun))
                    {
                        if (effect is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect)effect).ImmunityState)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected static SpellLine m_mobSpellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);

        /// <summary>
        /// Checks if the living target has a spell effect
        /// </summary>
        /// <param name="target">The target living object</param>
        /// <param name="spell">The spell to check</param>
        /// <returns>True if the living has the effect</returns>
        protected bool LivingHasEffect(GameLiving target, Spell spell)
        {
            if (target == null)
                return true;

            if (target is GamePlayer && (target as GamePlayer).CharacterClass.ID == (int)eCharacterClass.Vampiir)
            {
                switch (spell.SpellType)
                {
                    case "StrengthConstitutionBuff":
                    case "DexterityQuicknessBuff":
                    case "StrengthBuff":
                    case "DexterityBuff":
                    case "ConstitutionBuff":
                    case "AcuityBuff":

                        return true;
                }
            }

            lock (target.EffectList)
            {
                //Check through each effect in the target's effect list
                foreach (IGameEffect effect in target.EffectList)
                {
                    //If the effect we are checking is not a gamespelleffect keep going
                    if (effect is GameSpellEffect == false)
                        continue;

                    GameSpellEffect speffect = effect as GameSpellEffect;

                    //if the effect effectgroup is the same as the checking spells effectgroup then these are considered the same
                    if (speffect.Spell.EffectGroup == spell.EffectGroup)
                        return true;

                }
            }

            //the answer is no, the effect has not been found
            return false;
        }

        protected bool LivingIsPoisoned(GameLiving target)
        {
            foreach (IGameEffect effect in target.EffectList)
            {
                //If the effect we are checking is not a gamespelleffect keep going
                if (effect is GameSpellEffect == false)
                    continue;

                GameSpellEffect speffect = effect as GameSpellEffect;

                // if this is a DOT then target is poisoned
                if (speffect.Spell.SpellType == "DamageOverTime")
                    return true;
            }

            return false;
        }


        #endregion

        #region Random Walk
        public virtual bool CanRandomWalk
        {
            get
            {
                if (!DOL.GS.ServerProperties.Properties.ALLOW_ROAM)
                    return false;
                if (Body.RoamingRange == 0)
                    return false;
                return true;
            }
        }

        public virtual IPoint3D CalcRandomWalkTarget()
        {
            int maxRoamingRadius = Body.CurrentRegion.IsDungeon ? 5 : 500;
            int minRoamingRadius = Body.CurrentRegion.IsDungeon ? 1 : 100;

            if (Body.RoamingRange > 0)
            {
                minRoamingRadius = Body.RoamingRange / 2;
                maxRoamingRadius = Body.RoamingRange;

                if (minRoamingRadius >= maxRoamingRadius)
                    minRoamingRadius = maxRoamingRadius / 3;
            }

            int roamingRadius = Util.Random(minRoamingRadius, maxRoamingRadius);

            double angle = Util.Random(0, 360) / (2 * Math.PI);
            double targetX = Body.SpawnPoint.X + Util.Random(-roamingRadius, roamingRadius);
            double targetY = Body.SpawnPoint.Y + Util.Random(-roamingRadius, roamingRadius);

            return new Point3D((int)targetX, (int)targetY, Body.SpawnPoint.Z);
        }

        #endregion
    }
}

