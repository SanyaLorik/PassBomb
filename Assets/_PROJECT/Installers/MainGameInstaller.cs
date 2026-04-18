using UnityEngine;
using Zenject;

public class MainGameInstaller : MonoInstaller {
    public override void InstallBindings() {
        BindThrow();
        BindViews();
    }
    
    
    private void BindThrow() {
        Container.Bind<MainGameStarter>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<BattleManager>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<BonusManager>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<BonusesLoader>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<Bomb>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<PlayersRoleManager>().FromComponentInHierarchy().AsSingle().NonLazy();
    }


    private void BindViews() {
        Container.Bind<GameOverShower>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<BattleStartVisualizer>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<BattleDiesInformator>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<MapsToBattleChanger>().FromComponentInHierarchy().AsSingle().NonLazy();
    }
}