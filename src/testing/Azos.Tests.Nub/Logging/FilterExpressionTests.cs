/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Log;
using Azos.Log.Filters;
using Azos.Scripting;

namespace Azos.Tests.Nub.Logging
{
  [Runnable]
  public class FilterExpressionTests
  {
    [Run("1", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.Or'
        left{ type='Azos.Log.Filters.ByTopic' include='apple'}
        right{ type='Azos.Log.Filters.ByTopic' include='doesnt-exist'}
      }
    }")]

    [Run("2", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.Or'
        left{ type='Azos.Log.Filters.ByTopic' include='shake;bake;cake'}
        right{ type='Azos.Log.Filters.ByTopic' include='grapes;  apple ;tea  '}
      }
    }")]

    [Run("3", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.And'
        left{ type='Azos.Log.Filters.ByTopic' include='apple'}
        right{ type='Azos.Log.Filters.ByTopic' exclude='grapes'}
      }
    }")]

    [Run("4", @"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.And'
        left{ type='Azos.Log.Filters.ByTopic' include='apple'}
        right{ type='Azos.Log.Filters.ByTopic' exclude='apple'}
      }
    }")]

    [Run("5", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.And'
        left{ type='Azos.Log.Filters.ByTopic' include='gnosis;prognosis;window*;gyugiuy*;*pple' exclude='sound;left;right;dsfsd*a;shiuhiu*;duhiuhiu*;hiuhi*f;gjnknjn;jhkjhk*h;hggjg*j;hhjlk*hhjh*jkj'}
        right{ type='Azos.Log.Filters.ByTopic' exclude='souend;lwereft;riwerght;dswer'}
      }
    }")]

    [Run("6", @"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.And'
        left{ type='Azos.Log.Filters.ByTopic' include='gnosis;prognosis;window*;gyugiuy*;*pple' exclude='sound;left;right;dsfsd*a;shiuhiu*;  appl*  ;duhiuhiu*;hiuhi*f;gjnknjn;jhkjhk*h;hggjg*j;hhjlk*hhjh*jkj'}
        right{ type='Azos.Log.Filters.ByTopic' exclude='souend;lwereft;riwerght;dswer'}
      }
    }")]

    [Run("7", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.And'
        left{ type='Azos.Log.Filters.ByTopic' include='baby;apple' exclude='*-left; right-*'}
        right{ type='Azos.Log.Filters.ByFrom' exclude='*records*' include='Hyd*' case=true}
      }
    }")]

    [Run("8", @"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.And'
        left{ type='Azos.Log.Filters.ByTopic' include='baby;apple' exclude='*-left; right-*'}
        right{ type='Azos.Log.Filters.ByFrom' exclude='*records*' include='Hyd*' case=false}
      }
    }")]

    [Run("9", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.And'
        left{ type='Azos.Log.Filters.ByTopic' include='apple' exclude='Apple' case=true}
        right
        {
         type='Azos.Log.Filters.And'
         left{ type='Azos.Log.Filters.ByFrom' exclude='*Lenin*' include='Hydra*' }
         right{ type='Azos.Log.Filters.ByText' include='Lenin*' }
        }
      }
    }")]

    [Run("10", @"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.Not'
        operand
        {
          type='Azos.Log.Filters.And'
          left{ type='Azos.Log.Filters.ByTopic' include='apple' exclude='Apple' case=true}
          right
          {
           type='Azos.Log.Filters.And'
           left{ type='Azos.Log.Filters.ByFrom' exclude='*Lenin*' include='Hydra*' }
           right{ type='Azos.Log.Filters.ByText' include='Lenin*' }
          }
        }//operand
      }//tree
    }")]

    [Run("11", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByTopic' include='APPLE'
      }
    }")]

    [Run("12", @"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByTopic' include='APPLE' case=true
      }
    }")]

    [Run("13", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByFrom' include='HyDra.REcorDS'
      }
    }")]

    [Run("14", @"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByFrom' include='HyDra.REcorDS' case=true
      }
    }")]

    [Run("15", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByText' include='LENIN*'
      }
    }")]

    [Run("16", @"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByText' include='LENIN*' case=true
      }
    }")]

    [Run("17", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByHost' include='CLEV*'
      }
    }")]

    [Run("18", @"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByHost' include='CLEV*' case=true
      }
    }")]

    [Run("19", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByChannel' include='meduza'
      }
    }")]

    [Run("20", @"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByChannel' include='MEDUZA'
      }
    }")]

    [Run("21", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByApp'
        include='DUD'
      }
    }")]

    [Run("22", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByApp' include='dud' case=true
      }
    }")]

    [Run("23", @"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.ByApp' include='DuD' case=true
      }
    }")]

    [Run("24", @"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.And'
        left
        {
          type='Azos.Log.Filters.And'
          left{ type='Azos.Log.Filters.ByApp' exclude='DUD' case=true}
          right
          {
            type='Azos.Log.Filters.And'
            left{ type='Azos.Log.Filters.ByApp' include='DUD' case=false}
            right
            {
              type='Azos.Log.Filters.And'
              left{ type='Azos.Log.Filters.Not' operand{ type='Azos.Log.Filters.ByChannel' include='NOTTHERE'} }
              right
              {
                type='Azos.Log.Filters.Xor'
                left
                {
                  type='Azos.Log.Filters.ByText' include='*loved*' exclude='*titanic*'
                }
                right
                {
                  type='Azos.Log.Filters.ByText' exclude='*loved*'
                }
              }
            }
          }
          right
          {
            type='Azos.Log.Filters.ByText' exclude='*.AAAAAAAAAAAAAAAAA'
          }
        }
        right
        {
          type='Azos.Log.Filters.ByHost' include='*.com'
        }
      }
    }")]

    [Run("25", @"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.And'
        left
        {
          type='Azos.Log.Filters.And'
          left{ type='Azos.Log.Filters.ByApp' exclude='DUD' case=true}
          right
          {
            type='Azos.Log.Filters.And'
            left{ type='Azos.Log.Filters.ByApp' include='DUD' case=false}
            right
            {
              type='Azos.Log.Filters.And'
              left{ type='Azos.Log.Filters.Not' operand{ type='Azos.Log.Filters.ByChannel' include='NOTTHERE'} }
              right
              {
                type='Azos.Log.Filters.Xor'
                left
                {
                  type='Azos.Log.Filters.ByText' include='*loved*' exclude='*titanic*'
                }
                right
                {
                  type='Azos.Log.Filters.ByText' include='*loved*'   // xor true ^ true
                }
              }
            }
          }
          right
          {
            type='Azos.Log.Filters.ByText' exclude='*.AAAAAAAAAAAAAAAAA'
          }
        }
        right
        {
          type='Azos.Log.Filters.ByHost' include='*.com'
        }
      }
    }")]

    [Run("26", @"
    expect=true
    def
    {
      type-path='Azos.Log.Filters, Azos'
      tree
      {
        type='And'
        left
        {
          type='And'
          left{ type='ByApp' exclude='DUD' case=true}
          right
          {
            type='And'
            left{ type='ByApp' include='DUD' case=false}
            right
            {
              type='And'
              left{ type='Not' operand{ type='Azos.Log.Filters.ByChannel' include='NOTTHERE'} }
              right
              {
                type='Azos.Log.Filters.Xor'
                left
                {
                  type='ByText' include='*loved*' exclude='*titanic*'
                }
                right
                {
                  type='Azos.Log.Filters.ByText' exclude='*loved*'
                }
              }
            }
          }
          right
          {
            type='Azos.Log.Filters.ByText' exclude='*.AAAAAAAAAAAAAAAAA'
          }
        }
        right
        {
          type='ByHost' include='*.com'
        }
      }
    }")]
    public void FromConf(IConfigSectionNode def, bool expect)
    {
     // def.See();

      var filter = FactoryUtils.MakeAndConfigure<LogMessageFilter>(def, typeof(LogMessageFilter));
      var msg = new Message
      {
        Topic = "apple",
        From = "Hydra.Records",
        Text = "Lenin loved mushrooms",
        Host = "cleveland.com",
        Channel = Atom.Encode("meduza"),
        App = Atom.Encode("dud")
      };

      //////var cnt = 500_000;
      //////var time = Azos.Time.Timeter.StartNew();
      //////for (var i = 0; i < cnt; i++)
      //////{
        var got = filter.Evaluate(msg);
        Aver.AreEqual(expect, got);
      //////}
      //////"Rate {0:n0} ops/sec".Args(cnt / (time.ElapsedMs / 1000d)).See();
    }
  }
}
