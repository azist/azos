/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.IO;

namespace Azos.Serialization.Slim
{
    internal static class Arrays
    {
          /// <summary>
          /// Maximum number of supported array dimensions.
          /// Used for possible stream corruption detection
          /// </summary>
          public const int MAX_DIM_COUNT = 37;

          /// <summary>
          /// Maximum number of elements in any array.
          /// Used for possible stream corruption detection
          /// </summary>
          public const int MAX_ELM_COUNT = MAX_DIM_COUNT * 10 * 1024 * 1024;

           public static string ArrayToDescriptor(Array array, Type type, VarIntStr typeHandle)
           {
              if (array.LongLength>MAX_ELM_COUNT)
                throw new SlimSerializationException(StringConsts.SLIM_ARRAYS_OVER_MAX_ELM_ERROR.Args(array.LongLength, MAX_ELM_COUNT));

              if (type==typeof(object[]))//special case for object[], because this type is very often used in Glue and other places
               return "$2|"+array.Length.ToString();


              var th = typeHandle.StringValue ??
                      ( typeHandle.IntValue < TypeRegistry.STR_HNDL_POOL.Length ?
                                TypeRegistry.STR_HNDL_POOL[typeHandle.IntValue] :
                                '$'+typeHandle.IntValue.ToString()
                      );

               var ar = array.Rank;
               if (ar>MAX_DIM_COUNT)
                throw new SlimSerializationException(StringConsts.SLIM_ARRAYS_OVER_MAX_DIMS_ERROR.Args(ar, MAX_DIM_COUNT));


               var descr = new StringBuilder();
               descr.Append( th );
               descr.Append('|');//separator char

               for(int i=0; i<ar; i++)
               {
                  descr.Append(array.GetLowerBound(i));
                  descr.Append('~');
                  descr.Append(array.GetUpperBound(i));
                  if (i<ar-1)
                   descr.Append(',');
               }

              return descr.ToString();
           }

      //20140702 DLat+Dkh parsing speed optimization
      public static Array DescriptorToArray(string descr, Type type)
      {
          var i = descr.IndexOf('|');

          if (i<2)
          {
            throw new SlimDeserializationException(StringConsts.SLIM_ARRAYS_MISSING_ARRAY_DIMS_ERROR + descr.ToString());
          }

          if (i==2 && descr[1]=='2' && descr[0]=='$')//object[] case: $2|len
          {
            i++;//over |
            var total = quickParseInt(descr, ref i, descr.Length);
            if (total>MAX_ELM_COUNT)
              throw new SlimDeserializationException(StringConsts.SLIM_ARRAYS_OVER_MAX_ELM_ERROR.Args(total, MAX_ELM_COUNT));

            return new object[total];
          }



          Array instance = null;

          if (!type.IsArray)
            throw new SlimDeserializationException(StringConsts.SLIM_ARRAYS_TYPE_NOT_ARRAY_ERROR + type.FullName);

          i++;//over |
          var len = descr.Length;
          //descr = $0|0~12,1~100
          //           ^

          try
          {
              var dimCount = 1;
              for(var j=i;j<len-1;j++) if (descr[j]==',') dimCount++;

              if (dimCount>MAX_DIM_COUNT)
                throw new SlimDeserializationException(StringConsts.SLIM_ARRAYS_OVER_MAX_DIMS_ERROR.Args(dimCount, MAX_DIM_COUNT));

              int[] lengths = new int[dimCount];
              int[] lowerBounds = new int[dimCount];

              long total = 0;
              for(int dim=0; dim<dimCount; dim++)
              {
                var lb = quickParseInt(descr, ref i, len);
                var ub = quickParseInt(descr, ref i, len);

                var onelen = (ub-lb)+1;
                lengths[dim] = onelen;
                lowerBounds[dim] = lb;
                total+=onelen;
              }

             if (total>MAX_ELM_COUNT)
                throw new SlimDeserializationException(StringConsts.SLIM_ARRAYS_OVER_MAX_ELM_ERROR.Args(total, MAX_ELM_COUNT));

              instance = Array.CreateInstance(type.GetElementType(), lengths, lowerBounds);
          }
          catch(Exception error)
          {
             throw new SlimDeserializationException(StringConsts.SLIM_ARRAYS_ARRAY_INSTANCE_ERROR + descr.ToString()+"': "+error.Message, error);
          }

          return instance;
      }

                private static int quickParseInt(string str, ref int i, int len)
                {
                  int result = 0;
                  bool pos = true;
                  for(; i<len; i++)
                  {
                    var c = str[i];
                    if (c=='-')
                    {
                      pos = false;
                      continue;
                    }
                    if (c=='~' || c==',')
                    {
                      i++;
                      return pos ? result : -result;
                    }
                    var d = c - '0';
                    if (d<0 || d>9) throw new SlimDeserializationException(StringConsts.SLIM_ARRAYS_WRONG_ARRAY_DIMS_ERROR + str);
                    result *= 10;
                    result += d;
                  }

                  return pos ? result : -result;
                }

    }
}
