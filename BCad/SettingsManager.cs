using System;
using BCad.Services;

namespace BCad
{
    [Serializable]
    public class SettingsManager : DefaultSettingsManager
    {
        internal void SetInputService(IInputService inputService)
        {
            InputService = inputService;
        }
    }
}
