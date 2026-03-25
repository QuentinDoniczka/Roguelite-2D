using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    /// <summary>
    /// Base class for Edit Mode tests that create GameObjects.
    /// Tracks all created objects and destroys them immediately in TearDown.
    /// </summary>
    public abstract class EditModeTestBase
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        /// <summary>
        /// Registers a GameObject for automatic cleanup in TearDown.
        /// Returns the same object for inline use.
        /// </summary>
        protected GameObject Track(GameObject go)
        {
            _created.Add(go);
            return go;
        }

        [TearDown]
        public void TearDown()
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
