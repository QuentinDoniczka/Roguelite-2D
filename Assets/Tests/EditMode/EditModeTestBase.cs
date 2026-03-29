using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public abstract class EditModeTestBase
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        protected GameObject Track(GameObject go)
        {
            _created.Add(go);
            return go;
        }

        [TearDown]
        public virtual void TearDown()
        {
            foreach (var go in _created)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }

            _created.Clear();
        }
    }
}
