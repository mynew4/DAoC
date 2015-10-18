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
using DOL.Database.Attributes;

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




namespace DOL
{
    namespace Database
    {
        /// <summary>
        /// mob skill DB
        /// </summary>
        [DataTable(TableName = "MobSkill")]
        public class DBMobSkill : DataObject
        {
            private string m_mobname;
            private string m_skill;
            private string m_skill_arg;
            private string m_stage;
            private int m_chance;
            private int m_repeat;


            public DBMobSkill()
            {
                AllowAdd = false;
            }

            /// <summary>
            /// the index / mobname
            /// </summary>
            [DataElement(AllowDbNull = false, Index = true)]
            public string Mob_Name
            {
                get
                {
                    return m_mobname;
                }
                set
                {
                    Dirty = true;
                    m_mobname = value;
                }
            }

            /// <summary>
            /// the name of the skill
            /// </summary>
            [DataElement(AllowDbNull = false)]
            public string Skill
            {
                get
                {
                    return m_skill;
                }
                set
                {
                    Dirty = true;
                    m_skill = value;
                }
            }

            /// <summary>
            /// the stage of mob health
            /// </summary>
            [DataElement(AllowDbNull = false)]
            public string Stage
            {
                get
                {
                    return m_stage;
                }
                set
                {
                    Dirty = true;
                    m_stage = value;
                }
            }

            /// <summary>
            /// Skill Argument (int)
            /// </summary>
            [DataElement(AllowDbNull = false)]
            public string Skill_Arg
            {
                get
                {
                    return m_skill_arg;
                }
                set
                {
                    Dirty = true;
                    m_skill_arg = value;
                }
            }

            /// <summary>
            /// Chance of skill casting (int)
            /// </summary>
            [DataElement(AllowDbNull = false)]
            public int Chance
            {
                get
                {
                    return m_chance;
                }
                set
                {
                    Dirty = true;
                    m_chance = value;
                }
            }

            /// <summary>
            /// Repeat or not (1:repeat)
            /// </summary>
            [DataElement(AllowDbNull = false)]
            public int Repeat
            {
                get
                {
                    return m_repeat;
                }
                set
                {
                    Dirty = true;
                    m_repeat = value;
                }
            }


        }
    }
}

namespace DOL
{
    namespace Database
    {
        /// <summary>
        /// mob skill DB
        /// </summary>
        [DataTable(TableName = "MobStyle")]
        public class DBMobStyle : DataObject
        {
            private string m_mobname;
            private string m_skill;
            private string m_stage;
            private int m_chance;


            public DBMobStyle()
            {
                AllowAdd = false;
            }

            /// <summary>
            /// the index / mobname
            /// </summary>
            [DataElement(AllowDbNull = false, Index = true)]
            public string Mob_Name
            {
                get
                {
                    return m_mobname;
                }
                set
                {
                    Dirty = true;
                    m_mobname = value;
                }
            }

            /// <summary>
            /// the name of the skill
            /// </summary>
            [DataElement(AllowDbNull = false)]
            public string Skill
            {
                get
                {
                    return m_skill;
                }
                set
                {
                    Dirty = true;
                    m_skill = value;
                }
            }

            /// <summary>
            /// the stage of mob health
            /// </summary>
            [DataElement(AllowDbNull = false)]
            public string Stage
            {
                get
                {
                    return m_stage;
                }
                set
                {
                    Dirty = true;
                    m_stage = value;
                }
            }


            /// <summary>
            /// Chance of skill casting (int)
            /// </summary>
            [DataElement(AllowDbNull = false)]
            public int Chance
            {
                get
                {
                    return m_chance;
                }
                set
                {
                    Dirty = true;
                    m_chance = value;
                }
            }


        }
    }
}

/* ========================================================================================== CUSTOM MOB ========================================================================================== */

/* /mob create DOL.GS.WhriaMob
	*/
namespace DOL.GS
{
    /// <summary>
    /// New Custom Mob 
    /// </summary>
    public class WhriaMob : GameNPC 
    {
        public double dmgmulti = 1;
        public int iStage = -1;

        WhriaMobBrain popbrain = null; 
        WhriaMob MOB1 = null; 

        public IList<DBMobSkill> MobSkills;
        public IList<DBMobStyle> MobStyles;

        public override bool AddToWorld()
        {
            if (base.AddToWorld())
            {
                string stQuery = "(`Mob_Name` = '" + GameServer.Database.Escape(this.Name) + "')";
                stQuery = stQuery + " OR (`Mob_Name` = 'level" + Math.Truncate((double)(this.Level / 10)) + "0')";
                stQuery = stQuery + " OR (`Mob_Name` = '" + GameServer.Database.Escape(this.GuildName) + "')";
                string stBodyType;
                switch (this.BodyType)
                {
                    case 1: stBodyType = "Animal"; break;
                    case 2: stBodyType = "Demon"; break;
                    case 3: stBodyType = "Dragon"; break;
                    case 4: stBodyType = "Elemental"; break;
                    case 5: stBodyType = "Giant"; break;
                    case 6: stBodyType = "Humanoid"; break;
                    case 7: stBodyType = "Insect"; break;
                    case 8: stBodyType = "Magical"; break;
                    case 9: stBodyType = "Reptile"; break;
                    case 10: stBodyType = "Plant"; break;
                    case 11: stBodyType = "Undead"; break;
                    default: stBodyType = ""; break;
                }
                if (stBodyType!="")
                    stQuery = stQuery + " OR (`Mob_Name` LIKE '" + stBodyType + "')";
                stQuery = stQuery + " OR (`Mob_Name` LIKE '" + GameServer.Database.Escape(this.CurrentRegion.Name) + "')";

                MobSkills = GameServer.Database.SelectObjects<DBMobSkill>(stQuery);
                MobStyles = GameServer.Database.SelectObjects<DBMobStyle>(stQuery);



                if ((this.Brain is WhriaMobBrain) == false)
                {
                    if (this.Brain is StandardMobBrain)
                    {
                        m_ownBrain = new WhriaMobBrain { AggroLevel = ((StandardMobBrain)Brain).AggroLevel, AggroRange = ((StandardMobBrain)Brain).AggroRange };
                        SetOwnBrain(m_ownBrain);
                    }
                    else
                    {
                        SetOwnBrain(new WhriaMobBrain());
                    }
                }

                return true;
            }
            return false;
        }

        protected override Style GetStyleToUse()
        {

            if (this.TargetObject == null)
                return base.GetStyleToUse();


            if (this.TargetObject is GameLiving) 
            {
                GameLiving living = this.TargetObject as GameLiving;

                IList styles = new ArrayList(1);

                foreach (DBMobStyle mobskill in MobStyles)
                {
                    if (!Util.Chance(mobskill.Chance)) continue;

                    if (mobskill.Stage != "")
                    {
                        bool bRightStage = false;
                        foreach (string strStage in Util.SplitCSV(mobskill.Stage))
                        {
                            int iMobStage;
                            if (!int.TryParse(strStage, out iMobStage)) continue;

                            if (iMobStage == (iStage + 1)) { bRightStage = true; break; }
                        }
                        if (!bRightStage) continue;
                    }

                    string[] skill_list = mobskill.Skill.Split(';');
                    foreach (string skill_ in skill_list)
                    {
                        string[] skill_ids=skill_.Split('|');

                        int iStyleID;int iClassID;

                        if (skill_ids.Length != 2) continue;
                        if (!int.TryParse(skill_ids[0], out iStyleID)) continue;
                        if (!int.TryParse(skill_ids[1], out iClassID)) continue;

                        
                        Style style = SkillBase.GetStyleByID(iStyleID,iClassID);
                        if (style != null)
                            styles.Add(style);
                    }
                }

                if (styles.Count>0)
                    return (Style)styles[Util.Random(styles.Count - 1)];

            }

            return base.GetStyleToUse();
        }
        
        public WhriaMob()
            : base()
        {
        }

        public void POPSLAVE(byte iLevel,string stName,ushort iModel) 
        {
            MOB1 = new WhriaMob(); 
            Random r = new Random();
            MOB1.X = X + r.Next(-100,100);
            MOB1.Y = Y + r.Next(-100, 100);
            MOB1.Z = Z + r.Next(-100, 100);
            MOB1.CurrentRegion = CurrentRegion;
            MOB1.Heading = 3340;
            MOB1.Level = iLevel;
            MOB1.Realm = 0;
            if (stName == "")
                MOB1.Name = "Slave";
            else
                MOB1.Name = stName;
            MOB1.Model = iModel;
            MOB1.Size = 50;
            MOB1.CurrentSpeed = 0;
            MOB1.MaxSpeedBase = 300;
            if (MOB1.Name.Contains("fast"))
                MOB1.MaxSpeedBase = 500;
//          MOB1.SetControlledBrain((IControlledBrain)(this.Brain));
            MOB1.GuildName = "";
            MOB1.RespawnInterval = -1;
            popbrain = new WhriaMobBrain();
            popbrain.isSlave = true;
            popbrain.AggroLevel = 100;
            popbrain.AggroRange = 3500;
            MOB1.SetOwnBrain(popbrain);
            MOB1.AddToWorld();
        }
       
      

        public override double AttackDamage(InventoryItem weapon)
        {
            if (this.Level >= 60) 
                return base.AttackDamage(weapon) * (dmgmulti * 1.2);
            else if (this.Level >= 50) 
                return base.AttackDamage(weapon) * (dmgmulti * 1.1);
            else
                return (base.AttackDamage(weapon) * dmgmulti); 
        }
       

    }
}







namespace DOL.AI.Brain
{
    /// <summary>
    /// Whria brain from standard mobbrain
    /// </summary>
    public class WhriaMobBrain : StandardMobBrain
    {
        public bool[] phase = new bool[] { true, true, true, true, true, true, true, true, true, true, true };
        public bool isSlave = false;
        public bool isLinked = false;
        public bool isFollowMaster = false;

        /// <summary>
        /// Constructs a new WhriaMobBrain
        /// </summary>
        public WhriaMobBrain()
            : base()
        {
        }

        /// <summary>
        /// Do the mob AI
        /// </summary>

        private void CastRandomA()
        {
            if (!Body.IsStunned)
            {
                int i = Util.Random(2);
                if (i==0)
                    Body.CastSpell(MLAoEDisease, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                if (i == 1)
                    Body.CastSpell(MLStunSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                if (i == 2)
                    Body.CastSpell(MLDotAoeSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
//                if (i == 3)                     Body.CastSpell(MLDSSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
//                if (i == 4)                     Body.CastSpell(MLAddSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
//                if (i == 5)                     Body.CastSpell(MLHaste1Spell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
            }
        }
        public static bool IsFamaily(GameNPC sourceNpc, GameNPC targetNpc)
        {
            if (null == sourceNpc || null == targetNpc)
                return false;

            const int iMobModelGap = 5;

            //렐름, 모델번호차이, 이름포함 유무로 패밀리인지 체크한다
            if (sourceNpc.Realm == targetNpc.Realm && Util.IsNearValue(sourceNpc.Model, targetNpc.Model, iMobModelGap) || sourceNpc.Name.Contains(targetNpc.Name) || targetNpc.Name.Contains(sourceNpc.Name))
                return true;

            return false;
        }

        private int BringCloseFrinedsToTarget(GameLiving target,int iMaxBringNumber)
        {
            int iBringNumber=0;
            foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)(m_aggroMaxRange*1.5)))
            {
                if (false == npc.IsAggressive)
                    continue;

                if ((npc.Brain is StandardMobBrain) == false) continue;

                if (true == IsFamaily(Body, npc))
                {
                    StandardMobBrain brain = (StandardMobBrain)npc.Brain;
                    if (target is GamePlayer)
                    {
                        brain.AddToAggroList(PickTarget((GamePlayer)target), 1);
                    }
                    else
                    {
                        brain.AddToAggroList(target, 1);
                    }
                    npc.StartAttack(target);
                    iBringNumber++;

                    if (npc.Brain is WhriaMobBrain)
                        ((WhriaMobBrain)npc.Brain).isLinked = true;

                    if (iBringNumber >= iMaxBringNumber) return iBringNumber; 
                }
            }
            return iBringNumber;
        }

        public override void Think()
        {
            // MOVE back

            Body.TargetObject = CalculateNextAttackTarget();
            GameObject old_target = Body.TargetObject;

            if ((!Body.AttackState && !Body.InCombat) || !(Body is WhriaMob) || !(Body.TargetObject is GameLiving))
            {
                base.Think();
                return;
            }

            if (isSlave == true)
            {
                int players = 0;

                foreach (GamePlayer player in Body.GetPlayersInRadius(2000))
                {
                    players++;
                }

                if (players == 0)
                {
                    Body.Die(this.Body);
                    return;
                }
            }

            WhriaMob MOB = Body as WhriaMob; 

            if (Body.TargetObject != null)
            {
                float angle = Body.TargetObject.GetAngle(Body);

                if (angle < 150 || angle > 210) // not BACK 
                {
                    if (IsLivingStunned((GameLiving)Body.TargetObject)) // if living is stunned !! (ONly does it if the living is actually stunned!))
                    {
                        //move to the back side
                        Point2D positionalPoint;
                        positionalPoint = Body.TargetObject.GetPointFromHeading((ushort)((Body.TargetObject.Heading + 2048) & 0xFFF), 275);
                        Body.WalkTo(positionalPoint.X, positionalPoint.Y, Body.TargetObject.Z, 1250);
                    }
                }
            }

            if (isFollowMaster && Body.ControlledBrain != null)
            {
                Point3D point_=null;
                point_.X=Body.ControlledBrain.Body.X;
                point_.Y=Body.ControlledBrain.Body.Y;
                point_.Z=Body.ControlledBrain.Body.Z;
                
                if (Body.GetDistanceTo(point_)>1000)
                {
                    Body.MoveTo(Body.ControlledBrain.Body.CurrentRegionID, point_.X, point_.Y, point_.Z, 0);
                }
                else 
                {
                    Random r = new Random();
                    Body.WalkTo(Body.ControlledBrain.Body.X + r.Next(-100, 100), Body.ControlledBrain.Body.Y + r.Next(-100, 100), Body.ControlledBrain.Body.Z, Body.MaxSpeed);
                }
            }

            MOB.iStage = (int)(Math.Truncate((double)(100 - MOB.HealthPercent - 1) / 10));
            if (MOB.iStage < 0) MOB.iStage = 0;
            if (MOB.iStage > 10) MOB.iStage = 10;

            if (MOB.HealthPercent == 100)
            {
                MOB.iStage = -1;
                MOB.dmgmulti = 1;

                phase = new bool[] { true, true, true, true, true, true, true, true, true, true, true };
            }

            foreach (DBMobSkill mobskill in MOB.MobSkills)
            {
                if (!Util.Chance(mobskill.Chance)) continue;

                if (mobskill.Stage != "")
                {
                    bool bRightStage = false;
                    foreach (string strStage in Util.SplitCSV(mobskill.Stage))
                    {
                        int iMobStage;
                        if (!int.TryParse(strStage, out iMobStage)) continue;

                        if (iMobStage == (MOB.iStage + 1)) { bRightStage = true; break; }
                    }
                    if (!bRightStage) continue;
                }

                if (MOB.iStage != -1)
                {
                    if (phase[MOB.iStage] == false && mobskill.Repeat == 0) continue;
                }
                if (MOB.iStage == -1 && mobskill.Repeat == 0) continue;


                if (!Body.IsStunned && !Body.IsCasting && Body != null)
                {

                    // out
                    if (mobskill.Skill_Arg=="out") 
                    {
                      foreach (GamePlayer player in Body.GetPlayersInRadius(3000))
                        {
                            if (player != null)
                            {
                                if (Body.GetDistanceTo(player) > 500 && player.IsAlive)
                                {
                                    Body.TargetObject = player;
                                    break;
                                }
                            }
                        }

                      if (!(Body.TargetObject is GameLiving) || Body.TargetObject == null) continue;
                      if (Body.GetDistanceTo(Body.TargetObject) <= 500) continue;
                    }
                    // in
                    if (mobskill.Skill_Arg == "in")
                    {
                        foreach (GamePlayer player in Body.GetPlayersInRadius(3000))
                        {
                            if (player != null)
                            {
                                if (Body.GetDistanceTo(player) < 500 && player.IsAlive)
                                {
                                    Body.TargetObject = player;
                                    break;
                                }
                            }
                        }
                        if (!(Body.TargetObject is GameLiving) || Body.TargetObject==null) continue;
                        if (Body.GetDistanceTo(Body.TargetObject) >= 500) continue;
                    }

                    if (Body.TargetObject is GameLiving)
                    {
                        if (mobskill.Skill_Arg == "runagate" && Body.GetDistanceTo(Body.TargetObject) < 500)
                            continue;
                    }


                    string[] skill_list = mobskill.Skill.Split(';');
                    foreach (string skill_ in skill_list)
                    {
                        switch (skill_)
                        {
                            case "aoestun":
                                Body.CastSpell(MLStunSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                                break;
                            case "aoedisease":
                                Body.CastSpell(MLAoEDisease, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                                break;
                            case "aoedot":
                                Body.CastSpell(MLDotAoeSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                                break;
                            case "dmgshield":
                                Body.CastSpell(MLDSSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                                break;
                            case "dmgadd":
                                Body.CastSpell(MLAddSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                                break;
                            case "haste":
                                Body.CastSpell(MLHaste1Spell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                                break;
                            case "link":
                            case "help":
                                if (skill_ == "link" && isLinked == true) break;
                                if (skill_ == "help")
                                    Body.Yell("Help my darkness !!! ");
                                int iBringnumber = 1;
                                if (!int.TryParse(mobskill.Skill_Arg, out iBringnumber)) iBringnumber = 1;
                                BringCloseFrinedsToTarget((GameLiving)(MOB.TargetObject), iBringnumber);
                                break;
                            case "strong":
                                if (!double.TryParse(mobskill.Skill_Arg, out MOB.dmgmulti)) MOB.dmgmulti = 2;
                                Body.Yell("I will be stronger !!! [" + MOB.dmgmulti + "]");
                                Body.Yell("I will be stronger !!! [" + MOB.dmgmulti + "]");
                                Body.Yell("I will be stronger !!! [" + MOB.dmgmulti + "]");
                                break;
                            case "pop_runagate":
                            case "pop":

                                if (skill_ == "pop_runagate" && Body.GetDistanceTo(Body.TargetObject) < 500)
                                    break;

                                // number , level , name , model 
                                int iNumber = 1;
                                string stName = "Slave";
                                int iModel = 1733;
                                int ilevel = 1;
                                if (MOB.Level > 10) ilevel = MOB.Level - 10;
                                if (MOB.Level <= 10) ilevel = MOB.Level;

                                string[] args = mobskill.Skill_Arg.Split('|');

                                if (mobskill.Skill_Arg == "")
                                {
                                    MOB.POPSLAVE((byte)(ilevel), stName, (ushort)iModel);
                                    break;
                                }

                                if (args.Length >= 1)
                                {
                                    if (!int.TryParse(args[0], out iNumber)) iNumber = 1;
                                }
                                if (args.Length >= 2)
                                {
                                    if (!int.TryParse(args[1], out ilevel))
                                    {
                                        if (MOB.Level > 10) ilevel = MOB.Level - 10;
                                        if (MOB.Level <= 10) ilevel = MOB.Level;
                                    }
                                }
                                if (args.Length >= 3)
                                {
                                    if (args[2].Trim() != "") stName = args[2];
                                }
                                if (args.Length >= 4)
                                {
                                    if (!int.TryParse(args[3], out iModel)) iModel = 1733;
                                }


                                for (int i = 0; i < iNumber; i++)
                                {
                                    MOB.POPSLAVE((byte)(ilevel), stName, (ushort)iModel);
                                }

                                break;
                            case "random":
                                CastRandomA();
                                break;

                            default:
                                int iSpellID;
                                if (int.TryParse(mobskill.Skill, out iSpellID))
                                {
                                    Spell spell = SkillBase.GetSpellByID(Convert.ToInt32(iSpellID));
                                    if (spell != null)
                                    {

                                        if (mobskill.Skill_Arg == "runagate")
                                        {
                                            if (Body.IsMoving && spell.CastTime > 0)
                                                Body.StopFollowing();
                                        }

                                        if (Body.TargetObject != Body && spell.CastTime > 0)
                                            Body.TurnTo(Body.TargetObject);

                                        if (!Body.IsCasting || spell.CastTime == 0)
                                        {
                                            if (spell.SpellType.ToLower()=="heal")
                                            {
                                                Body.TargetObject = null;
                                                if (spell.Target.ToLower() == "self")
                                                {
                                                    if (Body.HealthPercent < 75)
                                                    {
                                                        Body.TargetObject = Body;
                                                    }
                                                }
                                                else if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null
                                                    && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && Body.ControlledBrain.Body.HealthPercent < 60 && spell.Target.ToLower() != "self")
                                                {
                                                    Body.TargetObject = Body.ControlledBrain.Body;
                                                }
                                                else 
                                                {
                                                    foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)spell.Range))
                                                    {
                                                        if (npc != null)
                                                        {
                                                            if (npc.Realm==Body.Realm && npc.Health<75)
                                                            {
                                                                Body.TargetObject = npc; break;
                                                            }
                                                        }
                                                    }
                                                }

                                                if (Body.TargetObject != null)
                                                    Body.CastSpell(spell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                                            }
                                            else
                                                Body.CastSpell(spell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

            }

            if (MOB.iStage != -1)
            {
                if (phase[MOB.iStage] == true) phase[MOB.iStage] = false;
            }
            Body.TargetObject = old_target;

            base.Think();

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
                spell.Damage = 55;
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

        public override bool Stop()
        {
            // tolakram - when the brain stops, due to either death or no players in the vicinity, clear the aggro list
            if (base.Stop())
            {
                ClearAggroList();
                phase = new bool[] { true, true, true, true, true, true, true, true, true, true, true };
                if (Body is WhriaMob)
                    ((WhriaMob)Body).dmgmulti = 1;
                return true;
            }

            return false;
        }

        private static bool IsLivingStunned(GameLiving living)
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
       
    }
}

