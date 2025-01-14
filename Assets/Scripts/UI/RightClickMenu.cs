﻿using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RightClickMenu : MonoBehaviour
{
    Button[] Buttons;
    public Transform ButtonFolder;
    public GameObject ButtonPrefab;
    RectTransform Rect;

    Button[] PounceButtons;
    public Transform PouncePanel;
    RectTransform PounceRect;

    bool activeWait;
    bool pounceNeedsRefresh;

    const int MaxButtons = 30;

    struct CommandData
    {
        internal Actor_Unit Actor;
        internal Actor_Unit Target;
        internal int Range;
        internal float DevourChance;
    }

    public void CloseAll()
    {
        gameObject.SetActive(false);
        PouncePanel.gameObject.SetActive(false);
    }

    public void Open(Actor_Unit actor, Actor_Unit target)
    {
        PouncePanel.gameObject.SetActive(false);
        pounceNeedsRefresh = true;
        if (State.TutorialMode && State.GameManager.TutorialScript.step < 6)
        {
            State.GameManager.CreateMessageBox("Can't use the right click action menu for the first battle of the tutorial");
            return;
        }

        if (Rect == null)
            Rect = GetComponent<RectTransform>();
        gameObject.SetActive(true);
        CreateButtons(actor, target);
        float xAdjust = 10;
        float exceeded = Input.mousePosition.x + (Rect.rect.width * Screen.width / 1920) - Screen.width;
        if (exceeded > 0)
            xAdjust = -exceeded;
        transform.position = Input.mousePosition + new Vector3(xAdjust, 0, 0);
    }

    public void OpenWithNoTarget(Actor_Unit actor, Vec2i location)
    {
        PouncePanel.gameObject.SetActive(false);

        if (Rect == null)
            Rect = GetComponent<RectTransform>();
        gameObject.SetActive(true);
        CreateButtonsForNoTarget(actor, location);
        float xAdjust = 10;
        float exceeded = Input.mousePosition.x + (Rect.rect.width * Screen.width / 1920) - Screen.width;
        if (exceeded > 0)
            xAdjust = -exceeded;
        transform.position = Input.mousePosition + new Vector3(xAdjust, 0, 0);
    }

    public void CreateButtonsForNoTarget(Actor_Unit actor, Vec2i location)
    {
        int currentButton = 0;
        const int ButtonCount = MaxButtons;
        if (Buttons == null)
        {
            Buttons = new Button[ButtonCount];
            for (int i = 0; i < ButtonCount; i++)
            {
                Buttons[i] = Instantiate(ButtonPrefab, ButtonFolder).GetComponent<Button>();
            }
        }

        for (int i = 0; i < ButtonCount; i++)
        {
            Buttons[i].gameObject.SetActive(false);
            Buttons[i].interactable = true;
            Buttons[i].onClick.RemoveAllListeners();
            Destroy(Buttons[i].gameObject.GetComponent<EventTrigger>());
        }
        if (TacticalUtilities.OpenTile(location, actor))
        {
            Buttons[currentButton].GetComponentInChildren<Text>().text = "Move to location";
            Buttons[currentButton].onClick.AddListener(() => State.GameManager.TacticalMode.OrderSelectedUnitToMoveTo(location.x, location.y));
            Buttons[currentButton].onClick.AddListener(FinishMoveAction);
            currentButton++;
        }
        int range = actor.Position.GetNumberOfMovesDistance(location);
        foreach (Spell spell in actor.Unit.UseableSpells)
        {
            if (spell.AcceptibleTargets.Contains(AbilityTargets.Tile))
            {
                currentButton = AddSpellLocation(spell, actor, location, currentButton, range, 1);
            }
        }
        foreach (TargetedTacticalAction action in TacticalActionList.TargetedActions.Where(s => s.OnExecuteLocation != null && s.AppearConditional(actor)))
        {
            Buttons[currentButton].onClick.AddListener(() => action.OnExecuteLocation(actor, location));
            Buttons[currentButton].onClick.AddListener(FinishAction);
            Buttons[currentButton].GetComponentInChildren<Text>().text = action.Name;
            currentButton++;
        }
        if (currentButton == 0)
        {
            CloseAll();
        }
        else
            ActivateButtons(currentButton);
    }

    public void CreateButtons(Actor_Unit actor, Actor_Unit target)
    {

        //var racePar = RaceParameters.GetTraitData(actor.Unit.Race);
        int currentButton = 0;
        const int ButtonCount = MaxButtons;
        if (Buttons == null)
        {
            Buttons = new Button[ButtonCount];
            for (int i = 0; i < ButtonCount; i++)
            {
                Buttons[i] = Instantiate(ButtonPrefab, ButtonFolder).GetComponent<Button>();
            }
        }
        for (int i = 0; i < ButtonCount; i++)
        {
            Buttons[i].gameObject.SetActive(false);
            Buttons[i].interactable = true;
            Buttons[i].onClick.RemoveAllListeners();
            Destroy(Buttons[i].gameObject.GetComponent<EventTrigger>());
        }
        int range = actor.Position.GetNumberOfMovesDistance(target.Position);

        if (actor == target)
        {
            foreach (Spell spell in actor.Unit.UseableSpells)
            {
                if (spell.AcceptibleTargets.Contains(AbilityTargets.Ally))
                {
                    currentButton = AddSpell(spell, actor, target, currentButton, range, 1);
                }
            }
            Buttons[currentButton].onClick.AddListener(() => actor.BellyRub(target));
            Buttons[currentButton].onClick.AddListener(FinishAction);
            if (target.ReceivedRub)
            {
                Buttons[currentButton].interactable = false;
                Buttons[currentButton].GetComponentInChildren<Text>().text = "Belly Rub\nAlready rubbed";
            }
            else
                Buttons[currentButton].GetComponentInChildren<Text>().text = "Belly Rub";
            currentButton++;

            foreach (var action in TacticalActionList.UntargetedActions)
            {
                Buttons[currentButton].onClick.AddListener(() => action.OnClicked());
                Buttons[currentButton].onClick.AddListener(FinishAction);
                Buttons[currentButton].GetComponentInChildren<Text>().text = action.Name;
                currentButton++;
            }

            ActivateButtons(currentButton);
            return;
        }


        if (target.Unit.Side == actor.Unit.Side)
        {
            foreach (Spell spell in actor.Unit.UseableSpells)
            {
                if (spell.AcceptibleTargets.Contains(AbilityTargets.Ally))
                {
                    currentButton = AddSpell(spell, actor, target, currentButton, range, 1);
                }
            }
            Buttons[currentButton].onClick.AddListener(() => actor.BellyRub(target));
            Buttons[currentButton].onClick.AddListener(FinishAction);
            if (target.ReceivedRub)
            {
                Buttons[currentButton].interactable = false;
                Buttons[currentButton].GetComponentInChildren<Text>().text = "Belly Rub\nAlready rubbed";
            }
            else
                Buttons[currentButton].GetComponentInChildren<Text>().text = "Belly Rub";
            if (range != 1 || target.PredatorComponent?.Fullness <= 0)
                Buttons[currentButton].interactable = false;
            currentButton++;

            if (target.Surrendered == false && actor.Unit.HasTrait(Traits.Cruel) == false && Config.AllowInfighting == false)
            {
                if (actor.Unit.HasTrait(Traits.Endosoma))
                {
                    float devChance;
                    if (actor.PredatorComponent != null)
                        devChance = Mathf.Round(100 * target.GetDevourChance(actor, true));
                    else
                        devChance = 0;

                    CommandData data2 = new CommandData()
                    {
                        Actor = actor,
                        Target = target,
                        Range = range,
                        DevourChance = devChance
                    };
                    currentButton = AddVore(actor, currentButton, data2);
                }
                ActivateButtons(currentButton);
                return;
            }
        }
        Buttons[currentButton].onClick.AddListener(() => State.GameManager.TacticalMode.MeleeAttack(actor, target));
        Buttons[currentButton].onClick.AddListener(FinishAction);
        int damage = actor.WeaponDamageAgainstTarget(target, false);
        Buttons[currentButton].GetComponentInChildren<Text>().text = $"Melee Attack {Math.Round(100 * target.GetAttackChance(actor, false, true))}% {(damage >= target.Unit.Health ? "Kill" : $"{damage} dmg")} ";
        if (range != 1)
            Buttons[currentButton].interactable = false;
        currentButton++;


        if (actor.BestRanged != null)
        {
            Buttons[currentButton].onClick.AddListener(() => State.GameManager.TacticalMode.RangedAttack(actor, target));
            Buttons[currentButton].onClick.AddListener(FinishAction);
            damage = actor.WeaponDamageAgainstTarget(target, true);
            Buttons[currentButton].GetComponentInChildren<Text>().text = $"Ranged Attack {Math.Round(100 * target.GetAttackChance(actor, true, true))}% {(damage >= target.Unit.Health ? "Kill" : $"{damage} dmg")} ";
            if (actor.BestRanged.Omni == false && (range < 2 || range > actor.BestRanged.Range))
                Buttons[currentButton].interactable = false;
            currentButton++;
        }


        float devourChance;
        if (actor.PredatorComponent != null)
            devourChance = Mathf.Round(100 * target.GetDevourChance(actor, true));
        else
            devourChance = 0;

        CommandData data = new CommandData()
        {
            Actor = actor,
            Target = target,
            Range = range,
            DevourChance = devourChance
        };

        if (actor.Unit.UseableSpells != null)
        {
            foreach (Spell spell in actor.Unit.UseableSpells)
            {
                if (spell.AcceptibleTargets.Contains(AbilityTargets.Enemy))
                {
                    if (spell == SpellList.Maw)
                        currentButton = AddSpell(spell, actor, target, currentButton, range, target.GetMagicChance(actor, spell) * target.GetDevourChance(actor, skillBoost: actor.Unit.GetStat(Stat.Mind)));
                    else
                        currentButton = AddSpell(spell, actor, target, currentButton, range, target.GetMagicChance(actor, spell));
                }
            }
        }


        if (actor.Unit.HasTrait(Traits.Pounce))
        {
            Buttons[currentButton].onClick.AddListener(() => CreatePounceButtons(actor, target));
            if (actor.Movement > 1)
            {
                Buttons[currentButton].GetComponentInChildren<Text>().text = "Pounces =>";
                var trigger = Buttons[currentButton].gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                entry.callback.AddListener((s) => { CreatePounceButtons(actor, target); });
                trigger.triggers.Add(entry);
                entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerExit
                };
                entry.callback.AddListener((s) => { Invoke("QueueCloseLoop", .25f); });
                trigger.triggers.Add(entry);
                if (range < 2 || range > 4)
                    Buttons[currentButton].interactable = false;
                currentButton++;
            }
            else
            {
                Buttons[currentButton].GetComponentInChildren<Text>().text = "Pounces (No AP)";
                Buttons[currentButton].interactable = false;
                currentButton++;
            }           

        }

	if (target.Unit.Side != actor.Unit.Side && 
            (Config.CanUseStomachRubOnEnemies || actor.Unit.HasTrait(Traits.SeductiveTouch)))
        {
            Buttons[currentButton].onClick.AddListener(() => actor.BellyRub(target));
            Buttons[currentButton].onClick.AddListener(FinishAction);
            if (target.ReceivedRub)
            {
                Buttons[currentButton].interactable = false;
                Buttons[currentButton].GetComponentInChildren<Text>().text = "Belly Rub\nAlready rubbed";
            }
            else
                Buttons[currentButton].GetComponentInChildren<Text>().text = "Belly Rub" + (actor.Unit.HasTrait(Traits.SeductiveTouch) ? " (Seduce " + Math.Round(100 * target.GetPureStatClashChance(actor.Unit.GetStat(Stat.Dexterity), target.Unit.GetStat(Stat.Will), .1f)) + "%)" :  "");
            if (range != 1 || !(target.PredatorComponent?.Fullness > 0))                                     // Still can't rub empty bellies
                Buttons[currentButton].interactable = false;
            currentButton++;
        }

        currentButton = AddVore(actor, currentButton, data);

        if (actor.Unit.HasTrait(Traits.ShunGokuSatsu))
        {
            if (TacticalActionList.TargetedDictionary.TryGetValue(SpecialAction.ShunGokuSatsu, out var targetedAction))
            {
                if (targetedAction.AppearConditional(data.Actor))
                {
                    Buttons[currentButton].onClick.AddListener(() => targetedAction.OnExecute(data.Actor, data.Target));
                    Buttons[currentButton].onClick.AddListener(FinishAction);
                    damage = 2 * actor.WeaponDamageAgainstTarget(target, false);
                    Buttons[currentButton].GetComponentInChildren<Text>().text = $"Shun Goku Satsu {Math.Round(100 * target.GetAttackChance(actor, false, true))}% {(damage >= target.Unit.Health ? "Kill" : $"{damage} dmg")} ";
                    if (data.Range != 1)
                        Buttons[currentButton].interactable = false;
                    currentButton++;
                }

            }
        }


        if (actor.PredatorComponent != null)
        {
            if (data.Target.PredatorComponent != null)
                data.DevourChance = Mathf.Round(100 * data.Target.PredatorComponent.GetVoreStealChance(data.Actor));
            currentButton = AddKTCommands(actor, currentButton, data);
        }

        ActivateButtons(currentButton);
    }

    private int AddVore(Actor_Unit actor, int currentButton, CommandData data)
    {
        if (actor.PredatorComponent != null)
        {
            var voreTypes = State.RaceSettings.GetVoreTypes(actor.Unit.Race);
            if (voreTypes.Contains(VoreType.Oral))
            {
                Buttons[currentButton].onClick.AddListener(() => State.GameManager.TacticalMode.VoreAttack(data.Actor, data.Target));
                Buttons[currentButton].onClick.AddListener(FinishAction);
                Buttons[currentButton].GetComponentInChildren<Text>().text = $"Oral Vore {data.DevourChance}%";
                if (actor.Unit.HasTrait(Traits.RangedVore))
                {
                    if (data.Range > 4)
                        Buttons[currentButton].interactable = false;
                }
                else
                {
                    if (data.Range != 1)
                        Buttons[currentButton].interactable = false;
                }
                if (data.Actor.PredatorComponent.FreeCap() < data.Target.Bulk())
                {
                    Buttons[currentButton].GetComponentInChildren<Text>().text = $"Too bulky to vore";
                    Buttons[currentButton].interactable = false;

                }
                currentButton++;
            }

            currentButton = AltVore(actor, currentButton, SpecialAction.BreastVore, data);
            currentButton = AltVore(actor, currentButton, SpecialAction.CockVore, data);
            currentButton = AltVore(actor, currentButton, SpecialAction.Unbirth, data);
            currentButton = AltVore(actor, currentButton, SpecialAction.AnalVore, data);
            currentButton = AltVore(actor, currentButton, SpecialAction.TailVore, data);


        }

        return currentButton;
    }

    private int AltVore(Actor_Unit actor, int currentButton, SpecialAction actionType, CommandData data)
    {
        if (TacticalActionList.TargetedDictionary.TryGetValue(actionType, out var targetedAction))
        {
            if (targetedAction.AppearConditional(data.Actor) && (targetedAction.RequiresPred == false || data.Actor.PredatorComponent != null))
            {
                Buttons[currentButton].onClick.AddListener(() => targetedAction.OnExecute(data.Actor, data.Target));
                Buttons[currentButton].onClick.AddListener(FinishAction);
                if (actionType == SpecialAction.TailVore && actor.Unit.Race == Race.Terrorbird)
                    Buttons[currentButton].GetComponentInChildren<Text>().text = $"Crop Vore {data.DevourChance}%";
                else
                    Buttons[currentButton].GetComponentInChildren<Text>().text = $"{targetedAction.Name} {data.DevourChance}%";
                if (actor.Unit.HasTrait(Traits.RangedVore))
                {
                    if (data.Range > 4)
                        Buttons[currentButton].interactable = false;
                }
                else
                {
                    if (data.Range != 1)
                        Buttons[currentButton].interactable = false;
                }
                if (data.Actor.PredatorComponent.FreeCap() < data.Target.Bulk())
                {
                    Buttons[currentButton].GetComponentInChildren<Text>().text = $"Too bulky to {targetedAction.Name}";
                    Buttons[currentButton].interactable = false;

                }
                currentButton++;
                return currentButton;
            }

        }
        return currentButton;
    }

    private int AddSpell(Spell spell, Actor_Unit actor, Actor_Unit target, int currentButton, int range, float spellChance)
    {
        if (actor.Unit.Mana >= spell.ManaCost)
            Buttons[currentButton].GetComponentInChildren<Text>().text = $"{spell.Name} {(spell.Resistable ? Mathf.Round(100 * spellChance).ToString() : "100")}%";
        else
            Buttons[currentButton].GetComponentInChildren<Text>().text = $"{spell.Name} (no mana)";
        Buttons[currentButton].onClick.AddListener(() => spell.TryCast(actor, target));
        if (range < spell.Range.Min || range > spell.Range.Max || actor.Unit.Mana < spell.ManaCost)
            Buttons[currentButton].interactable = false;
        Buttons[currentButton].onClick.AddListener(FinishAction);
        currentButton++;
        return currentButton;
    }

    private int AddSpellLocation(Spell spell, Actor_Unit actor, Vec2i location, int currentButton, int range, float spellChance)
    {
        if (actor.Unit.Mana >= spell.ManaCost)
            Buttons[currentButton].GetComponentInChildren<Text>().text = $"{spell.Name}";
        else
            Buttons[currentButton].GetComponentInChildren<Text>().text = $"{spell.Name} (no mana)";
        Buttons[currentButton].onClick.AddListener(() => spell.TryCast(actor, location));
        if (range < spell.Range.Min || range > spell.Range.Max || actor.Unit.Mana < spell.ManaCost)
            Buttons[currentButton].interactable = false;
        Buttons[currentButton].onClick.AddListener(FinishAction);
        currentButton++;
        return currentButton;
    }

    public void CreatePounceButtons(Actor_Unit actor, Actor_Unit target)
    {
        if (pounceNeedsRefresh == false)
        {
            PouncePanel.gameObject.SetActive(true);
            activeWait = false;
        }
        int currentButton = 0;
        const int ButtonCount = 7;
        if (PounceButtons == null)
        {
            PounceButtons = new Button[ButtonCount];
            for (int i = 0; i < ButtonCount; i++)
            {
                PounceButtons[i] = Instantiate(ButtonPrefab, PouncePanel).GetComponent<Button>();
            }
        }
        for (int i = 0; i < ButtonCount; i++)
        {
            PounceButtons[i].gameObject.SetActive(false);
            PounceButtons[i].interactable = true;
            PounceButtons[i].onClick.RemoveAllListeners();
        }
        int range = actor.Position.GetNumberOfMovesDistance(target.Position);


        if (PounceRect == null)
            PounceRect = PouncePanel.GetComponent<RectTransform>();
        PouncePanel.gameObject.SetActive(true);
        float xAdjust = 60;
        float exceeded = Input.mousePosition.x + (PounceRect.rect.width * Screen.width / 1920) - Screen.width;
        if (exceeded > 0)
            xAdjust = -exceeded;
        PouncePanel.position = Input.mousePosition + new Vector3(xAdjust, 0, 0);


        float devourChance;
        if (actor.PredatorComponent != null)
            devourChance = Mathf.Round(100 * target.GetDevourChance(actor, true));
        else
            devourChance = 0;

        CommandData data = new CommandData()
        {
            Actor = actor,
            Target = target,
            Range = range,
            DevourChance = devourChance
        };

        PounceButtons[currentButton].onClick.AddListener(() => actor.MeleePounce(target));
        PounceButtons[currentButton].onClick.AddListener(FinishAction);
        int damage = actor.WeaponDamageAgainstTarget(target, false);
        PounceButtons[currentButton].GetComponentInChildren<Text>().text = $"Melee Pounce {Math.Round(100 * target.GetAttackChance(actor, false, true))}% {(damage >= target.Unit.Health ? "Kill" : $"{ damage} dmg")}";
        if (range < 2 || range > 4)
            PounceButtons[currentButton].interactable = false;
        currentButton++;
        if (actor.PredatorComponent != null)
        {
            var voreTypes = State.RaceSettings.GetVoreTypes(actor.Unit.Race);
            if (voreTypes.Contains(VoreType.Oral))
            {
                PounceButtons[currentButton].onClick.AddListener(() => actor.VorePounce(target));
                PounceButtons[currentButton].onClick.AddListener(FinishAction);
                if (data.Actor.PredatorComponent.FreeCap() < data.Target.Bulk())
                {
                    PounceButtons[currentButton].GetComponentInChildren<Text>().text = $"Too bulky to vore";
                    PounceButtons[currentButton].interactable = false;
                }
                else
                    PounceButtons[currentButton].GetComponentInChildren<Text>().text = $"Oral Vore Pounce {devourChance}%";
                if (range < 2 || range > 4)
                    PounceButtons[currentButton].interactable = false;
                currentButton++;
            }

            currentButton = AltVorePounce(data, SpecialAction.BreastVore, currentButton);
            currentButton = AltVorePounce(data, SpecialAction.CockVore, currentButton);
            currentButton = AltVorePounce(data, SpecialAction.AnalVore, currentButton);
            currentButton = AltVorePounce(data, SpecialAction.Unbirth, currentButton);
            currentButton = AltVorePounce(data, SpecialAction.TailVore, currentButton);

        }
        pounceNeedsRefresh = false;
        ActivatePounceButtons(currentButton);
    }

    private int AltVorePounce(CommandData data, SpecialAction type, int currentButton)
    {
        if (TacticalActionList.TargetedDictionary.TryGetValue(type, out var targetedAction))
        {
            if (targetedAction.AppearConditional(data.Actor) && (targetedAction.RequiresPred == false || data.Actor.PredatorComponent != null))
            {
                PounceButtons[currentButton].onClick.AddListener(() => data.Actor.VorePounce(data.Target, type));
                PounceButtons[currentButton].onClick.AddListener(FinishAction);
                if (data.Actor.PredatorComponent.FreeCap() < data.Target.Bulk())
                {
                    PounceButtons[currentButton].GetComponentInChildren<Text>().text = $"Too bulky to {targetedAction.Name}";
                    PounceButtons[currentButton].interactable = false;
                }
                else
                    PounceButtons[currentButton].GetComponentInChildren<Text>().text = $"{targetedAction.Name} Pounce {data.DevourChance}%";
                if (data.Range < 2 || data.Range > 4)
                    PounceButtons[currentButton].interactable = false;
                currentButton++;
                return currentButton;
            }

        }
        return currentButton;
    }

    private int AddKTCommands(Actor_Unit actor, int currentButton, CommandData data)
    {
        if (Config.KuroTenkoEnabled)
        {
            if (data.Actor.Unit.Side == data.Target.Unit.Side && data.Actor.Unit != data.Target.Unit)
            {
                if (actor.PredatorComponent.CanFeed())
                {
                    Buttons[currentButton].onClick.AddListener(() => data.Actor.PredatorComponent.Feed(data.Target));
                    Buttons[currentButton].onClick.AddListener(FinishAction);
                    Buttons[currentButton].GetComponentInChildren<Text>().text = $"Breastfeed";
                    currentButton++;
                }
                if (actor.PredatorComponent.CanFeedCum())
                {
                    Buttons[currentButton].onClick.AddListener(() => data.Actor.PredatorComponent.FeedCum(data.Target));
                    Buttons[currentButton].onClick.AddListener(FinishAction);
                    Buttons[currentButton].GetComponentInChildren<Text>().text = $"Feed Cum";
                    currentButton++;
                }
                if (actor.PredatorComponent.CanTransfer() && data.Target.PredatorComponent != null)
                {
                    Buttons[currentButton].onClick.AddListener(() => data.Actor.PredatorComponent.TransferAttempt(data.Target));
                    Buttons[currentButton].onClick.AddListener(FinishAction);
                    Buttons[currentButton].GetComponentInChildren<Text>().text = $"Transfer";
                    if (data.Target.PredatorComponent.FreeCap() < actor.PredatorComponent.GetTransferBulk())
                    {
                        Buttons[currentButton].GetComponentInChildren<Text>().text = $"Too bulky to Transfer";
                        Buttons[currentButton].interactable = false;
                    }
                    currentButton++;
                }
            }
            else if (data.Actor.Unit != data.Target.Unit && data.Target.PredatorComponent != null)
            {
                if (actor.PredatorComponent.CanVoreSteal(data.Target))
                {
                    Buttons[currentButton].onClick.AddListener(() => data.Actor.PredatorComponent.VoreStealAttempt(data.Target));
                    Buttons[currentButton].onClick.AddListener(FinishAction);
                    Buttons[currentButton].GetComponentInChildren<Text>().text = $"Vore Steal {data.DevourChance}%";
                    currentButton++;
                }
            }
            if (actor.PredatorComponent.CanSuckle() && actor.PredatorComponent.GetSuckle(data.Target)[0] != 0)
            {
                Buttons[currentButton].onClick.AddListener(() => data.Actor.PredatorComponent.Suckle(data.Target));
                Buttons[currentButton].onClick.AddListener(FinishAction);
                Buttons[currentButton].GetComponentInChildren<Text>().text = $"Suckle";
                currentButton++;
            }
        }
        return currentButton;
    }

    private void ActivateButtons(int currentButton)
    {
        for (int i = 0; i < currentButton; i++)
        {
            Buttons[i].gameObject.SetActive(true);
        }
        Rect.sizeDelta = new Vector2(Rect.sizeDelta.x, currentButton * 40);
    }

    private void ActivatePounceButtons(int currentButton)
    {
        for (int i = 0; i < currentButton; i++)
        {
            PounceButtons[i].gameObject.SetActive(true);
        }
        PounceRect.sizeDelta = new Vector2(PounceRect.sizeDelta.x, currentButton * 40);
    }

    void FinishAction()
    {
        State.GameManager.TacticalMode.ActionDone();
        gameObject.SetActive(false);
        PouncePanel.gameObject.SetActive(false);
    }

    void FinishMoveAction()
    {
        gameObject.SetActive(false);
        PouncePanel.gameObject.SetActive(false);
    }

    void QueueCloseLoop()
    {
        if (activeWait == false)
        {
            activeWait = true;
            CloseSecond();
        }
    }

    void CloseSecond()
    {
        if (activeWait == false)
            return;
        Vector2 localMousePosition = PounceRect.InverseTransformPoint(Input.mousePosition);
        if (PounceRect.rect.Contains(localMousePosition))
        {
            MiscUtilities.DelayedInvoke(CloseSecond, .25f);
        }
        else
        {
            PouncePanel.gameObject.SetActive(false);
            activeWait = false;
        }
    }


}
