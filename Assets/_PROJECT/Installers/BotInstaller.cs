using Zenject;

public class BotInstaller: MonoInstaller {
    
    public override void InstallBindings() {
        BindBotStateManager();
        Container.BindInterfacesAndSelfTo<NavMeshHelper>().AsSingle().NonLazy();
    }

    private void BindBotStateManager() {
        
        Container.BindInterfacesAndSelfTo<BotsMainManager>().AsSingle().NonLazy();
        Container.Bind<BotStateManager>()
            .FromComponentsInHierarchy()
            .AsTransient();

    }
}
