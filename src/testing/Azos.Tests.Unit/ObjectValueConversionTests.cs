/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Unit
{
    public enum TestEnum{ A=0,B=123,C=234 }


    [Runnable(TRUN.BASE)]
    public class ObjectValueConversionTests
    {
        [Run]
        public void AsLaconicConfig()
        {
          Aver.AreEqual(23, "value=23".AsLaconicConfig().AttrByName("value").ValueAsInt());
          Aver.AreEqual(223, "nfx{value=223}".AsLaconicConfig().AttrByName("value").ValueAsInt());
          Aver.IsNull("nfx{value 223}".AsLaconicConfig());

          try
          {
            "nfx".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
            Aver.Fail("No exception");
          }
          catch
          {
            Aver.Pass("Got exception as expected");
          }
        }

        [Run]
        public void AsJSONConfig()
        {
          Aver.AreEqual(23, "{value: 23}".AsJSONConfig().AttrByName("value").ValueAsInt());
          Aver.AreEqual(223, "{nfx: {value: 223}}".AsJSONConfig().AttrByName("value").ValueAsInt());
          Aver.IsNull("{ bad }".AsJSONConfig());

          try
          {
            "bad".AsJSONConfig(handling: ConvertErrorHandling.Throw);
            Aver.Fail("No exception");
          }
          catch
          {
            Aver.Pass("Got exception as expected");
          }
        }

        [Run]
        public void FromInt()
        {
            object obj = 123;

            Aver.AreEqual(123, obj.AsShort());
            Aver.AreEqual(123, obj.AsNullableShort());

            Aver.AreEqual(123, obj.AsUShort());
            Aver.AreEqual(123, obj.AsNullableUShort());

            Aver.AreEqual(123, obj.AsInt());
            Aver.AreEqual(123, obj.AsNullableInt());

            Aver.AreEqual(123u, obj.AsUInt());
            Aver.AreEqual(123, obj.AsNullableUInt());

            Aver.AreEqual(123L, obj.AsLong());
            Aver.AreEqual(123L, obj.AsNullableLong());

            Aver.AreEqual(123UL, obj.AsULong());
            Aver.AreEqual(123UL, obj.AsNullableULong());

            Aver.AreEqual(123d, obj.AsDouble());
            Aver.AreEqual(123d, obj.AsNullableDouble());

            Aver.AreEqual(123f, obj.AsFloat());
            Aver.AreEqual(123f, obj.AsNullableFloat());

            Aver.AreEqual(123m, obj.AsDecimal());
            Aver.AreEqual(123m, obj.AsNullableDecimal());

            Aver.AreEqual(true, obj.AsBool());
            Aver.AreEqual(true, obj.AsNullableBool());

            Aver.AreEqual("123", obj.AsString());
            Aver.AreEqual(123, obj.AsDateTime().Ticks);
            Aver.AreEqual(123, obj.AsTimeSpan().Ticks);

            Aver.IsTrue( TestEnum.B == obj.AsEnum(TestEnum.A));

        }


        [Run]
        public void FromLong()
        {
            object obj = 123L;

            Aver.AreEqual(123, obj.AsShort());
            Aver.AreEqual(123, obj.AsNullableShort());

            Aver.AreEqual(123, obj.AsUShort());
            Aver.AreEqual(123, obj.AsNullableUShort());

            Aver.AreEqual(123, obj.AsInt());
            Aver.AreEqual(123, obj.AsNullableInt());

            Aver.AreEqual(123u, obj.AsUInt());
            Aver.AreEqual(123, obj.AsNullableUInt());

            Aver.AreEqual(123L, obj.AsLong());
            Aver.AreEqual(123L, obj.AsNullableLong());

            Aver.AreEqual(123UL, obj.AsULong());
            Aver.AreEqual(123UL, obj.AsNullableULong());

            Aver.AreEqual(123d, obj.AsDouble());
            Aver.AreEqual(123d, obj.AsNullableDouble());

            Aver.AreEqual(123f, obj.AsFloat());
            Aver.AreEqual(123f, obj.AsNullableFloat());

            Aver.AreEqual(123m, obj.AsDecimal());
            Aver.AreEqual(123m, obj.AsNullableDecimal());

            Aver.AreEqual(true, obj.AsBool());
            Aver.AreEqual(true, obj.AsNullableBool());

            Aver.AreEqual("123", obj.AsString());
            Aver.AreEqual(123, obj.AsDateTime().Ticks);
            Aver.AreEqual(123, obj.AsTimeSpan().Ticks);

            Aver.IsTrue(TestEnum.B == obj.AsEnum(TestEnum.A));

        }

        [Run]
        public void FromShort()
        {
            short s = 123;
            object obj = s;

            Aver.AreEqual(123, obj.AsShort());
            Aver.AreEqual(123, obj.AsNullableShort());

            Aver.AreEqual(123, obj.AsUShort());
            Aver.AreEqual(123, obj.AsNullableUShort());

            Aver.AreEqual(123, obj.AsInt());
            Aver.AreEqual(123, obj.AsNullableInt());

            Aver.AreEqual(123u, obj.AsUInt());
            Aver.AreEqual(123, obj.AsNullableUInt());

            Aver.AreEqual(123L, obj.AsLong());
            Aver.AreEqual(123L, obj.AsNullableLong());

            Aver.AreEqual(123UL, obj.AsULong());
            Aver.AreEqual(123UL, obj.AsNullableULong());

            Aver.AreEqual(123d, obj.AsDouble());
            Aver.AreEqual(123d, obj.AsNullableDouble());

            Aver.AreEqual(123f, obj.AsFloat());
            Aver.AreEqual(123f, obj.AsNullableFloat());

            Aver.AreEqual(123m, obj.AsDecimal());
            Aver.AreEqual(123m, obj.AsNullableDecimal());

            Aver.AreEqual(true, obj.AsBool());
            Aver.AreEqual(true, obj.AsNullableBool());

            Aver.AreEqual("123", obj.AsString());

            Aver.IsTrue( TestEnum.B == obj.AsEnum(TestEnum.A));

        }


        [Run]
        public void FromDouble()
        {
            object obj = 123d;

            Aver.AreEqual(123, obj.AsShort());
            Aver.AreEqual(123, obj.AsNullableShort());

            Aver.AreEqual(123, obj.AsUShort());
            Aver.AreEqual(123, obj.AsNullableUShort());

            Aver.AreEqual(123, obj.AsInt());
            Aver.AreEqual(123, obj.AsNullableInt());

            Aver.AreEqual(123u, obj.AsUInt());
            Aver.AreEqual(123, obj.AsNullableUInt());

            Aver.AreEqual(123L, obj.AsLong());
            Aver.AreEqual(123L, obj.AsNullableLong());

            Aver.AreEqual(123UL, obj.AsULong());
            Aver.AreEqual(123UL, obj.AsNullableULong());

            Aver.AreEqual(123d, obj.AsDouble());
            Aver.AreEqual(123d, obj.AsNullableDouble());

            Aver.AreEqual(123f, obj.AsFloat());
            Aver.AreEqual(123f, obj.AsNullableFloat());

            Aver.AreEqual(123m, obj.AsDecimal());
            Aver.AreEqual(123m, obj.AsNullableDecimal());

            Aver.AreEqual(true, obj.AsBool());
            Aver.AreEqual(true, obj.AsNullableBool());

            Aver.AreEqual("123", obj.AsString());
            Aver.AreEqual(123, obj.AsDateTime().Ticks);
            Aver.AreEqual(123, obj.AsTimeSpan().Ticks);

            Aver.IsTrue(TestEnum.B == obj.AsEnum(TestEnum.A));

        }

        [Run]
        public void FromDecimal()
        {
            object obj = 123m;

            Aver.AreEqual(123, obj.AsShort());
            Aver.AreEqual(123, obj.AsNullableShort());

            Aver.AreEqual(123, obj.AsUShort());
            Aver.AreEqual(123, obj.AsNullableUShort());

            Aver.AreEqual(123, obj.AsInt());
            Aver.AreEqual(123, obj.AsNullableInt());

            Aver.AreEqual(123u, obj.AsUInt());
            Aver.AreEqual(123, obj.AsNullableUInt());

            Aver.AreEqual(123L, obj.AsLong());
            Aver.AreEqual(123L, obj.AsNullableLong());

            Aver.AreEqual(123UL, obj.AsULong());
            Aver.AreEqual(123UL, obj.AsNullableULong());

            Aver.AreEqual(123d, obj.AsDouble());
            Aver.AreEqual(123d, obj.AsNullableDouble());

            Aver.AreEqual(123f, obj.AsFloat());
            Aver.AreEqual(123f, obj.AsNullableFloat());

            Aver.AreEqual(123m, obj.AsDecimal());
            Aver.AreEqual(123m, obj.AsNullableDecimal());

            Aver.AreEqual(true, obj.AsBool());
            Aver.AreEqual(true, obj.AsNullableBool());

            Aver.AreEqual("123", obj.AsString());
            Aver.AreEqual(123, obj.AsDateTime().Ticks);
            Aver.AreEqual(123, obj.AsTimeSpan().Ticks);

            Aver.IsTrue(TestEnum.B == obj.AsEnum(TestEnum.A));

        }

        [Run]
        public void FromString()
        {
            object obj = "123";

            Aver.AreEqual(123, obj.AsShort());
            Aver.AreEqual(123, obj.AsNullableShort());

            Aver.AreEqual(123, obj.AsUShort());
            Aver.AreEqual(123, obj.AsNullableUShort());

            Aver.AreEqual(123, obj.AsInt());
            Aver.AreEqual(123, obj.AsNullableInt());

            Aver.AreEqual(123, obj.AsInt());
            Aver.AreEqual(123, obj.AsNullableUInt());

            Aver.AreEqual(123L, obj.AsLong());
            Aver.AreEqual(123L, obj.AsNullableLong());

            Aver.AreEqual(123UL, obj.AsULong());
            Aver.AreEqual(123UL, obj.AsNullableULong());

            Aver.AreEqual(123d, obj.AsDouble());
            Aver.AreEqual(123d, obj.AsNullableDouble());

            Aver.AreEqual(123f, obj.AsFloat());
            Aver.AreEqual(123f, obj.AsNullableFloat());

            Aver.AreEqual(123m, obj.AsDecimal());
            Aver.AreEqual(123m, obj.AsNullableDecimal());

            Aver.AreEqual(true, obj.AsBool());
            Aver.AreEqual(true, obj.AsNullableBool());

            Aver.AreEqual("123", obj.AsString());
            Aver.AreEqual(123, obj.AsDateTime().Ticks);
            Aver.AreEqual(123, obj.AsTimeSpan().Ticks);

            Aver.IsTrue(TestEnum.B == obj.AsEnum(TestEnum.A));

        }


         [Run]
        public void FromBool()
        {
            object obj = true;

            Aver.AreEqual(1, obj.AsShort());
            Aver.AreEqual(1, obj.AsNullableShort());

            Aver.AreEqual(1, obj.AsInt());
            Aver.AreEqual(1, obj.AsNullableInt());

            Aver.AreEqual(1d, obj.AsDouble());
            Aver.AreEqual(1d, obj.AsNullableDouble());

            Aver.AreEqual(1f, obj.AsFloat());
            Aver.AreEqual(1f, obj.AsNullableFloat());

            Aver.AreEqual(1m, obj.AsDecimal());
            Aver.AreEqual(1m, obj.AsNullableDecimal());

            Aver.AreEqual(true, obj.AsBool());
            Aver.AreEqual(true, obj.AsNullableBool());

            Aver.AreEqual("True", obj.AsString());
            Aver.AreEqual(new DateTime(2001,1,1), obj.AsDateTime(new DateTime(2001,1,1)));
            Aver.AreEqual(1, obj.AsTimeSpan().Ticks);


        }


        [Run]
        public void FromNull()
        {
            object obj = null;

            Aver.AreEqual(0, obj.AsShort());
            Aver.AreEqual(null, obj.AsNullableShort());

            Aver.AreEqual(0, obj.AsUShort());
            Aver.AreEqual(null, obj.AsNullableUShort());

            Aver.AreEqual(0, obj.AsInt());
            Aver.AreEqual(null, obj.AsNullableInt());

            Aver.AreEqual(0u, obj.AsUInt());
            Aver.AreEqual(null, obj.AsNullableUInt());

            Aver.AreEqual(0L, obj.AsLong());
            Aver.AreEqual(null, obj.AsNullableLong());

            Aver.AreEqual(0UL, obj.AsULong());
            Aver.AreEqual(null, obj.AsNullableULong());

            Aver.AreEqual(0d, obj.AsDouble());
            Aver.AreEqual(null, obj.AsNullableDouble());

            Aver.AreEqual(0f, obj.AsFloat());
            Aver.AreEqual(null, obj.AsNullableFloat());

            Aver.AreEqual(0m, obj.AsDecimal());
            Aver.AreEqual(null, obj.AsNullableDecimal());

            Aver.AreEqual(false, obj.AsBool());
            Aver.AreEqual(null, obj.AsNullableBool());

            Aver.AreEqual(null, obj.AsString());
            Aver.AreEqual(null, obj.AsNullableDateTime());
            Aver.AreEqual(null, obj.AsNullableTimeSpan());

            Aver.IsNull(obj.AsNullableEnum<TestEnum>());

        }

        [Run]
        public void FromNullWithDifferentDefaults()
        {
            object obj = null;

            Aver.AreEqual(5, obj.AsShort(5));
            Aver.AreEqual(null, obj.AsNullableShort(5));

            Aver.AreEqual(5, obj.AsUShort(5));
            Aver.AreEqual(null, obj.AsNullableUShort(5));

            Aver.AreEqual(6, obj.AsInt(6));
            Aver.AreEqual(null, obj.AsNullableInt(6));

            Aver.AreEqual(6u, obj.AsUInt(6));
            Aver.AreEqual(null, obj.AsNullableUInt(6));

            Aver.AreEqual(6L, obj.AsLong(6L));
            Aver.AreEqual(null, obj.AsNullableLong(6L));

            Aver.AreEqual(6u, obj.AsULong(6));
            Aver.AreEqual(null, obj.AsNullableULong(6));

            Aver.AreEqual(7d, obj.AsDouble(7));
            Aver.AreEqual(null, obj.AsNullableDouble(7));

            Aver.AreEqual(8f, obj.AsFloat(8));
            Aver.AreEqual(null, obj.AsNullableFloat(8));

            Aver.AreEqual(9m, obj.AsDecimal(9));
            Aver.AreEqual(null, obj.AsNullableDecimal(9));

            Aver.AreEqual(true, obj.AsBool(true));
            Aver.AreEqual(null, obj.AsNullableBool(true));

            Aver.AreEqual("yez!", obj.AsString("yez!"));
            Aver.AreEqual(new DateTime(1921, 04,07), obj.AsDateTime(new DateTime(1921, 04,07)));
            Aver.AreEqual(null, obj.AsNullableDateTime(new DateTime(1921, 04,07)));
            Aver.AreEqual(TimeSpan.FromHours(12.5), obj.AsTimeSpan( TimeSpan.FromHours(12.5)));
            Aver.AreEqual(null, obj.AsNullableTimeSpan( TimeSpan.FromHours(12.5)));

            Aver.IsTrue( TestEnum.C == obj.AsEnum<TestEnum>(TestEnum.C));
            Aver.IsNull( obj.AsNullableEnum<TestEnum>(TestEnum.C));

        }


        [Run]
        public void Unsigned()
        {
            object obj = "127";

            Aver.AreEqual(127, obj.AsByte());
            Aver.AreEqual(127, obj.AsUShort());
            Aver.AreEqual(127u, obj.AsUInt());
            Aver.AreEqual(127ul, obj.AsULong());
        }

        [Run]
        public void GUID_1()
        {
            object obj = "{CF04F818-6194-48C3-B618-8965ACA4D229}";

            Aver.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsGUID(Guid.Empty));
        }

        [Run]
        public void GUID_2()
        {
            object obj = "CF04F818-6194-48C3-B618-8965ACA4D229";

            Aver.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsGUID(Guid.Empty));
        }

        [Run]
        public void GUID_3()
        {
            object obj = "sfsadfsd fsdafsd CF04F818-6194-48C3-B618-8965ACA4D229";

            Aver.AreEqual(Guid.Empty, obj.AsGUID(Guid.Empty));
        }

        [Run]
        public void GUID_4()
        {
            object obj = "CF04F818619448C3B6188965ACA4D229";

            Aver.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsGUID(Guid.Empty));
        }


        [Run]
        public void GUID_5()
        {
            object obj = new Guid("CF04F818-6194-48C3-B618-8965ACA4D229");

            Aver.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsGUID(Guid.Empty));
        }


        [Run]
        public void NullableGUID_1()
        {
            object obj = "{CF04F818-6194-48C3-B618-8965ACA4D229}";

            Aver.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsNullableGUID());
        }

        [Run]
        public void NullableGUID_2()
        {
            object obj = "CF04F818-6194-48C3-B618-8965ACA4D229";

            Aver.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsNullableGUID());
        }

        [Run]
        public void NullableGUID_3()
        {
            object obj = "sfsadfsd fsdafsd CF04F818-6194-48C3-B618-8965ACA4D229";

            Aver.IsNull(obj.AsNullableGUID());
        }

        [Run]
        public void NullableGUID_4()
        {
            object obj = "sfsadfsd fsdafsd CF04F818-6194-48C3-B618-8965ACA4D229";

            Aver.AreEqual(Guid.Empty, obj.AsNullableGUID(Guid.Empty));
        }

        [Run]
        public void NullableGUID_5()
        {
            object obj = "CF04F818619448C3B6188965ACA4D229";

            Aver.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsNullableGUID());
        }

        [Run]
        public void NullableGUID_6()
        {
            object obj = new Guid("CF04F818-6194-48C3-B618-8965ACA4D229");

            Aver.AreEqual(new Guid("CF04F818-6194-48C3-B618-8965ACA4D229"), obj.AsNullableGUID());
        }


        [Run]
        public void GDID()
        {
            object obj = new GDID(3,4,5);

            Aver.AreObjectsEqual(obj, obj.AsGDID());
            Aver.AreObjectsEqual(obj, obj.AsGDID(new GDID(2,3,4)));


            Aver.AreObjectsEqual(obj, "3:4:5".AsGDID(new GDID(2,3,4)));
            Aver.AreEqual(new GDID(2,3,4), "3rewtfef:4:5".AsGDID(new GDID(2,3,4)));

            try
            {
              "3rewtfef:4:5".AsGDID(new GDID(2,3,4), handling: ConvertErrorHandling.Throw);
              Aver.Fail("No execpetion");
            }
            catch
            {
              Aver.Pass("");
            }
        }

        [Run]
        public void NullableGDID()
        {
            object obj = new GDID(3,4,5);

            Aver.AreObjectsEqual(obj, obj.AsNullableGDID());
            Aver.AreObjectsEqual(obj, obj.AsNullableGDID(new GDID(2,3,4)));


            Aver.AreObjectsEqual(obj, "3:4:5".AsNullableGDID(new GDID(2,3,4)));
            object on = null;
            Aver.IsNull( on.AsNullableGDID() );

            Aver.IsNull( on.AsNullableGDID(new GDID(3,4,5)));

            Aver.AreObjectsEqual(obj, "fdwsfsdfds".AsNullableGDID(new GDID(3,4,5)));
        }


        [Run]
        public void GDIDSymbol()
        {
            object obj = new GDIDSymbol(new GDID(3,4,5), "ABC");

            Aver.AreObjectsEqual(obj, obj.AsGDIDSymbol());
            Aver.AreObjectsEqual(obj, obj.AsGDIDSymbol(new GDIDSymbol(new GDID(23,14,15), "ABC")));


            var link = new ELink(new GDID(4,12,8721));


            Aver.AreEqual(link.AsGDIDSymbol, link.Link.AsGDIDSymbol());
            Aver.AreEqual(link.AsGDIDSymbol, "3rewtfef:4:5".AsGDIDSymbol(link.AsGDIDSymbol()));

            try
            {
              "3rewtfef:4:5".AsGDIDSymbol(link.AsGDIDSymbol, handling: ConvertErrorHandling.Throw);
              Aver.Fail("No excepetion");
            }
            catch
            {
              Aver.Pass();
            }
        }


        [Run]
        public void NullableGDIDSymbol()
        {
            var obj = new GDIDSymbol(new GDID(3,4,5), "3:4:5");

            Aver.AreEqual(obj, obj.AsNullableGDIDSymbol());
            Aver.AreEqual(obj, obj.AsNullableGDIDSymbol(new GDIDSymbol(new GDID(13,14,15), "ABC")));


            Aver.AreEqual(obj, "3:4:5".AsNullableGDIDSymbol(new GDIDSymbol(new GDID(13,14,15), "ABC")));
            object on = null;
            Aver.IsNull( on.AsNullableGDIDSymbol() );

            Aver.IsNull( on.AsNullableGDIDSymbol(obj));

            Aver.AreEqual(obj, "fdwsfsdfds".AsNullableGDIDSymbol(obj));
        }

        [Run]
        public void Uri()
        {
          Aver.IsNull(new { _ = "" }.AsUri());

          Aver.Throws<AzosException>(() => Azos.Data.GDID.Zero.AsUri(handling: ConvertErrorHandling.Throw), "GDID.AsUri");

          object obj = "https://example.com";

          Aver.AreObjectsEqual(new Uri("https://example.com"), obj.AsUri());
        }
    }
}
