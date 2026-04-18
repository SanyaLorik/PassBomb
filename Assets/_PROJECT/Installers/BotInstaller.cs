using Zenject;

public class BotInstaller: MonoInstaller {
    
    public override void InstallBindings() {
        BindBotStateManager();
    }

    private void BindBotStateManager() {
        
        Container.BindInterfacesAndSelfTo<BotsMainManager>().AsSingle().NonLazy();
        Container.Bind<BotStateManager>()
            .FromComponentsInHierarchy()
            .AsTransient();

    }
}
