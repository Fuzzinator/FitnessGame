using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class NewPlaylistEnvSetter : EnvironmentSetter
    {
        public override void SetDropdownOption(int value)
        {
            if (PlaylistMaker.Instance != null)
            {
                var targetEnv = _dropdownField.options[value].text;
                PlaylistMaker.Instance.SetTargetEnvironment(targetEnv);
            }
        }

        protected override int GetOptionIndex(string optionName)
        {
            return 0;
        }
    }
}