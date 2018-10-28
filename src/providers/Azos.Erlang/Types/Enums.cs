/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

// Author:  Serge Aleynikov <saleyn@gmail.com>
// Created: 2013-08-26
// This code is derived from:
// https://github.com/saleyn/otp.net

namespace Azos.Erlang
{
  /// <summary>
  /// Defines ordering semantics for Erlang types
  /// </summary>
  public enum ErlTypeOrder
  {
    ErlObject = 0,
    ErlAtom,
    ErlBinary,
    ErlBoolean,
    ErlByte,
    ErlDouble,
    ErlLong,
    ErlList,
    ErlPid,
    ErlPort,
    ErlRef,
    ErlString,
    ErlTuple,
    ErlVar,
    ErlMap
  }

  /// <summary>
  /// Tags used for external format serialization
  /// </summary>
  /// <remarks>
  /// https://github.com/erlang/otp/blob/master/lib/erl_interface/include/ei.h
  /// </remarks>
  internal enum ErlExternalTag
  {
    SmallInt        = 97,
    Int             = 98,
    Float           = 99,
    NewFloat        = 70,
    Atom            = 100,
    SmallAtom       = 115,
    AtomUtf8        = 118,
    SmallAtomUtf8   = 119,
    Ref             = 101,
    NewRef          = 114,
    Port            = 102,
    Pid             = 103,
    SmallTuple      = 104,
    LargeTuple      = 105,
    Nil             = 106,
    String          = 107,
    List            = 108,
    Bin             = 109,
    SmallBigInt     = 110,
    LargeBigInt     = 111,
    Map             = 116,

    Version         = 131,
  }
}
