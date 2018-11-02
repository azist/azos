
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
