using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Missions;
using VrEfm;
using Module = VrEfm.Module;

//This part of the class is mainly used to get the selected bomb/module. Mostly from Command Line
public partial class VrEfmService
{
    private List<Bomb> Bombs = new List<Bomb> { };
    private List<BombCommander> BombCommanders = new List<BombCommander> { };
    private List<Module> Modules = new List<Module> { };

    private bool BombActive = false;
    private bool Infinite = false;

    private IEnumerator CheckNewModule(Action<Module> OnNew)
    {
        Module OldModule = null;
        while(true)
        {
            yield return null;
            var NewModule = GetFocusedModule();
            if(NewModule!=OldModule)
            {
                OnNew(NewModule);
                OldModule = NewModule;
            }
        }
    }

    private IEnumerator CheckNewBomb(Action<BombCommander> OnNew)
    {
        BombCommander OldBomb = null;
        while (true)
        {
            yield return null;
            var NewBomb = GetHeldBomb();
            if (NewBomb != OldBomb)
            {
                OnNew(NewBomb);
                OldBomb = NewBomb;
            }
        }
    }

    private BombCommander GetHeldBomb()
    {
        BombCommander held = null;
        foreach (BombCommander commander in BombCommanders)
        {
            if (commander.IsHeld())
                held = commander;
        }
        return held;
    }

    private Module GetFocusedModule()
    {
        Module focused = null;
        foreach (Module module in Modules)
        {
            if (module.IsHeld())
                focused = module;
        }
        return focused;
    }

    private IEnumerator CheckForBomb()
    {
        yield return new WaitUntil(() => SceneManager.Instance.GameplayState.Bombs != null && SceneManager.Instance.GameplayState.Bombs.Count > 0);
        yield return new WaitForSeconds(2.0f);
        Bombs.AddRange(SceneManager.Instance.GameplayState.Bombs);
        int i = 0;
        string[] keyModules =
        {
            "SouvenirModule", "MemoryV2", "TurnTheKey", "TurnTheKeyAdvanced", "theSwan", "HexiEvilFMN", "taxReturns"
        };
        foreach (Bomb bomb in Bombs)
        {
            BombCommanders.Add(new BombCommander(bomb, i));
            foreach (BombComponent bombComponent in bomb.BombComponents)
            {
                ComponentTypeEnum componentType = bombComponent.ComponentType;
                bool keyModule = false;
                string moduleName = string.Empty;

                switch (componentType)
                {
                    case ComponentTypeEnum.Empty:
                    case ComponentTypeEnum.Timer:
                        continue;

                    case ComponentTypeEnum.NeedyCapacitor:
                    case ComponentTypeEnum.NeedyKnob:
                    case ComponentTypeEnum.NeedyVentGas:
                    case ComponentTypeEnum.NeedyMod:
                        moduleName = bombComponent.GetModuleDisplayName();
                        keyModule = true;
                        break;

                    case ComponentTypeEnum.Mod:
                        KMBombModule KMModule = bombComponent.GetComponent<KMBombModule>();
                        keyModule = keyModules.Contains(KMModule.ModuleType);
                        goto default;

                    default:
                        moduleName = bombComponent.GetModuleDisplayName();
                        break;
                }
                Module module = new Module(bombComponent, i)
                {
                    ComponentType = componentType,
                    IsKeyModule = keyModule,
                    ModuleName = moduleName
                };

                Modules.Add(module);
            }
            i++;
        }
        BombActive = true;
    }

    #region Factory Implementation
    private IEnumerator FactoryCheck()
    {
        yield return new WaitUntil(() => SceneManager.Instance.GameplayState.Bombs != null && SceneManager.Instance.GameplayState.Bombs.Count > 0);
        GameObject _gameObject = null;
        for (var i = 0; i < 4 && _gameObject == null; i++)
        {
            _gameObject = GameObject.Find("Factory_Info");
            yield return null;
        }

        if (_gameObject == null) yield break;

        _factoryType = ReflectionHelper.FindType("FactoryAssembly.FactoryRoom");
        if (_factoryType == null) yield break;

        _factoryBombType = ReflectionHelper.FindType("FactoryAssembly.FactoryBomb");
        _internalBombProperty = _factoryBombType.GetProperty("InternalBomb", BindingFlags.NonPublic | BindingFlags.Instance);

        _factoryStaticModeType = ReflectionHelper.FindType("FactoryAssembly.StaticMode");
        _factoryFiniteModeType = ReflectionHelper.FindType("FactoryAssembly.FiniteSequenceMode");
        _factoryInfiniteModeType = ReflectionHelper.FindType("FactoryAssembly.InfiniteSequenceMode");
        _currentBombField = _factoryFiniteModeType.GetField("_currentBomb", BindingFlags.NonPublic | BindingFlags.Instance);

        _gameModeProperty = _factoryType.GetProperty("GameMode", BindingFlags.NonPublic | BindingFlags.Instance);

        List<UnityEngine.Object> factoryObject = FindObjectsOfType(_factoryType).ToList();

        if (factoryObject == null || factoryObject.Count == 0) yield break;

        _factory = factoryObject[0];
        _gameroom = _gameModeProperty.GetValue(_factory, new object[] { });
        if (_gameroom.GetType() == _factoryInfiniteModeType)
        {
            Infinite = true;
            StartCoroutine(WaitUntilEndFactory());
        }
    }

    private UnityEngine.Object GetBomb => (UnityEngine.Object)_currentBombField.GetValue(_gameroom);

    private IEnumerator WaitUntilEndFactory()
    {
        yield return new WaitUntil(() => GetBomb != null);

        while (GetBomb != null)
        {
            UnityEngine.Object currentBomb = GetBomb;
            Bomb bomb1 = (Bomb)_internalBombProperty.GetValue(currentBomb, null);
            yield return new WaitUntil(() => bomb1.HasDetonated || bomb1.IsSolved());

            Modules.Clear();
            BombCommanders.Clear();
            Bombs.Clear();

            while (currentBomb == GetBomb)
            {
                yield return new WaitForSeconds(0.10f);
                if (currentBomb != GetBomb)
                    continue;
                yield return new WaitForSeconds(0.10f);
            }

            StartCoroutine(CheckForBomb());
        }
    }
    //factory specific types

    private static Type _factoryType = null;
    private static Type _factoryBombType = null;
    private static PropertyInfo _internalBombProperty = null;

    private static Type _factoryStaticModeType = null;
    private static Type _factoryFiniteModeType = null;
    private static Type _factoryInfiniteModeType = null;

    private static PropertyInfo _gameModeProperty = null;
    private static FieldInfo _currentBombField = null;

    private object _factory = null;
    private object _gameroom = null;
    #endregion
}