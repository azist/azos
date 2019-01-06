/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using Azos.Sky;
using Azos.Scripting;

namespace Azos.Tests.Unit.Sky
{
  [Runnable]
  public class ExtensionsTests
  {

      [Run]
      public void EX_IsSameRegionPath_1()
      {
        var path1 = "US/East/CLE/A/I/wmed0001";
        var path2 = "US/East/CLE/A.z/I.z/wmed0001";
        Aver.IsTrue( path1.IsSameRegionPath(path2) );
      }

      [Run]
      public void EX_IsSameRegionPath_2()
      {
        var path1 = "US/East/CLE/A/I/wmed0001";
        var path2 = "US/East/cle///A.z/I.z/wmed0001";
        Aver.IsTrue( path1.IsSameRegionPath(path2) );
      }

      [Run]
      public void EX_IsSameRegionPath_3()
      {
        var path1 = "US/East.r/CLE/A/I/wmed0001";
        var path2 = "US/East/CLE/A.z/I.z/wmed0001.h";
        Aver.IsTrue( path1.IsSameRegionPath(path2) );
      }

      [Run]
      public void EX_IsSameRegionPath_4()
      {
        var path1 = "US/East.r/CLE/A/I/wmed0001";
        var path2 = "US/West/CLE/A.z/I.z/wmed0001.h";
        Aver.IsFalse( path1.IsSameRegionPath(path2) );
      }

      [Run]
      public void EX_IsSameRegionPath_5()
      {
        var path1 = "US/East.r/CLE/A/I/wmed0001";
        var path2 = "US/East/CLE/A.z/I.z/G/wmed0001.h";
        Aver.IsFalse( path1.IsSameRegionPath(path2) );
      }


      [Run]
      public void EX_IsSameRegionPath_6()
      {
        var path1 = "US/ East.r /CLE /A/I/wmed0001";
        var path2 = "US/         East/CLE/A.z/ I.z /wmed0001.h";
        Aver.IsTrue( path1.IsSameRegionPath(path2) );
      }

      [Run]
      public void EX_IsSameRegionPath_7()
      {
        var path1 = "US/ East.r /CLE/A/I/wlgdyn0001";
        var path2 = "US/East/CLE/A.z/ I.z /wlgdyn0001~5-672C21E7-2950C499BF8DFA68";
        Aver.IsTrue( path1.IsSameRegionPath(path2) );
      }

      [Run]
      public void EX_IsSameRegionPath_8()
      {
        var path1 = "US/ East.r /CLE/A/I/wlgdyn0001";
        var path2 = "US/East/CLE/A.z/ I.z /wlgdyn0001 ~ 5-672C21E7-2950C499BF8DFA68";
        Aver.IsTrue( path1.IsSameRegionPath(path2) );
      }

      [Run]
      public void EX_IsSameRegionPath_9()
      {
        var path1 = "US/ East.r /CLE/A/I/wlgdyn0001.h";
        var path2 = "US/ East/CLE/A.z/ I.z /wlgdyn0001.h ~ 5-672C21E7-2950C499BF8DFA68";
        Aver.IsTrue( path1.IsSameRegionPath(path2) );
      }

      [Run]
      public void EX_IsSameRegionPath_10()
      {
        var path1 = "US/ East.r /CLE/A/I/wlgdyn0001.h";
        var path2 = "US/ East/CLE/A.z/ I.z /wlgdyn0001~ 5-672C21E7-2950C499BF8DFA68";
        Aver.IsTrue( path1.IsSameRegionPath(path2) );
      }


      [Run]
      public void EX_GetRegionPathHashCode_1()
      {
        var path1 = "US/East.r/CLE/A/I/wmed0001";
        var path2 = "US/East/CLE/A.z/I.z/wmed0001.h";
        Aver.AreEqual( path1.GetRegionPathHashCode(), path2.GetRegionPathHashCode() );
      }

      [Run]
      public void EX_GetRegionPathHashCode_2()
      {
        var path1 = "US/East.r/CLE/A/I/wmed0001";
        var path2 = "US/West/CLE/A/I/wmed0001";
        Aver.AreNotEqual( path1.GetRegionPathHashCode(), path2.GetRegionPathHashCode() );
      }

      [Run]
      public void EX_GetRegionPathHashCode_3()
      {
        var path1 = "US/ East.r /CLE/A/I /wlgdyn0001.h";
        var path2 = "US/ East/CLE/A.z/ I.z /wlgdyn0001";
        Aver.AreEqual( path1.GetRegionPathHashCode(), path2.GetRegionPathHashCode() );
      }

      [Run]
      public void EX_GetRegionPathHashCode_4()
      {
        var path1 = "US/ East.r /CLE/A/I/wlgdyn0001.h";
        var path2 = "US/ East/CLE/A.z/ I.z /wlgdyn0001 ~ 5-672C21E7-2950C499BF8DFA68";
        Aver.AreEqual( path1.GetRegionPathHashCode(), path2.GetRegionPathHashCode() );
      }

      [Run]
      public void EX_GetRegionPathHashCode_5()
      {
        var path1 = "US/East.r /CLE/A/I/wlgdyn0001.h";
        var path2 = "US/ East/ CLE/A.z/ I.z /wlgdyn0001.h ~5-672C21E7-2950C499BF8DFA68";
        Aver.AreEqual( path1.GetRegionPathHashCode(), path2.GetRegionPathHashCode() );
      }

      [Run]
      public void EX_GetRegionPathHashCode_6()
      {
        var path1 = "US/East.r /CLE/A/I/wlgdyn0001  ";
        var path2 = "US/ East/ CLE/A.z/ I.z /wlgdyn0001~5-672C21E7-2950C499BF8DFA68";
        Aver.AreEqual( path1.GetRegionPathHashCode(), path2.GetRegionPathHashCode() );
      }


      [Run]
      public void EX_StripPathOfRegionExtensions_1()
      {
        Aver.AreEqual( "US/East/CLE/A/I/wmed0001", "US/East.r/CLE////A/I.z/wmed0001.h".StripPathOfRegionExtensions() );
        Aver.AreEqual( "US/East/CLE/A/I/wmed0001", "US/ East.r/CLE/ A/I.z/      wmed0001.h".StripPathOfRegionExtensions() );
        Aver.AreEqual( "US/East/CLE/A/I/wmed0001", "US/ East.r/CLE / A/I.z/   wmed0001       ".StripPathOfRegionExtensions() );
      }

      [Run]
      public void EX_StripPathOfRegionExtensions_2()
      {
        Aver.AreEqual( "US/East/CLE/A/I/wlgdyn0001", "US/East.r/CLE////A/I.z/wlgdyn0001.h".StripPathOfRegionExtensions() );
        Aver.AreEqual( "US/East/CLE/A/I/wlgdyn0001", "US/ East.r/CLE/ A/I.z/      wlgdyn0001.h".StripPathOfRegionExtensions() );
        Aver.AreEqual( "US/East/CLE/A/I/wlgdyn0001", "US/ East.r/CLE / A/I.z/   wlgdyn0001.h~5-672C21E7-2950C499BF8DFA68".StripPathOfRegionExtensions() );
        Aver.AreEqual( "US/East/CLE/A/I/wlgdyn0001", "US/ East.r/CLE / A/I.z/   wlgdyn0001~5-672C21E7-2950C499BF8DFA68".StripPathOfRegionExtensions() );
        Aver.AreEqual( "US/East/CLE/A/I/wlgdyn0001", "US/ East/CLE.noc / A/I.z/wlgdyn0001~ 5-672C21E7-2950C499BF8DFA68".StripPathOfRegionExtensions() );
        Aver.AreEqual( "US/East/CLE/A/I/wlgdyn0001", "US/ East/CLE.noc / A/I.z/wlgdyn0001.h~ 5-672C21E7-2950C499BF8DFA68".StripPathOfRegionExtensions() );
      }


      [Run]
      public void EX_IsValidName()
      {
        Aver.IsTrue( "aaa".IsValidName() );
        Aver.IsTrue( "a aa".IsValidName() );
        Aver.IsTrue( "a a a".IsValidName() );

        Aver.IsFalse( " aaa".IsValidName() );
        Aver.IsFalse( "aaa ".IsValidName() );
        Aver.IsFalse( " aaa ".IsValidName() );
        Aver.IsFalse( " a aa ".IsValidName() );

        Aver.IsTrue( "$abcd-fgh".IsValidName() );
        Aver.IsFalse( "$abc\rd-fgh".IsValidName() );
        Aver.IsFalse( "$abc\nd-fgh".IsValidName() );
        Aver.IsFalse( "abc*def".IsValidName() );
        Aver.IsFalse( "abc&def".IsValidName() );
        Aver.IsFalse( "abc~def".IsValidName() );
        Aver.IsFalse( "abc<def".IsValidName() );
        Aver.IsFalse( "abc>def".IsValidName() );
        Aver.IsFalse( "abc{def".IsValidName() );
        Aver.IsFalse( "abc}def".IsValidName() );
        Aver.IsFalse( "abc(def".IsValidName() );
        Aver.IsFalse( "abc)def".IsValidName() );

        Aver.IsFalse( "abc@def".IsValidName() );
        Aver.IsFalse( "abc#def".IsValidName() );

        Aver.IsFalse( ("abc"+Azos.Sky.Metabase.Metabank.HOST_DYNAMIC_SUFFIX_SEPARATOR+"def").IsValidName() );
      }
  }
}
