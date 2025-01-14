﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct Range
{
    internal int Min;
    internal int Max;

    public Range(int min, int max)
    {
        Min = min;
        Max = max;
    }
    public Range(int max)
    {
        Min = 0;
        Max = max;
    }
}

public enum AbilityTargets
{
    Enemy,
    Ally,
    SurrenderedAlly,
    Tile,
}

static class SpellList
{
    static internal readonly DamageSpell Fireball;
    static internal readonly DamageSpell PowerBolt;
    static internal readonly DamageSpell LightningBolt;
    static internal readonly StatusSpell Shield;
    static internal readonly StatusSpell Mending;
    static internal readonly StatusSpell Speed;
    static internal readonly StatusSpell Valor;
    static internal readonly StatusSpell Predation;
    static internal readonly StatusSpell Charm;

    static internal readonly DamageSpell IceBlast;
    static internal readonly DamageSpell Pyre;
    //static internal readonly Spell Warp;
    //static internal readonly DamageSpell MagicWall;
    static internal readonly StatusSpell Poison;

    //Quicksand
    static internal readonly StatusSpell PreysCurse;
    static internal readonly StatusSpell Enlarge;
    static internal readonly Spell Maw;
    static internal readonly Spell Summon;

    static internal readonly StatusSpell Diminishment;
    //Raze
    static internal readonly Spell GateMaw;
    static internal readonly Spell Resurrection;

    static internal readonly StatusSpell AlraunePuff;
    static internal readonly StatusSpell Web;
    static internal readonly StatusSpell GlueBomb;
    static internal readonly StatusSpell Petrify;

    static internal readonly DamageSpell ViperPoisonDamage;
    static internal readonly StatusSpell ViperPoisonStatus;

    static internal Dictionary<SpellTypes, Spell> SpellDict;

    static SpellList()
    {
        SpellDict = new Dictionary<SpellTypes, Spell>();
        Fireball = new DamageSpell()
        {
            Name = "Fireball",
            Id = "fireball",
            SpellType = SpellTypes.Fireball,
            Description = "Deals damage in an area",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy, AbilityTargets.Tile },
            Range = new Range(6),
            AreaOfEffect = 1,
            Tier = 1,
            Resistable = true,
            DamageType = DamageTypes.Fire,
            Damage = (a, t) => 5 + a.Unit.GetStat(Stat.Mind) / 10,
            OnExecute = (a, t) =>
            {
                a.CastOffensiveSpell(Fireball, t);
                TacticalGraphicalEffects.CreateFireBall(a.Position, t.Position, t);
            },
            OnExecuteTile = (a, l) =>
            {
                a.CastOffensiveSpell(Fireball, null, l);
                TacticalGraphicalEffects.CreateFireBall(a.Position, l, null);
            },
        };
        SpellDict[SpellTypes.Fireball] = Fireball;

        PowerBolt = new DamageSpell()
        {
            Name = "Power Bolt",
            Id = "power-bolt",
            SpellType = SpellTypes.PowerBolt,
            Description = "Deals damage to a single target, not resistable",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy },
            Range = new Range(6),
            AreaOfEffect = 0,
            Tier = 1,
            Resistable = false,
            Damage = (a, t) => 1 + a.Unit.GetStat(Stat.Mind) / 5,
            OnExecute = (a, t) =>
            {
                a.CastOffensiveSpell(PowerBolt, t);
                TacticalGraphicalEffects.CreateGenericMagic(a.Position, t.Position, t);
            },
        };
        SpellDict[SpellTypes.PowerBolt] = PowerBolt;

        LightningBolt = new DamageSpell()
        {
            Name = "Lightning Bolt",
            Id = "lightning-bolt",
            SpellType = SpellTypes.LightningBolt,
            Description = "Deals damage to a single target, long range",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy },
            Range = new Range(40),
            AreaOfEffect = 0,
            Tier = 1,
            Resistable = true,
            Damage = (a, t) => 5 + a.Unit.GetStat(Stat.Mind) / 10,
            OnExecute = (a, t) =>
            {
                a.CastOffensiveSpell(LightningBolt, t);
                TacticalGraphicalEffects.CreateGenericMagic(a.Position, t.Position, t);
            },
        };
        SpellDict[SpellTypes.LightningBolt] = LightningBolt;

        Shield = new StatusSpell()
        {
            Name = "Shield",
            Id = "shield",
            SpellType = SpellTypes.Shield,
            Description = "Causes target to take reduced damage",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Ally },
            Range = new Range(6),
            Duration = (a, t) => 2 + a.Unit.GetStat(Stat.Mind) / 10,
            Effect = (a, t) => .25f,
            Type = StatusEffectType.Shielded,
            Tier = 1,
            Resistable = false,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(Shield, t);
                TacticalGraphicalEffects.CreateGenericMagic(a.Position, t.Position, t, TacticalGraphicalEffects.SpellEffectIcon.Buff);
            },
        };
        SpellDict[SpellTypes.Shield] = Shield;

        Mending = new StatusSpell()
        {
            Name = "Mending",
            Id = "mending",
            SpellType = SpellTypes.Mending,
            Description = "Heals target over time",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Ally },
            Range = new Range(4),
            Duration = (a, t) => 1 + a.Unit.GetStat(Stat.Mind) / 10,
            Effect = (a, t) => a.Unit.GetStat(Stat.Mind),
            Type = StatusEffectType.Mending,
            Tier = 1,
            Resistable = false,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(Mending, t);
                TacticalGraphicalEffects.CreateGenericMagic(a.Position, t.Position, t, TacticalGraphicalEffects.SpellEffectIcon.Heal);
            },
        };
        SpellDict[SpellTypes.Mending] = Mending;

        Speed = new StatusSpell()
        {
            Name = "Speed",
            Id = "speed",
            SpellType = SpellTypes.Speed,
            Description = "Makes target faster",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Ally },
            Range = new Range(4),
            Duration = (a, t) => 3 + a.Unit.GetStat(Stat.Mind) / 10,
            Effect = (a, t) => .3f + a.Unit.GetStat(Stat.Mind) / 1000,
            Type = StatusEffectType.Fast,
            Tier = 1,
            Resistable = false,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(Speed, t);
                TacticalGraphicalEffects.CreateGenericMagic(a.Position, t.Position, t, TacticalGraphicalEffects.SpellEffectIcon.Buff);
            },
        };
        SpellDict[SpellTypes.Speed] = Speed;

        Valor = new StatusSpell()
        {
            Name = "Valor",
            Id = "valor",
            SpellType = SpellTypes.Valor,
            Description = "Target does more damage",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Ally },
            Range = new Range(6),
            Duration = (a, t) => 2 + a.Unit.GetStat(Stat.Mind) / 10,
            Effect = (a, t) => .25f,
            Type = StatusEffectType.Valor,
            Tier = 1,
            Resistable = false,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(Valor, t);
                TacticalGraphicalEffects.CreateGenericMagic(a.Position, t.Position, t, TacticalGraphicalEffects.SpellEffectIcon.Buff);
            },
        };
        SpellDict[SpellTypes.Valor] = Valor;

        Predation = new StatusSpell()
        {
            Name = "Predation",
            Id = "predation",
            SpellType = SpellTypes.Predation,
            Description = "Target is better at vore",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Ally },
            Range = new Range(6),
            Duration = (a, t) => 2 + a.Unit.GetStat(Stat.Mind) / 10,
            Effect = (a, t) => a.Unit.GetStat(Stat.Mind) / 4,
            Type = StatusEffectType.Predation,
            Tier = 1,
            Resistable = false,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(Predation, t);
                TacticalGraphicalEffects.CreateGenericMagic(a.Position, t.Position, t, TacticalGraphicalEffects.SpellEffectIcon.Buff);
            },
        };
        SpellDict[SpellTypes.Predation] = Predation;

        IceBlast = new DamageSpell()
        {
            Name = "Ice Blast",
            Id = "ice-blast",
            SpellType = SpellTypes.IceBlast,
            Description = "Area of effect cold damage and lays down ice",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy, AbilityTargets.Tile },
            Range = new Range(6),
            AreaOfEffect = 1,
            Tier = 2,
            Resistable = true,
            Damage = (a, t) => 5 + a.Unit.GetStat(Stat.Mind) / 10,
            OnExecute = (a, t) =>
            {
                a.CastOffensiveSpell(IceBlast, t);
                TacticalUtilities.CreateEffect(t.Position, TileEffectType.IcePatch, 1, 2, 10 + a.Unit.GetStat(Stat.Mind) / 5);
                TacticalGraphicalEffects.CreateIceBlast(t.Position);
            },
            OnExecuteTile = (a, l) =>
            {
                a.CastOffensiveSpell(IceBlast, null, l);
                TacticalUtilities.CreateEffect(l, TileEffectType.IcePatch, 1, 2, 10 + a.Unit.GetStat(Stat.Mind) / 5);
                TacticalGraphicalEffects.CreateIceBlast(l);
            },
        };
        SpellDict[SpellTypes.IceBlast] = IceBlast;

        Pyre = new DamageSpell()
        {
            Name = "Pyre",
            Id = "pyre",
            SpellType = SpellTypes.Pyre,
            Description = "Deals damage in an area, sets the ground on fire for a few turns",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy, AbilityTargets.Tile },
            Range = new Range(6),
            AreaOfEffect = 1,
            Tier = 2,
            Resistable = true,
            DamageType = DamageTypes.Fire,
            Damage = (a, t) => 7 + a.Unit.GetStat(Stat.Mind) / 10,
            OnExecute = (a, t) =>
            {
                a.CastOffensiveSpell(Pyre, t);
                TacticalUtilities.CreateEffect(t.Position, TileEffectType.Fire, 1, 1 + a.Unit.GetStat(Stat.Mind) / 30, 4);
            },
            OnExecuteTile = (a, l) =>
            {
                a.CastOffensiveSpell(Pyre, null, l);
                TacticalUtilities.CreateEffect(l, TileEffectType.Fire, 1, 1 + a.Unit.GetStat(Stat.Mind) / 30, 4);
            },
        };
        SpellDict[SpellTypes.Pyre] = Pyre;

        //Warp = new Spell() //Implemented this and forgot it was supposed to be target and then location, only the caster makes it highly situational
        //{
        //    Name = "Warp",
        //    SpellType = SpellTypes.Warp,
        //    Description = "Teleports the Unit to the target tile",
        //    AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Tile },
        //    Range = new Range(16),
        //    Tier = 2,
        //    Resistable = false,
        //    OnExecuteTile = (a, loc) =>
        //    {
        //        if (TacticalUtilities.OpenTile(loc, null) && a.CastSpell(Warp, null))
        //        {
        //            a.Position.x = loc.x;
        //            a.Position.y = loc.y;
        //            TacticalUtilities.UpdateActorLocations();
        //        }               
        //    },
        //};
        //SpellDict[SpellTypes.Warp] = Warp;

        Poison = new StatusSpell()
        {
            Name = "Poison",
            Id = "poison",
            SpellType = SpellTypes.Poison,
            Description = "Deals damage over time, can not kill targets",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy },
            Range = new Range(8),
            Duration = (a, t) => 4 + a.Unit.GetStat(Stat.Mind) / 5,
            Effect = (a, t) => 1 + a.Unit.GetStat(Stat.Mind) / 20,
            Type = StatusEffectType.Poisoned,
            Tier = 2,
            Resistable = true,
            OnExecute = (a, t) =>
            {
                if (a.CastStatusSpell(Poison, t))
                    TacticalGraphicalEffects.CreateGenericMagic(a.Position, t.Position, t, TacticalGraphicalEffects.SpellEffectIcon.Poison);
            },
        };
        SpellDict[SpellTypes.Poison] = Poison;

        PreysCurse = new StatusSpell()
        {
            Name = "Prey's Curse",
            Id = "preys-curse",
            SpellType = SpellTypes.PreysCurse,
            Description = "Makes target into a semi-willing prey, increasing vore chance on them (and changing some flavor text)",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy },
            Range = new Range(6),
            Duration = (a, t) => 2 + a.Unit.GetStat(Stat.Mind) / 10,
            Effect = (a, t) => 0,
            Type = StatusEffectType.WillingPrey,
            Tier = 3,
            Resistable = true,
            OnExecute = (a, t) =>
            {
                if (a.CastStatusSpell(PreysCurse, t))
                    TacticalGraphicalEffects.CreateGenericMagic(a.Position, t.Position, t, TacticalGraphicalEffects.SpellEffectIcon.Debuff);
            },
        };
        SpellDict[SpellTypes.PreysCurse] = PreysCurse;

        Maw = new Spell()
        {
            Name = "Maw",
            Id = "maw",
            SpellType = SpellTypes.Maw,
            Description = "Attempts to teleport the target into caster's belly, adding caster's Power to Voracity.",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy },
            Range = new Range(4),
            Tier = 3,
            Resistable = true,
            OnExecute = (a, t) => a.CastMaw(Maw, t),
        };
        SpellDict[SpellTypes.Maw] = Maw;

        Charm = new StatusSpell()
        {
            Name = "Charm",
            Id = "charm",
            SpellType = SpellTypes.Charm,
            Description = "Has a chance to temporarily make an enemy fight for the caster.",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy },
            Range = new Range(6),
            Duration = (a, t) => 2 + a.Unit.GetStat(Stat.Mind) / 10,
            Effect = (a, t) => a.Unit.hiddenFixedSide ? a.Unit.Side : a.Unit.FixedSide,         // the unit will act to the best of its knowledge
            Type = StatusEffectType.Charmed,
            Tier = 3,
            Resistable = true,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(Charm, t);
                TacticalGraphicalEffects.CreateHeartProjectile(a.Position, t.Position, t);
            },
        };
        SpellDict[SpellTypes.Charm] = Charm;

        Summon = new Spell()
        {
            Name = "Summon",
            Id = "summon",
            SpellType = SpellTypes.Summon,
            Description = "Summons a random monster at 50 % of caster’s experience (Available monsters depend on what monsters are available to hire as mercs, or set to spawn, so that monsters you aren't interested in don't spawn.)",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Tile },
            Range = new Range(4),
            Tier = 3,
            Resistable = false,
            OnExecuteTile = (a, loc) =>
            {
                if (TacticalUtilities.OpenTile(loc, null) && a.CastSpell(Summon, null))
                {
                    var AvailableRaces = new List<Race>();
                    foreach (Race race in (Race[])System.Enum.GetValues(typeof(Race)))
                    {
                        if (race >= Race.Vagrants && race < Race.Selicia && (Config.World.GetValue($"Merc {race}") || (Config.SpawnerInfoWithoutGeneration(race)?.Enabled ?? false)))
                            AvailableRaces.Add(race);
                    }
                    AvailableRaces.Remove(Race.Dragon);
                    if (AvailableRaces.Any() == false)
                        AvailableRaces.Add(Race.Vagrants);
                    Race summonRace = AvailableRaces[State.Rand.Next(AvailableRaces.Count())];
                    Unit unit = new Unit(a.Unit.Side, summonRace, (int)(a.Unit.Experience * .50f), true, UnitType.Summon);
                    var actorCharm = a.Unit.GetStatusEffect(StatusEffectType.Charmed);
                    if (actorCharm != null)
                    {
                        unit.ApplyStatusEffect(StatusEffectType.Charmed, actorCharm.Strength, actorCharm.Duration);
                    }
                        
                    StrategicUtilities.SpendLevelUps(unit);
                    State.GameManager.TacticalMode.AddUnitToBattle(unit, loc);
                }
            },
        };
        SpellDict[SpellTypes.Summon] = Summon;

        Enlarge = new StatusSpell()
        {
            Name = "Enlarge",
            Id = "enlarge",
            SpellType = SpellTypes.Enlarge,
            Description = "Makes target unit 20% bigger, increasing stats and increasing body size and stomach size",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Ally },
            Range = new Range(4),
            Tier = 3,
            Resistable = false,
            Duration = (a, t) => 2 + a.Unit.GetStat(Stat.Mind) / 15,
            Type = StatusEffectType.Enlarged,
            Effect = (a, t) => .2f,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(Enlarge, t);
                TacticalGraphicalEffects.CreateGenericMagic(a.Position, t.Position, t, TacticalGraphicalEffects.SpellEffectIcon.Buff);
            },
        };
        SpellDict[SpellTypes.Enlarge] = Enlarge;


        Diminishment = new StatusSpell()
        {
            Name = "Diminishment",
            Id = "diminishment",
            SpellType = SpellTypes.Diminishment,
            Description = "Reduces target in size greatly, cutting all its statistics (including size) by 75%.",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy },
            Range = new Range(6),
            Duration = (a, t) => 1 + a.Unit.GetStat(Stat.Mind) / 20,
            Effect = (a, t) => .75f,
            Type = StatusEffectType.Diminished,
            Tier = 4,
            Resistable = true,
            ResistanceMult = 1.5f,
            OnExecute = (a, t) => a.CastStatusSpell(Diminishment, t),
        };
        SpellDict[SpellTypes.Diminishment] = Diminishment;

        GateMaw = new Spell()
        {
            Name = "Gate Maw",
            Id = "gate-maw",
            SpellType = SpellTypes.GateMaw,
            Description = "Attempts to teleport all targets in radius into caster's belly, adding caster's Power to Voracity. If caster doesn't have enough stomach space, targets are unaffected.",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy, AbilityTargets.Tile },
            Range = new Range(4),
            Tier = 4,
            AreaOfEffect = 1,
            Resistable = true,
            OnExecute = (a, t) => a.CastMaw(GateMaw, t),
            OnExecuteTile = (a, l) => a.CastMaw(GateMaw, null, l),
        };
        SpellDict[SpellTypes.GateMaw] = GateMaw;

        Resurrection = new Spell()
        {
            Name = "Resurrection",
            Id = "resurrection",
            SpellType = SpellTypes.Resurrection,
            Description = "Resurrects the highest exp unit that died during this battle",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Tile },
            Range = new Range(4),
            Tier = 4,
            Resistable = false,
            OnExecuteTile = (a, loc) =>
            {
                var target = TacticalUtilities.FindUnitToResurrect(a);
                if (target != null)
                {
                    if (TacticalUtilities.OpenTile(loc, null) && a.CastSpell(Resurrection, null))
                    {
                        if (State.GameManager.TacticalMode.IsPlayerInControl && State.GameManager.CurrentScene == State.GameManager.TacticalMode)
                        {
                            TacticalUtilities.CreateResurrectionPanel(loc, a.Unit.Side);
                        }
                        else
                        {
                            TacticalUtilities.Resurrect(loc, target);
                        }
                    }
                }

            },
        };
        SpellDict[SpellTypes.Resurrection] = Resurrection;

        ViperPoisonDamage = new DamageSpell()
        {
            Name = "Viper Spit",
            Id = "viperDamage",
            SpellType = SpellTypes.ViperDamage,
            Description = "(Designed as part of the Viper spit spell)",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy, AbilityTargets.Tile },
            Range = new Range(6),
            AreaOfEffect = 1,
            Tier = 0,
            Resistable = true,
            DamageType = DamageTypes.Poison,
            Damage = (a, t) => 5 + a.Unit.GetStat(Stat.Mind) / 10,
            OnExecute = (a, t) =>
            {
                
            },
            OnExecuteTile = (a, l) =>
            {
              
            },
        };
        SpellDict[SpellTypes.ViperDamage] = ViperPoisonDamage;

        ViperPoisonStatus = new StatusSpell()
        {
            Name = "Poison Spit",
            Id = "viper-poison",
            SpellType = SpellTypes.ViperPoison,
            Description = "Deals damage and applies a strong short lived poison in a 3x3 area",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy, AbilityTargets.Tile },
            Range = new Range(6),
            Duration = (a, t) => 4,
            Effect = (a, t) => 2 + a.Unit.GetStat(Stat.Mind) / 10,
            AreaOfEffect = 1,
            Type = StatusEffectType.Poisoned,
            Tier = 0,
            Resistable = true,
            ResistanceMult = 1f,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(ViperPoisonStatus, t);
                a.Unit.RestoreMana(2);
                a.CastOffensiveSpell(ViperPoisonDamage, t);
                TacticalGraphicalEffects.CreatePoisonCloud(t.Position);
            },
            OnExecuteTile = (a, loc) =>
            {
                a.CastStatusSpell(ViperPoisonStatus, null, loc);
                a.Unit.RestoreMana(2);
                a.CastOffensiveSpell(ViperPoisonDamage, null, loc);
                TacticalGraphicalEffects.CreatePoisonCloud(loc);
            }

        };
        SpellDict[SpellTypes.ViperPoison] = ViperPoisonStatus;

        AlraunePuff = new StatusSpell()
        {
            Name = "Pollen Cloud",
            Id = "alraune-spell",
            SpellType = SpellTypes.AlraunePuff,
            Description = "Applies a variety of status effects in a 3x3 area",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy, AbilityTargets.Tile },
            Range = new Range(8),
            Duration = (a, t) => 3,
            Effect = (a, t) => .15f,
            AreaOfEffect = 1,
            Type = StatusEffectType.Shaken,
            Tier = 0,
            Resistable = true,
            ResistanceMult = 1f,
            Alraune = true,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(AlraunePuff, t);
                TacticalGraphicalEffects.CreatePollenCloud(t.Position);
            },
            OnExecuteTile = (a, loc) =>
            {
                a.CastStatusSpell(AlraunePuff, null, loc);
                TacticalGraphicalEffects.CreatePollenCloud(loc);
            }

        };
        SpellDict[SpellTypes.AlraunePuff] = AlraunePuff;

        Web = new StatusSpell()
        {
            Name = "Web",
            Id = "web",
            SpellType = SpellTypes.Web,
            Description = "Webs the target, lowering their movement to 1 for a few turns and lowering their stats",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy },
            Range = new Range(4),
            Duration = (a, t) => 3,
            Effect = (a, t) => 1,
            Type = StatusEffectType.Webbed,
            Tier = 0,
            Resistable = true,
            ResistanceMult = 0.55f,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(Web, t);
                TacticalGraphicalEffects.CreateSpiderWeb(a.Position, t.Position, t);
            },

        };
        SpellDict[SpellTypes.Web] = Web;

        GlueBomb = new StatusSpell()
        {
            Name = "Glue Bomb",
            Id = "glueBomb",
            SpellType = SpellTypes.GlueBomb,
            Description = "Glues a 3x3 area, greatly slowing them down for a significant number of turns.",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy, AbilityTargets.Tile },
            Range = new Range(8),
            Duration = (a, t) => 8,
            Effect = (a, t) => 1,
            AreaOfEffect = 1,
            Type = StatusEffectType.Glued,
            Tier = 0,
            Resistable = true,
            ResistanceMult = 0.45f,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(GlueBomb, t);
                TacticalGraphicalEffects.CreateGlueBomb(a.Position, t.Position);
            },
            OnExecuteTile = (a, loc) =>
            {
                a.CastStatusSpell(GlueBomb, null, loc);
                TacticalGraphicalEffects.CreateGlueBomb(a.Position, loc);
            }

        };
        SpellDict[SpellTypes.GlueBomb] = GlueBomb;

        Petrify = new StatusSpell()
        {
            Name = "Petrify",
            Id = "petrify",
            SpellType = SpellTypes.Petrify,
            Description = "Petrifies the target, preventing them from taking any action or dodging, but reducing damage taken and making them harder to eat.",
            AcceptibleTargets = new List<AbilityTargets>() { AbilityTargets.Enemy },
            Range = new Range(4),
            Duration = (a, t) => 4,
            Effect = (a, t) => 1,
            Type = StatusEffectType.Petrify,
            Tier = 0,
            Resistable = true,
            ResistanceMult = .9f,
            OnExecute = (a, t) =>
            {
                a.CastStatusSpell(Petrify, t);                
            },

        };
        SpellDict[SpellTypes.Petrify] = Petrify;

    }


}


public class Spell
{
    internal string Name;
    internal string Id;
    internal string Description;
    internal List<AbilityTargets> AcceptibleTargets;
    internal SpellTypes SpellType;
    internal Range Range;
    internal int Tier;
    /// <summary>The number of tiles away a target is affected (i.e. 1 = 9 total tiles)</summary>
    internal int AreaOfEffect;
    internal Action<Actor_Unit, Vec2i> OnExecuteTile;
    internal bool Resistable;
    internal float ResistanceMult = 1;
    internal Action<Actor_Unit, Actor_Unit> OnExecute;

    internal bool TryCast(Actor_Unit actor, Vec2i location)
    {
        if (actor.Unit.Mana >= ManaCost && actor.Movement > 0)
        {
            int startMana = actor.Unit.Mana;
            OnExecuteTile(actor, location);
            if (actor.Unit.SingleUseSpells?.Contains(SpellType) ?? false && actor.Unit.Mana != startMana)
            {
                actor.Unit.SingleUseSpells.Remove(SpellType);
                actor.Unit.UpdateSpells();
            }
            return true;
        }
        return false;
    }
    internal bool TryCast(Actor_Unit actor, Actor_Unit target)
    {
        if (actor.Unit.Mana >= ManaCost && actor.Movement > 0)
        {
            int startMana = actor.Unit.Mana;
            OnExecute(actor, target);
            if (actor.Unit.SingleUseSpells?.Contains(SpellType) ?? false && actor.Unit.Mana != startMana)
            {
                actor.Unit.SingleUseSpells.Remove(SpellType);
                actor.Unit.UpdateSpells();
            }
            return true;
        }
        return false;
    }

    internal int ManaCost => 2 * (int)Math.Pow(2, Tier);


}

//May redo this with interfaces later on

class DamageSpell : Spell
{

    internal Func<Actor_Unit, Actor_Unit, int> Damage;
    internal DamageTypes DamageType = DamageTypes.Generic;
}

class StatusSpell : Spell
{
    internal Func<Actor_Unit, Actor_Unit, int> Duration;
    internal Func<Actor_Unit, Actor_Unit, float> Effect;
    internal StatusEffectType Type;
    internal bool Alraune = false;
}

//class MultiTargetSpell : Spell
//{
//    internal Range SecondRange;
//}
