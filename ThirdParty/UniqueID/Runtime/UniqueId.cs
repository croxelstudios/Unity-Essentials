using System;
using UnityEngine;

namespace CleverCrow.Fluid.UniqueIds
{
    [ExecuteAlways]
    [DefaultExecutionOrder(-10000)]
    public class UniqueId : MonoBehaviour, IUniqueId
    {
        [HideInInspector]
        [SerializeField]
        protected string _id = null;

        public virtual string Id { get { return _id; } }

        /// <summary>
        /// Populate an ID only if it's missing
        /// </summary>
        public void PopulateIdIfEmpty()
        {
            if (!string.IsNullOrEmpty(_id)) return;
            ScrambleId();
        }

        /// <summary>
        /// Assign a new random ID and overwrite the previous
        /// </summary>
        public void ScrambleId()
        {
            _id = Guid.NewGuid().ToString();
        }
    }
}
