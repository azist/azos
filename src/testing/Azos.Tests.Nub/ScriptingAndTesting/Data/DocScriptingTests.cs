/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

using Azos.Scripting.Expressions.Data;
using System.Linq;

namespace Azos.Tests.Nub.ScriptingAndTesting.Data
{
  [Runnable]
  public class DocScriptingTests
  {
    [Run("id=null fa=true fb=true nm=abc  averVisible=true averMarked=true")]
    [Run("id=123 fa=false fb=false nm=abc  averVisible=false averMarked=false")]
    [Run("id=null fa=false fb=false nm=abc  averVisible=false averMarked=true")]
    public void Case01(int? id, bool fa, bool fb, string nm, bool averVisible, bool averMarked)
    {
      var data = new DocA();
      data.Id = id;
      data.FlagA = fa;
      data.FlagB = fb;
      data.Name = nm;

      var ctx = new ScriptCtx(data);

      var atrName = data.Schema["Name"]["*"];

      var (found1, isVisible) = ctx.RunScript(atrName, "visible");
      var (found2, isMarked) = ctx.RunScript(atrName, "marked");
      Aver.IsTrue(found1);
      Aver.IsTrue(found2);
      Aver.AreEqual(averVisible, isVisible.AsBool());
      Aver.AreEqual(averMarked, isMarked.AsBool());
    }

    [Run]
    public void Case02()
    {
      var data = new DocA();

      var ctx = new ScriptCtx(data);

      var atrSchema = data.Schema.SchemaAttrs.FirstOrDefault();

      var (found, valid) = ctx.RunScript(atrSchema, "validate");
      Aver.IsTrue(found);
      Aver.IsFalse(valid.AsBool());

      data.Name = "Jichael Mackson";
      (found, valid) = ctx.RunScript(atrSchema, "validate");
      Aver.IsTrue(found);
      Aver.IsTrue(valid.AsBool());

      data.Name = null;
      (found, valid) = ctx.RunScript(atrSchema, "validate");
      Aver.IsTrue(found);
      Aver.IsFalse(valid.AsBool());//false again

      data.FlagA = true;
      (found, valid) = ctx.RunScript(atrSchema, "validate");
      Aver.IsTrue(found);
      Aver.IsTrue(valid.AsBool());//true because of flag

      data.FlagB = true;
      (found, valid) = ctx.RunScript(atrSchema, "validate");
      Aver.IsTrue(found);
      Aver.IsTrue(valid.AsBool());//true because of flag

      data.FlagA = false;
      (found, valid) = ctx.RunScript(atrSchema, "validate");
      Aver.IsTrue(found);
      Aver.IsTrue(valid.AsBool());//true because of flagB

      data.FlagA = false;
      data.FlagB = false;
      (found, valid) = ctx.RunScript(atrSchema, "validate");
      Aver.IsTrue(found);
      Aver.IsFalse(valid.AsBool());//both flags are turned off
    }

    [Schema(MetadataContent = "./")]
    class DocA : TypedDoc
    {
      [Field]
      public int? Id { get; set; }

      [Field]
      public bool FlagA{ get; set;}

      [Field]
      public bool FlagB { get; set; }

      [Field(MetadataContent = "./")]
      public string Name{ get; set;}
    }

  }//tests
}
