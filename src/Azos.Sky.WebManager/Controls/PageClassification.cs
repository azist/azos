/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Sky.WebManager.Controls
{
  interface ISitePage{}

    interface IMainPage : ISitePage{}
      interface IHomePage             : IMainPage{}
      interface IConsolePage          : IMainPage{}
      interface IInstrumentationPage  : IMainPage{}
      interface ITheSystemPage        : IMainPage{}
      interface IProcessManagerPage   : IMainPage{}
}
