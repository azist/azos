
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Collections
{

    /// <summary>
    /// Specifies the phase of the event i.e. before/after
    /// </summary>
    public enum EventPhase { Before, After}

    /// <summary>
    /// Event handler for list changes
    /// </summary>
    public delegate bool EventedCollectionGetReadOnlyHandler<TContext>(EventedCollectionBase<TContext> collection);


    /// <summary>
    /// Provides base implementation for some evented collections
    /// </summary>
    public abstract class EventedCollectionBase<TContext>
    {
        protected EventedCollectionBase()
        {

        }

        protected EventedCollectionBase(TContext context, bool contextReadOnly)
        {
            m_ContextReadOnly = contextReadOnly;
            m_Context = context;
        }


        private bool m_ContextReadOnly;
        private TContext m_Context;



        /// <summary>
        /// Returns true to indicate that Context property can not be set (was injected in .ctor only
        /// </summary>
        public bool ContextReadOnly
        {
            get { return m_ContextReadOnly;}
        }

        /// <summary>
        /// Context that this structure works in
        /// </summary>
        public TContext Context
        {
            get { return m_Context; }
            set
            {
                if (m_ContextReadOnly)
                 throw new AzosException(StringConsts.INVALID_OPERATION_ERROR + this.GetType().FullName+".Context.set()");
                m_Context = value;
            }
        }

        [field:NonSerialized] public EventedCollectionGetReadOnlyHandler<TContext> GetReadOnlyEvent;

        /// <summary>
        /// Indicates whether collection can be modified
        /// </summary>
        public bool IsReadOnly
        {
            get { return GetReadOnlyEvent!=null ? GetReadOnlyEvent(this) : true; }
        }


        public void CheckReadOnly()
        {
           if (IsReadOnly)
            throw new AzosException(StringConsts.READONLY_COLLECTION_MUTATION_ERROR + this.GetType().FullName+".CheckReadOnly()");
        }

    }
}
