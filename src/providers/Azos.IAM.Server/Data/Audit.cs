using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;
using Azos.IAM.Protocol;

namespace Azos.IAM.Server.Data
{
  /// <summary>
  /// Represents an audit log item which describes a change made in the system by an actor operating
  /// via an application from a host at the specified timestamp
  /// </summary>
  public sealed class Audit : Entity
  {
    /// <summary> Describes a single field change </summary>
    public sealed class FieldChange : BaseDoc
    {
      [Field(required: true, description: "The name of the field which has changed")]
      [Field(typeof(FieldChange), nameof(FieldName), TMONGO, backendName: "f")]
      public string FieldName { get; set; }

      [Field(required: true, description: "Old field value before change")]
      [Field(typeof(FieldChange), nameof(OldValue), TMONGO, backendName: "o")]
      public string OldValue  { get; set; }

      [Field(required: true, description: "New field value after change")]
      [Field(typeof(FieldChange), nameof(NewValue), TMONGO, backendName: "n")]
      public string NewValue  { get; set; }
    }

    [Field(required: true,
           metadata: "idx{name='act' order='0' dir=asc}",
           description: "Actor/User who caused the change")]
    [Field(typeof(Audit), nameof(Actor), TMONGO, backendName: "act")]
    public GDID      Actor { get; set; }

    [Field(required: true, description: "Canonical user name/id when user gets deleted")]
    [Field(typeof(Audit), nameof(ActorTitle), TMONGO, backendName: "aact")]
    public string    ActorTitle { get; set; }

    [Field(required: true, description: "Application/User Agent which caused the change")]
    [Field(typeof(Audit), nameof(ActorUserAgent), TMONGO, backendName: "aua")]
    public string    ActorUserAgent {  get; set;}

    [Field(required: true, description: "Host/System from where user caused the change")]
    [Field(typeof(Audit), nameof(ActorHost), TMONGO, backendName: "ahst")]
    public string    ActorHost { get; set; }

    /// <summary>
    /// Target entity type - what has changed (e.g. group, role, user, login)
    /// </summary>
    [Field(required: true,
           description: "Specifies what entity was effected by this change",
           metadata: "idx{name='entity' order='0'}")]
    [Field(typeof(Audit), nameof(Entity), TMONGO, backendName: "ent")]
    public Atom? Entity{ get; set;}

    /// <summary>
    /// Entity GDID -
    /// </summary>
    [Field(required: true,
           description: "Primary key of the entity that was changed",
           metadata: "idx{name='entity' order='1' dir=asc}")]
    [Field(typeof(Audit), nameof(G_Entity), TMONGO, backendName: "g_ent")]
    public GDID G_Entity{ get; set;}

    [Field(required: true,
           description:
 @"Groups multiple changes by an ID value in one logical change, set to the first ID of the
 first change in subsequent changes. IF this instance does not represent a batch change, then this value must equal the GDID PK",
           metadata: "idx{name='batch' order='0' dir=asc}")]
    [Field(typeof(Audit), nameof(BatchId), TMONGO, backendName: "bat")]
    public GDID BatchId{  get; set;}

    [Field(required: true, description: "Provides brief textual description of a change/reason")]
    [Field(typeof(Audit), nameof(Description), TMONGO, backendName: "d")]
    public string Description{  get; set; }

    [Field(required: true, description: "Defines what has changed: insert|update|delete",valueList: ValueLists.ENTITY_VERSION_STATUS_VALUE_LIST)]
    [Field(typeof(Audit), nameof(ChangeType), TMONGO, backendName: "tp")]
    public char?  ChangeType { get; set; }

    [Field(required: true, description: "A list of changes applied to the entity")]
    [Field(typeof(Audit), nameof(FieldChanges), TMONGO, backendName: "chl")]
    public FieldChange[] FieldChanges{  get; set; }
  }
}
