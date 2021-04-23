using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Collections;
using Azos.Scripting;

namespace Azos.Tests.Nub.BaseCollections
{
  [Runnable]
  public class CappedQueueTests
  {
    public class MyItem
    {
      public string Name{ get; set;}
    }


    [Run]
    public void BasicQueueFunctions()
    {
      var sut = new CappedQueue<MyItem>(i => i==null ? 0 : i.Name==null ? 0 : i.Name.Length);

      Aver.IsTrue( sut.TryEnqueue(new MyItem{  Name="Cat" }) );
      Aver.AreEqual(1, sut.Count);
      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Dog" }));
      Aver.AreEqual(2, sut.Count);
      Aver.IsTrue(sut.TryEnqueue(null));
      Aver.AreEqual(3, sut.Count);
      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Bird" }));
      Aver.AreEqual(4, sut.Count);

      Aver.AreEqual(4, ((IEnumerable<MyItem>)sut).Count());

      Aver.AreEqual(4, sut.EnqueuedCount);
      Aver.AreEqual(3+3+4, sut.EnqueuedSize);

      //1st time
      Aver.IsTrue( sut.TryPeek(out var got) );
      Aver.IsNotNull(got);
      Aver.AreEqual("Cat", got.Name);

      //2nd time peek
      Aver.IsTrue(sut.TryPeek(out got));
      Aver.IsNotNull(got);
      Aver.AreEqual("Cat", got.Name);

      //3rd time dequeue
      Aver.IsTrue(sut.TryDequeue(out got));
      Aver.IsNotNull(got);
      Aver.AreEqual("Cat", got.Name);

      Aver.AreEqual(3, sut.EnqueuedCount);
      Aver.AreEqual(3 + 4, sut.EnqueuedSize);

      //4th time peek
      Aver.IsTrue(sut.TryPeek(out got));
      Aver.IsNotNull(got);
      Aver.AreEqual("Dog", got.Name);

      //5th time dequeue
      Aver.IsTrue(sut.TryDequeue(out got));
      Aver.IsNotNull(got);
      Aver.AreEqual("Dog", got.Name);

      Aver.AreEqual(2, sut.EnqueuedCount);
      Aver.AreEqual(4, sut.EnqueuedSize);

      //6th time dequeue
      Aver.IsTrue(sut.TryDequeue(out got));
      Aver.IsNull(got);

      Aver.AreEqual(1, sut.EnqueuedCount);
      Aver.AreEqual(4, sut.EnqueuedSize);

      //7th time dequeue the last
      Aver.IsTrue(sut.TryDequeue(out got));
      Aver.IsNotNull(got);
      Aver.AreEqual("Bird", got.Name);

      Aver.AreEqual(0, sut.EnqueuedCount);
      Aver.AreEqual(0, sut.EnqueuedSize);

      Aver.IsFalse(sut.TryPeek(out got));
      Aver.IsFalse(sut.TryDequeue(out got));
    }

    [Run]
    public void SetCountLimit_DefaultHandling()
    {
      var sut = new CappedQueue<MyItem>(i => i == null ? 0 : i.Name == null ? 0 : i.Name.Length);

      sut.CountLimit = 3;

      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Cat" }));
      Aver.AreEqual(1, sut.Count);

      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Dog" }));
      Aver.AreEqual(2, sut.Count);

      Aver.IsTrue(sut.TryEnqueue(null));
      Aver.AreEqual(3, sut.Count);

      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Rabbit" }));
      Aver.AreEqual(3, sut.Count);


      //1st is Dog because Cat was removed
      Aver.AreEqual(3, sut.EnqueuedCount);
      Aver.AreEqual(3+6, sut.EnqueuedSize);

      Aver.IsTrue(sut.TryDequeue(out var got));
      Aver.IsNotNull(got);
      Aver.AreEqual("Dog", got.Name);
      Aver.AreEqual(2, sut.EnqueuedCount);
      Aver.AreEqual(6, sut.EnqueuedSize);

      Aver.IsTrue(sut.TryDequeue(out got));
      Aver.IsNull(got);
      Aver.AreEqual(1, sut.EnqueuedCount);
      Aver.AreEqual(6, sut.EnqueuedSize);

      Aver.IsTrue(sut.TryDequeue(out got));
      Aver.IsNotNull(got);
      Aver.AreEqual("Rabbit", got.Name);
      Aver.AreEqual(0, sut.EnqueuedCount);
      Aver.AreEqual(0, sut.EnqueuedSize);

      Aver.IsFalse(sut.TryPeek(out got));
      Aver.IsFalse(sut.TryDequeue(out got));
    }

    [Run]
    public void SetCountLimit_DiscardOld()
    {
      var sut = new CappedQueue<MyItem>(i => i == null ? 0 : i.Name == null ? 0 : i.Name.Length);

      sut.CountLimit = 3;
      sut.Handling = QueueLimitHandling.DiscardOld;

      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Cat" }));
      Aver.AreEqual(1, sut.Count);

      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Dog" }));
      Aver.AreEqual(2, sut.Count);

      Aver.IsTrue(sut.TryEnqueue(null));
      Aver.AreEqual(3, sut.Count);

      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Rabbit" }));
      Aver.AreEqual(3, sut.Count);


      //1st is Dog because Cat was removed
      Aver.AreEqual(3, sut.EnqueuedCount);
      Aver.AreEqual(3 + 6, sut.EnqueuedSize);

      Aver.IsTrue(sut.TryDequeue(out var got));
      Aver.IsNotNull(got);
      Aver.AreEqual("Dog", got.Name);
      Aver.AreEqual(2, sut.EnqueuedCount);
      Aver.AreEqual(6, sut.EnqueuedSize);

      Aver.IsTrue(sut.TryDequeue(out got));
      Aver.IsNull(got);
      Aver.AreEqual(1, sut.EnqueuedCount);
      Aver.AreEqual(6, sut.EnqueuedSize);

      Aver.IsTrue(sut.TryDequeue(out got));
      Aver.IsNotNull(got);
      Aver.AreEqual("Rabbit", got.Name);
      Aver.AreEqual(0, sut.EnqueuedCount);
      Aver.AreEqual(0, sut.EnqueuedSize);

      Aver.IsFalse(sut.TryPeek(out got));
      Aver.IsFalse(sut.TryDequeue(out got));
    }

    [Run]
    public void SetCountLimit_DiscardNew()
    {
      var sut = new CappedQueue<MyItem>(i => i == null ? 0 : i.Name == null ? 0 : i.Name.Length);

      sut.CountLimit = 3;
      sut.Handling = QueueLimitHandling.DiscardNew;

      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Cat" }));
      Aver.AreEqual(1, sut.Count);

      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Dog" }));
      Aver.AreEqual(2, sut.Count);

      Aver.IsTrue(sut.TryEnqueue(null));
      Aver.AreEqual(3, sut.Count);

      //Rabbit will not fit - return FALSE
      Aver.IsFalse(sut.TryEnqueue(new MyItem { Name = "Rabbit" }));
      Aver.AreEqual(3, sut.Count);


      //1st is Dog because Cat was removed
      Aver.AreEqual(3, sut.EnqueuedCount);
      Aver.AreEqual(3 + 3, sut.EnqueuedSize);

      Aver.IsTrue(sut.TryDequeue(out var got));
      Aver.IsNotNull(got);
      Aver.AreEqual("Cat", got.Name);
      Aver.AreEqual(2, sut.EnqueuedCount);
      Aver.AreEqual(3, sut.EnqueuedSize);

      Aver.IsTrue(sut.TryDequeue(out got));
      Aver.IsNotNull(got);
      Aver.AreEqual("Dog", got.Name);
      Aver.AreEqual(1, sut.EnqueuedCount);
      Aver.AreEqual(0, sut.EnqueuedSize);

      Aver.IsTrue(sut.TryDequeue(out got));
      Aver.IsNull(got);
      Aver.AreEqual(0, sut.EnqueuedCount);
      Aver.AreEqual(0, sut.EnqueuedSize);

      //Rabbit was lost
      Aver.IsFalse(sut.TryPeek(out got));
      Aver.IsFalse(sut.TryDequeue(out got));
    }

    [Run]
    public void SetCountLimit_Throw()
    {
      var sut = new CappedQueue<MyItem>(i => i == null ? 0 : i.Name == null ? 0 : i.Name.Length);

      sut.CountLimit = 3;
      sut.Handling = QueueLimitHandling.Throw;

      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Cat" }));
      Aver.AreEqual(1, sut.Count);

      Aver.IsTrue(sut.TryEnqueue(new MyItem { Name = "Dog" }));
      Aver.AreEqual(2, sut.Count);

      Aver.IsTrue(sut.TryEnqueue(null));
      Aver.AreEqual(3, sut.Count);

      try
      {
        //Rabbit will not fit - will throw
        Aver.IsFalse(sut.TryEnqueue(new MyItem { Name = "Rabbit" }));
      }
      catch(AzosException error)
      {
        Aver.IsTrue(error.Message.Contains("limit is reached"));
        Conout.WriteLine("Expected and got: " + error.ToMessageWithType());
        return;
      }
      Aver.Fail(Constants.ERR_NOT_THROWN);
    }

  }
}
