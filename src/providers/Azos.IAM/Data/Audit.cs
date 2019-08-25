using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;

namespace Azos.IAM.Data
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
      [Field(required: true, backendName: "fn", description: "The name of the field which has changed")]
      public string FieldName { get; set; }

      [Field(required: true, backendName: "old", description: "Old field value before change, <null> is used for null values")]
      public string OldValue  { get; set; }

      [Field(required: true, backendName: "new", description: "New field value after change, <null> is used for null values")]
      public string NewValue  { get; set; }
    }

    [Field(required: true,
           backendName: "g_act",
            metadata: "idx{name='act' order='0' dir=asc}",
           description: "Actor/User who caused the change")]
    public GDID      G_Actor { get; set; }

    [Field(required: true, backendName: "aact", description: "Canonical user name/id when user gets deleted")]
    public string    CNActor { get; set; }

    [Field(required: true, backendName: "aapp", description: "Application which caused the change")]
    public Atom?     ActorApp {  get; set;}

    [Field(required: true, backendName: "ahost", description: "Host/System from where user caused the change")]
    public string    ActorHost { get; set; }

    /// <summary>
    /// Target entity type - what has changed (e.g. group, role, user, login)
    /// </summary>
    [Field(required: true,
           backendName: "ent",
           description: "Specifies what entity was effected by this change",
           metadata: "idx{name='entity' order='1'}")]
    public Atom? Entity{ get; set;}

    /// <summary>
    /// Entity GDID -
    /// </summary>
    [Field(required: true,
           backendName: "g_ent",
           description: "Primary key of the entity that was changed",
           metadata: "idx{name='entity' order='0' /*the most selectivity*/ dir=asc}")]
    public GDID G_Entity{ get; set;}

    [Field(required: true,
           backendName: "batch",
           description:
 @"Groups multiple changes by an ID value in one logical change, set to the first ID of the
 first change in subsequent changes. IF this instance does not represent a batch change, then this value must equal the GDID PK",
           metadata: "idx{name='batch' order='0' dir=asc}")]
    public GDID BatchId{  get; set;}

    [Field(required: true, backendName: "d", description: "Provides brief textual description of a change/reason")]
    public string Description{  get; set; }

    [Field(required: true, backendName: "ctp", description: "Defines what has changed: insert|update|delete")]
    public DocChangeType?  ChangeType { get; set; }

    [Field(required: true, backendName: "clst", description: "A list of changes applied to the entity")]
    public FieldChange[] FieldChanges{  get; set; }
  }
}
