using System.Collections.Generic;
using System.Threading.Tasks;
using Azos.Sky.Workers;

namespace Azos.Sky.Social
{
#warning refactor to use module
  public static class SocialGraphTodos
  {
    public static void EnqueueSubscribtion(Todo todo)                       { SkySystem.ProcessManager.Enqueue(todo, SocialConsts.HOST_SET_SOCIAL_GRAPH, SocialConsts.SVC_SOCIAL_GRAPH_TODO); }
    public static void EnqueueSubscribtion(IEnumerable<Todo> todos)         { SkySystem.ProcessManager.Enqueue(todos, SocialConsts.HOST_SET_SOCIAL_GRAPH, SocialConsts.SVC_SOCIAL_GRAPH_TODO); }
    public static Task Async_EnqueueSubscribtion(Todo todo)                 { return SkySystem.ProcessManager.Async_Enqueue(todo, SocialConsts.HOST_SET_SOCIAL_GRAPH, SocialConsts.SVC_SOCIAL_GRAPH_TODO); }
    public static Task Async_EnqueueSubscribtion(IEnumerable<Todo> todos)   { return SkySystem.ProcessManager.Async_Enqueue(todos, SocialConsts.HOST_SET_SOCIAL_GRAPH, SocialConsts.SVC_SOCIAL_GRAPH_TODO); }

    public static void EnqueueDelivery(Todo todo)                       { SkySystem.ProcessManager.Enqueue(todo, SocialConsts.HOST_SET_SUBS_DELIVERY, SocialConsts.SVC_SUBS_DELIBERY_TODO); }
    public static void EnqueueDelivery(IEnumerable<Todo> todos)         { SkySystem.ProcessManager.Enqueue(todos, SocialConsts.HOST_SET_SUBS_DELIVERY, SocialConsts.SVC_SUBS_DELIBERY_TODO); }
    public static Task Async_EnqueueDelivery(Todo todo)                 { return SkySystem.ProcessManager.Async_Enqueue(todo, SocialConsts.HOST_SET_SUBS_DELIVERY, SocialConsts.SVC_SUBS_DELIBERY_TODO); }
    public static Task Async_EnqueueDelivery(IEnumerable<Todo> todos)   { return SkySystem.ProcessManager.Async_Enqueue(todos, SocialConsts.HOST_SET_SUBS_DELIVERY, SocialConsts.SVC_SUBS_DELIBERY_TODO); }

    public static void EnqueueRemove(Todo todo)                       { SkySystem.ProcessManager.Enqueue(todo, SocialConsts.HOST_SET_SUBS_REMOVE, SocialConsts.SVC_SUBS_REMOVE_TODO); }
    public static void EnqueueRemove(IEnumerable<Todo> todos)         { SkySystem.ProcessManager.Enqueue(todos, SocialConsts.HOST_SET_SUBS_REMOVE, SocialConsts.SVC_SUBS_REMOVE_TODO); }
    public static Task Async_EnqueueRemove(Todo todo)                 { return SkySystem.ProcessManager.Async_Enqueue(todo, SocialConsts.HOST_SET_SUBS_REMOVE, SocialConsts.SVC_SUBS_REMOVE_TODO); }
    public static Task Async_EnqueueRemove(IEnumerable<Todo> todos)   { return SkySystem.ProcessManager.Async_Enqueue(todos, SocialConsts.HOST_SET_SUBS_REMOVE, SocialConsts.SVC_SUBS_REMOVE_TODO); }
  }
}