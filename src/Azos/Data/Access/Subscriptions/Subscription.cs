/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

using Azos.Collections;

namespace Azos.Data.Access.Subscriptions
{
  /// <summary>
  /// Thrown by provider to indicate that such a subscribtion can not be established in principle,
  /// i.e. you may have passed a zero size of populus group, the server may respond with this error
  /// to indicate the principal error in request
  /// </summary>
  [Serializable]
  public class InvalidSubscriptionRequestException : DataAccessException
  {
    public InvalidSubscriptionRequestException(string message, Exception inner) : base(message, inner) {}
    protected InvalidSubscriptionRequestException(SerializationInfo info, StreamingContext context) : base(info, context) {}
  }


  /// <summary>
  /// Represents a subscription to the CRUD Data.
  /// Call Dispose() to terminate the subscription.
  /// </summary>
  public abstract class Subscription : SubscriptionAppComponent, INamed
  {
    protected Subscription(ICRUDSubscriptionStoreImplementation store, string name, Query query, Mailbox mailbox, object correlate) : base(store)
    {
      if (store==null ||
          query==null||
          mailbox==null)
        throw new DataAccessException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(args...null)");

      if (mailbox.Store!=store)
        throw new DataAccessException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(mailbox.Store!=this.Store)");

      m_Store = store;
      m_Name = name.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString() : name;
      m_Query = query;
      m_Mailbox = mailbox;
      Correlate = correlate;

      var reg = m_Store.Subscriptions as Registry<Subscription>;
      if (!reg.RegisterOrReplace(this, out Subscription existing))
        existing.Dispose();

      ((Registry<Subscription>)mailbox.Subscriptions).Register(this);
    }

    protected override void Destructor()
    {
      if (m_Mailbox!=null)
       ((Registry<Subscription>)m_Mailbox.Subscriptions).Unregister(this);

      if (m_Store!=null)
       ((Registry<Subscription>)m_Store.Subscriptions).Unregister(this);

      base.Destructor();
    }

    private InvalidSubscriptionRequestException m_InvalidSubscriptionException;
    private ICRUDSubscriptionStoreImplementation m_Store;
    private string  m_Name;
    private bool    m_IsLoaded;
    private Query   m_Query;
    private Mailbox m_Mailbox;

    public ICRUDSubscriptionStore Store => m_Store;

    public string Name => m_Name;

    public bool IsLoaded => m_IsLoaded;

    public Query Query => m_Query;

    public Mailbox Mailbox => m_Mailbox;

    /// <summary>
    /// If this property is not null then this subscription is !IsValid
    /// </summary>
    public InvalidSubscriptionRequestException InvalidSubscriptionException => m_InvalidSubscriptionException;

    /// <summary>
    /// Returns false when this subscription experienced InvalidSubscriptionException
    /// </summary>
    public bool IsValid => m_InvalidSubscriptionException == null;

    /// <summary>
    /// Allows to attach arbtrary bject for correlation
    /// </summary>
    public object Correlate{get; set;}

    /// <summary>
    /// Called by descendants to invalidate this subscription
    /// </summary>
    protected void Invalidate(InvalidSubscriptionRequestException error)
    {
      m_InvalidSubscriptionException = error;
    }

    /// <summary>
    /// Call after subscription has been initialized
    /// </summary>
    protected void HasLoaded()
    {
      m_IsLoaded = true;
    }
  }

}
