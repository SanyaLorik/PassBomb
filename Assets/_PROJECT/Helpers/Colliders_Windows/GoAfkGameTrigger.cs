using UnityEngine;
using Zenject;

public class GoAfkGameTrigger : TriggerBehaviourBase {
    [SerializeField] private bool _afkStatusOnEnter;
    
    [Inject] MainGameStarter _gameStarter;
    
    
    protected override void PlayerBehaviourOnEnter() {
        _gameStarter.ChangeAfkStatus(_afkStatusOnEnter);
    }

    protected override void PlayerBehaviourOnExit() { }
}