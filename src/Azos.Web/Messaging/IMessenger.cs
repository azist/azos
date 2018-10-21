
using Azos.Apps;
using Azos.Conf;

namespace Azos.Web.Messaging
{
    /// <summary>
    /// Describes an entity that can send EMails
    /// </summary>
    public interface IMessenger : IApplicationComponent
    {
      void SendMsg(Message msg);
    }

    public interface IMessengerImplementation : IMessenger, IConfigurable, IService, IApplicationFinishNotifiable
    {

    }
}
