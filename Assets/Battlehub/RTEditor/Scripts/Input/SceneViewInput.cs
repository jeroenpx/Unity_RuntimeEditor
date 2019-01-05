
using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class SceneViewInput : BaseViewInput<SceneView>
    {
        public KeyCode SelectAllKey = KeyCode.A;

        protected override void StartOverride()
        {
            base.StartOverride();
        }

        protected virtual bool SelectAllAction()
        {
            return Input.GetKeyDown(SelectAllKey) && Input.GetKey(ModifierKey);
        }

        protected override void UpdateOverride()
        {
            base.UpdateOverride();

            if (SelectAllAction())
            {
                View.SelectAll();
            }
        }
    }
}

