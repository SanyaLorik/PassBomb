using Zenject;

public class PlayerInstaller : MonoInstaller {
    
    public override void InstallBindings() {
        BindPlayerSingletones();
    }

    private void BindPlayerSingletones() {
        Container.BindInterfacesAndSelfTo<PlayerMovement>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<PlayerStateManager>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<PlayerBank>().FromComponentInHierarchy().AsSingle().NonLazy();
    }


}
