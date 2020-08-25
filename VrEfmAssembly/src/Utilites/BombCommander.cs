﻿using Assets.Scripts.Missions;
using Assets.Scripts.Records;

//Origin: Command Line
namespace VrEfm
{
    public class BombCommander
    {
        public Bomb Bomb { get; private set; } = null;
        public TimerComponent TimerComponent = null;
        public WidgetManager WidgetManager { get; private set; } = null;
        public Selectable Selectable { get; private set; } = null;
        public FloatingHoldable FloatingHoldable { get; private set; } = null;

        public SelectableManager SelectableManager { get; private set; } = null;
        public float CurrentTimer
        {
            get => TimerComponent.TimeRemaining;
            set => TimerComponent.TimeRemaining = (value < 0) ? 0 : value;
        }
        public int StrikeLimit
        {
            get => Bomb.NumStrikesToLose;
            set { Bomb.NumStrikesToLose = value; }
        }
        public int StrikeCount
        {
            get => Bomb.NumStrikes;
            set
            {
                if (value < 0) value = 0;
                Bomb.NumStrikes = value;
            }
        }
        public float CurrentTimerElapsed => TimerComponent.TimeElapsed;
        public readonly int Id;

        public BombCommander(Bomb bomb, int id)
        {
            Bomb = bomb;
            TimerComponent = bomb.GetTimer();
            WidgetManager = bomb.WidgetManager;
            Selectable = bomb.GetComponent<Selectable>();
            FloatingHoldable = bomb.GetComponent<FloatingHoldable>();
            Id = id;
            SelectableManager = KTInputManager.Instance.SelectableManager;
        }

        public bool IsHeld()
        {
            return FloatingHoldable.HoldState == FloatingHoldable.HoldStateEnum.Held ? true : false;
        }

        public void Detonate(string reason)
        {
            for (int strikesToMake = StrikeLimit - StrikeCount; strikesToMake > 0; --strikesToMake)
            {
                CauseStrike(reason);
            }
        }

        public void CauseStrike(string reason)
        {
            StrikeSource strikeSource = new StrikeSource
            {
                ComponentType = ComponentTypeEnum.Mod,
                InteractionType = InteractionTypeEnum.Other,
                Time = CurrentTimerElapsed,
                ComponentName = reason
            };

            RecordManager recordManager = RecordManager.Instance;
            recordManager.RecordStrike(strikeSource);

            Bomb.OnStrike(null);
        }
    }
}