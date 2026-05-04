using Architecture_M;
using SanyaBeerExtension;
using UnityEngine;
using Zenject;

public class DeviceActivity : MonoBehaviour
{
    [Header("Mobile Disactivty When Desktop")]
    [SerializeField] private GameObject[] _mobileDisactivities;

    [Inject] private IDeviceTypeProvider _deviceTypeProvider;

    private void Awake()
    {
        if (_deviceTypeProvider.DeviceType == DeviceTypeEnum.Desktop) 
            _mobileDisactivities.ForEach(mobile => mobile.DisactiveSelf());
    }
}