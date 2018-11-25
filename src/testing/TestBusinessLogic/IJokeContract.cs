/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;

using Azos.Security;
using Azos.Glue;
using Azos.Glue.Protocol;

namespace TestBusinessLogic
{
    [Serializable]
    public class TextInfoHeader : Header
    {
      public string Text;
      public string Info;
    }

    [Glued]
    public interface IExampleContract
    {
      object ExampleMethod(string name);
    }


    [Glued]
    //[SultanPermission( 1000 )]
    [AuthenticationSupport]
    public interface IJokeContract
    {
      [AdHocPermission("/TestPermissions/Space/Flight", "Echo", AccessLevel.VIEW_CHANGE)]
      [SultanPermission(9)]
      string Echo(string text);

      /// <summary>
      /// Echo without permissions
      /// </summary>
      string UnsecureEcho(string text);


      [ArgsMarshalling(typeof(RequestMsg_IJokeContract_UnsecEchoMar))]
      string UnsecEchoMar(string text);


      string SimpleWorkAny(string s, int i1, int i2, bool b, double d);

      [ArgsMarshalling(typeof(RequestMsg_IJokeContract_SimpleWorkMar))]
      string SimpleWorkMar(string s, int i1, int i2, bool b, double d);


      /// <summary>
      /// Emulates database work that returns dummy data and pauses for some interval emulating blocking backend access
      /// </summary>
      object DBWork(string id, int recCount, int waitMs);

      [OneWay]
      //[SultanPermission(AccessLevel.VIEW)]
      void Notify(string text);

      object ObjectWork(object dummy);
    }

    public sealed class RequestMsg_IJokeContract_UnsecEchoMar : RequestMsg
    {
        public RequestMsg_IJokeContract_UnsecEchoMar(MethodInfo method, Guid? instance) : base(method, instance){}
        public RequestMsg_IJokeContract_UnsecEchoMar(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base(contract, method, oneWay, instance){}

        public string MethodArg_0_text;
    }

    public sealed class RequestMsg_IJokeContract_SimpleWorkMar : RequestMsg
    {
        public RequestMsg_IJokeContract_SimpleWorkMar(MethodInfo method, Guid? instance) : base(method, instance){}
        public RequestMsg_IJokeContract_SimpleWorkMar(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base(contract, method, oneWay, instance){}

        public string MethodArg_0_s;
        public int    MethodArg_1_i1;
        public int    MethodArg_2_i2;
        public bool   MethodArg_3_b;
        public double MethodArg_4_d;
    }



    [Glued]
    [LifeCycle(ServerInstanceMode.Stateful, 20000)]
    [AuthenticationSupport]
    public interface IJokeCalculator
    {
       [Constructor]
       void Init(int value);

       [SultanPermission( 250 )]
       int Add(int value);
       int Sub(int value);

       [Destructor]
       int Done();

    }




}
