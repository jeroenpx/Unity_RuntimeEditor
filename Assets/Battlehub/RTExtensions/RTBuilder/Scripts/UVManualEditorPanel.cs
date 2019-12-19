using UnityEngine;
using Battlehub.RTCommon;

namespace Battlehub.RTBuilder
{
    public class UVManualEditorPanel : MonoBehaviour
    {
        public IProBuilderTool Tool
        {
            get;
            set;
        }

        private void Awake()
        {
            Tool = IOC.Resolve<IProBuilderTool>();
        }

        private void OnDestroy()
        {
            Tool = null;
        }
    }
}
