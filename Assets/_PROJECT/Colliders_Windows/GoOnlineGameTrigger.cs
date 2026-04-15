using Zenject;

public class GoOnlineGameTrigger : TriggerBehaviourBase {
    [Inject] MainGameStarter _gameStarter;
    protected override void PlayerBehaviourOnEnter() {
        _gameStarter.StartOnlineGame();
    }

    protected override void PlayerBehaviourOnExit() { }
}