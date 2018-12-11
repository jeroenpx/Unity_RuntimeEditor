using UnityEngine;

namespace Battlehub.UIControls.MenuControl
{
    public class MenuTest : MonoBehaviour
    {
        public void InvalidateMenuItem(MenuItemValidationArgs args)
        {
            args.IsValid = false;
        }

    }

}
