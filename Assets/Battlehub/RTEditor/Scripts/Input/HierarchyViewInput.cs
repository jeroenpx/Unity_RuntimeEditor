﻿using Battlehub.RTCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class HierarchyViewInput : BaseViewInput<HierarchyView>
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
