/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Reflection;

using Azos.Data;
using Azos.Data.Business;
using Azos.Scripting;
using Azos.Security;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonDocNullableFieldTests
  {

    [Bix("5a43e224-012a-4c00-be80-e75c03547d6f")]
    [Schema(Description = "Boruch something")]
    public class DocWithNullables : AmorphousTypedDoc
    {
      public override bool AmorphousDataEnabled => true;

      /// <summary>
      /// User account realm
      /// </summary>
      [Field(required: true, Description = "User account realm")]
      public Atom Realm { get; set; }

      /// <summary>
      /// User account Gdid
      /// </summary>
      [Field(required: true, Description = "User account Gdid")]
      public GDID Gdid { get; set; }

      /// <summary>
      /// User account Gdid
      /// </summary>
      [Field(required: true, Description = "User account Guid")]
      public Guid Guid { get; set; }

      /// <summary>
      /// Name/(Screen Name)/Uri//Title of user account unique per realm
      /// </summary>
      [Field(required: true, Description = "Name/(Screen Name)/Uri//Title of user account unique per realm")]
      public string Name { get; set; }

      /// <summary>
      /// User access level
      /// </summary>
      [Field(required: true, Description = "User access level")]
      public UserStatus Level { get; set; }

      /// <summary>
      /// User description
      /// </summary>
      [Field(required: true, Description = "User description")]
      public string Description { get; set; }

      /// <summary>
      /// When user privilege takes effect
      /// </summary>
      [Field(required: true, Description = "When user privilege takes effect")]
      public DateRange ValidSpanUtc { get; set; }

      /// <summary>
      /// Tree path for org unit. So the user list may be searched by it
      /// </summary>
      [Field(required: false, Description = "Tree path for org unit. So the user list may be searched by it")]
      public EntityId? OrgUnit { get; set; }

      /// <summary>
      /// Properties such as tree connections (e.g. roles) and claims
      /// </summary>
      [Field(required: true, Description = "Properties such as tree connections (e.g. roles) and claims")]
      public ConfigVector Props { get; set; }

      /// <summary>
      /// User-specific Rights override or null for default rights
      /// </summary>
      [Field(Description = "User-specific Rights override or null for default rights")]
      public ConfigVector Rights { get; set; }

      /// <summary>
      /// Free form text notes associated with the account
      /// </summary>
      [Field(Description = "Free form text notes associated with the account")]
      public string Note { get; set; }

      /// <summary>
      /// Creation version (UTC, Actor, Origin)
      /// </summary>
      [Field(required: true, Description = "Creation version (UTC, Actor, Origin)")]
      public VersionInfo CreateVersion { get; set; }

      /// <summary>
      /// Version of this data record
      /// </summary>
      [Field(required: true, Description = "Version of this data record")]
      public VersionInfo DataVersion { get; set; }

      /// <summary>
      /// Lock timestamp range, if set the account is inactive past that timestamp, until LOCK_END_UTC
      /// </summary>
      [Field(Description = "Lock timestamp range, if set the account is inactive past that timestamp, until LOCK_END_UTC")]
      public DateRange? LockSpanUtc { get; set; }

      /// <summary>
      /// Who locked the user account
      /// </summary>
      [Field(Description = "Who locked the user account")]
      public EntityId? LockActor { get; set; }

      /// <summary>
      /// Short note explaining lock reason/status
      /// </summary>
      [Field(Description = "Short note explaining lock reason/status")]
      public string LockNote { get; set; }
    }




    [Run]
    public void Test01()
    {
      var json = @"{""ValidSpanUtc"": {start: ""1/1/2001"", end: ""12/31/2029""}, ""OrgUnit"": ""path@sys::addr""}";

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.OrgUnit);

      Aver.AreEqual(2029, got.ValidSpanUtc.End.Value.Year);
      Aver.AreEqual("addr", got.OrgUnit.Value.Address);

      got.See();
    }

    [Run]
    public void Test02()
    {
      var json =
    @"{
      'Realm': 'gdi',
      'Gdid': '0:1:2369',
      'Guid': 'ff8278f6-2e58-4596-8d92-51ffd03b3c53',
      'Name': 'usr02',
      'Level': 'Administrator',
      'Description': 'USer number two',
      'ValidSpanUtc': {
        'start': '2001-01-01T00:00:00Z',
        'end': '3091-01-01T00:00:00Z'
      },
      'OrgUnit': 'path@org::demons',
      'Props': '{\'prop\':{\'role\':\'path@role::\\\/se\\\/dolts\',\'claims\':{\'pub\':{\'newspaper\':\'NYTimes\'}}}}',
      'Rights': '',
      'Note': 'Reads New York times while on crapper',
      'CreateVersion': {
        'G_Version': '0:1:2369',
        'Utc': '2023-08-22T18:11:36Z',
        'Origin': 'devga',
        'Actor': 'usrn@idp::root',
        'State': 'Created'
      },
      'DataVersion': {
        'G_Version': '0:1:2369',
        'Utc': '2023-08-22T18:11:36Z',
        'Origin': 'devga',
        'Actor': 'usrn@idp::root',
        'State': 'Created'
      },
      'LockSpanUtc': {
        'start': null,
        'end': null
      },
      'LockActor': null,
      'LockNote': null
    }";

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.OrgUnit);

      Aver.AreEqual(3091, got.ValidSpanUtc.End.Value.Year);
      Aver.AreEqual("demons", got.OrgUnit.Value.Address);

      got.See();
    }

    [Run]
    public void Test03()
    {
      var map =
    @"
     {'Realm':'gdi','Gdid':'0:1:2369','Guid':'ff8278f6-2e58-4596-8d92-51ffd03b3c53','Name':'usr02','Level':'Administrator','Description':'USer number two',
     'ValidSpanUtc':{'start':'2001-01-01T00:00:00Z','end':'3091-01-01T00:00:00Z'},'OrgUnit':'path@org::demons',// 'SucxkUnit': 'shalom',
     'Props':'{\'prop\':{\'role\':\'path@role::\\\/se\\\/dolts\',\'claims\':{\'pub\':{\'newspaper\':\'NYTimes\'}}}}','Rights':'',
     'Note':'Reads New York times while on crapper','CreateVersion':{'G_Version':'0:1:2369','Utc':'2023-08-22T18:11:36Z','Origin':'devga',
     'Actor':'usrn@idp::root','State':'Created'},'DataVersion':{'G_Version':'0:1:2369','Utc':'2023-08-22T18:11:36Z','Origin':'devga','Actor':'usrn@idp::root',
     'State':'Created'},'LockSpanUtc':{'start':null,'end':null},'LockActor':null,'LockNote':null
     }".JsonToDataObject() as JsonDataMap;

      Aver.IsNotNull(map);
      var got = JsonReader.ToDoc<DocWithNullables>(map);
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.OrgUnit);

      Aver.AreEqual(3091, got.ValidSpanUtc.End.Value.Year);
      Aver.AreEqual("demons", got.OrgUnit.Value.Address);

      got.AmorphousData.See();
      got.See();
    }

  }
}
