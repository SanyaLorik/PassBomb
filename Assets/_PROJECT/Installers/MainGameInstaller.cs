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
        Container.Bind<BattleInformator>().FromComponentInHierarchy().AsSingle().NonLazy();
    }


    private void BindViews() {
        Container.Bind<StartBattleView>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<GameOverShower>().FromComponentInHierarchy().AsSingle().NonLazy();
    }
}