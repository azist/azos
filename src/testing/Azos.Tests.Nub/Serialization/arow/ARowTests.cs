/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Azos.Data;
using Azos.IO;
using Azos.Scripting;
using Azos.Serialization.Arow;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class ARowTests : IRunnableHook
  {
    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      ArowSerializer.RegisterTypeSerializationCores( Assembly.GetExecutingAssembly() );
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error) => false;


    [Run]
    public void SerDeser_OneSimplePerson()
    {
      var row1 = new SimplePersonRow
      {
          Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
      };
      var writer = SlimFormat.Instance.GetWritingStreamer();
      var reader = SlimFormat.Instance.GetReadingStreamer();
      using(var ms = new MemoryStream())
      {
          writer.BindStream(ms);
          ArowSerializer.Serialize(row1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var row2 = new SimplePersonRow();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(row2, reader);
          reader.UnbindStream();

          Aver.AreEqual(row1.ID,    row2.ID);
          Aver.AreEqual(row1.Bool1, row2.Bool1);
          Aver.AreEqual(row1.Name,  row2.Name);
          Aver.AreEqual(row1.Age,   row2.Age);
          Aver.AreEqual(row1.Salary,row2.Salary);
          Aver.AreEqual(row1.Str1,  row2.Str1);
          Aver.AreEqual(row1.Date,  row2.Date);
      }
    }

    [Run]
    public void SerDeserSubarray_OneSimplePerson()
    {
      var row1 = new SimplePersonRow
      {
        Age = 123,
        Bool1 = true,
        ID = new GDID(12, 234),
        Name = "Jacques Shiraquez",
        Salary = 143098,
        Str1 = "Zoloto",
        Date = new DateTime(1980, 08, 12, 13, 45, 11)
      };

      var data = ArowSerializer.SerializeToSubarray(row1);


      var row2 = new SimplePersonRow();
      ArowSerializer.Deserialize(row2, data.Array, 0);

      Aver.AreEqual(row1.ID, row2.ID);
      Aver.AreEqual(row1.Bool1, row2.Bool1);
      Aver.AreEqual(row1.Name, row2.Name);
      Aver.AreEqual(row1.Age, row2.Age);
      Aver.AreEqual(row1.Salary, row2.Salary);
      Aver.AreEqual(row1.Str1, row2.Str1);
      Aver.AreEqual(row1.Date, row2.Date);
    }

    [Run]
    public void SerDeserSubarray_OneSimplePersonWithByteArray_Small()
    {
      var row1 = new SimplePersonWithByteArrayRow
      {
        Age = 123,
        Bool1 = true,
        ID = new GDID(12, 234),
        Name = "Jacques Mudakes",
        Salary = 143098,
        Str1 = "Night Flower",
        Date = new DateTime(1980, 08, 12, 13, 45, 11),
        Buffer = new byte[]{1,7,9}
      };

      var data = ArowSerializer.SerializeToSubarray(row1);


      var row2 = new SimplePersonWithByteArrayRow();
      ArowSerializer.Deserialize(row2, data.Array, 0);

      Aver.AreEqual(row1.ID, row2.ID);
      Aver.AreEqual(row1.Bool1, row2.Bool1);
      Aver.AreEqual(row1.Name, row2.Name);
      Aver.AreEqual(row1.Age, row2.Age);
      Aver.AreEqual(row1.Salary, row2.Salary);
      Aver.AreEqual(row1.Str1, row2.Str1);
      Aver.AreEqual(row1.Date, row2.Date);
      Aver.AreArraysEquivalent(row1.Buffer, row2.Buffer);
    }

    [Run]
    public void SerDeserSubarray_OneSimplePersonWithByteArray_Large()
    {
      var row1 = new SimplePersonWithByteArrayRow
      {
        Age = 123,
        Bool1 = true,
        ID = new GDID(12, 234),
        Name = "Jacques Marazmos",
        Salary = 143098,
        Str1 = "Drunk Clowns on a hill",
        Date = new DateTime(1980, 08, 12, 13, 45, 11),
        Buffer = new byte[1024*1024] //large buffer which will out-grow buffer cache
      };

      var data = ArowSerializer.SerializeToSubarray(row1);


      var row2 = new SimplePersonWithByteArrayRow();
      ArowSerializer.Deserialize(row2, data.Array, 0);

      Aver.AreEqual(row1.ID, row2.ID);
      Aver.AreEqual(row1.Bool1, row2.Bool1);
      Aver.AreEqual(row1.Name, row2.Name);
      Aver.AreEqual(row1.Age, row2.Age);
      Aver.AreEqual(row1.Salary, row2.Salary);
      Aver.AreEqual(row1.Str1, row2.Str1);
      Aver.AreEqual(row1.Date, row2.Date);
      Aver.AreArraysEquivalent(row1.Buffer, row2.Buffer);

      //================== Try to reuse the serializer cache with smaller payload

      var row1_2 = new SimplePersonWithByteArrayRow
      {
        Age = 321,
        Bool1 = true,
        ID = new GDID(112, 243234),
        Name = "Ghabar Singh!",
        Salary = 13498,
        Str1 = "Hungry Moon",
        Date = new DateTime(1990, 08, 12, 13, 45, 11),
        Buffer = new byte[] { 21, 7, 9 }
      };

      var data_2 = ArowSerializer.SerializeToSubarray(row1_2);


      var row2_2 = new SimplePersonWithByteArrayRow();
      ArowSerializer.Deserialize(row2_2, data.Array, 0);

      Aver.AreEqual(row1_2.ID, row2_2.ID);
      Aver.AreEqual(row1_2.Bool1, row2_2.Bool1);
      Aver.AreEqual(row1_2.Name, row2_2.Name);
      Aver.AreEqual(row1_2.Age, row2_2.Age);
      Aver.AreEqual(row1_2.Salary, row2_2.Salary);
      Aver.AreEqual(row1_2.Str1, row2_2.Str1);
      Aver.AreEqual(row1_2.Date, row2_2.Date);
      Aver.AreArraysEquivalent(row1_2.Buffer, row2_2.Buffer);

    }

    [Run]
    public void SerDeser_OneSimplePersonWithEnum()
    {
      var row1 = new SimplePersonWithEnumRow
      {
          Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11),
          Married = SimplePersonWithEnumRow.MaritalStatus.Alien
      };
      var writer = SlimFormat.Instance.GetWritingStreamer();
      var reader = SlimFormat.Instance.GetReadingStreamer();
      using(var ms = new MemoryStream())
      {
          writer.BindStream(ms);
          ArowSerializer.Serialize(row1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var row2 = new SimplePersonWithEnumRow();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(row2, reader);
          reader.UnbindStream();

          Aver.AreEqual(row1.ID,    row2.ID);
          Aver.AreEqual(row1.Bool1, row2.Bool1);
          Aver.AreEqual(row1.Name,  row2.Name);
          Aver.AreEqual(row1.Age,   row2.Age);
          Aver.AreEqual(row1.Salary,row2.Salary);
          Aver.AreEqual(row1.Str1,  row2.Str1);
          Aver.AreEqual(row1.Date,  row2.Date);
          Aver.IsTrue(row1.Married == row2.Married);
      }
    }


    [Run]
    public void SerDeser_FamilyRow_1_NoReferences()
    {
      var row1 = new FamilyRow
      {
        ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

        //Father = new SimplePersonRow
        //{
        //  Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
        //}
      };

      var writer = SlimFormat.Instance.GetWritingStreamer();
      var reader = SlimFormat.Instance.GetReadingStreamer();
      using(var ms = new MemoryStream())
      {
          writer.BindStream(ms);
          ArowSerializer.Serialize(row1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var row2 = new FamilyRow();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(row2, reader);
          reader.UnbindStream();

          Aver.AreEqual(row1.ID,    row2.ID);
          Aver.AreEqual(row1.Name, row2.Name);
      }
    }

    [Run]
    public void SerDeser_FamilyRow_2_OneFieldRef()
    {
      var row1 = new FamilyRow
      {
        ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

        Father = new SimplePersonRow
        {
          Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
        }
      };

      var writer = SlimFormat.Instance.GetWritingStreamer();
      var reader = SlimFormat.Instance.GetReadingStreamer();
      using(var ms = new MemoryStream())
      {
          writer.BindStream(ms);
          ArowSerializer.Serialize(row1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var row2 = new FamilyRow();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(row2, reader);
          reader.UnbindStream();

          Aver.AreNotSameRef(row1, row2);
          Aver.AreEqual(row1.ID,    row2.ID);
          Aver.AreEqual(row1.Name, row2.Name);
          Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
          Aver.IsNotNull( row2.Father );
          Aver.AreEqual(row1.Father.ID, row2.Father.ID);
          Aver.AreEqual(row1.Father.Age, row2.Father.Age);
          Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

          Aver.AreEqual(row1.Father.Date, row2.Father.Date);
          Aver.IsNull(row2.Father.Str2);

          Aver.IsNull(row2.Mother);
      }
    }


    [Run]
    public void SerDeser_FamilyRow_3_TwoFieldRef()
    {
      var row1 = new FamilyRow
      {
        ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

        Father = new SimplePersonRow
        {
          Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
        },
        Mother = new SimplePersonRow
        {
          Age = 245, Bool1 =false, ID = new GDID(2,12), Name = "Katya Zhaba", Salary=180000, Str1="Snake", Str2="Borra", Date = new DateTime(1911, 01, 01, 14, 11, 07)
        }
      };

      var writer = SlimFormat.Instance.GetWritingStreamer();
      var reader = SlimFormat.Instance.GetReadingStreamer();
      using(var ms = new MemoryStream())
      {
          writer.BindStream(ms);
          ArowSerializer.Serialize(row1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var row2 = new FamilyRow();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(row2, reader);
          reader.UnbindStream();

          Aver.AreNotSameRef(row1, row2);
          Aver.AreEqual(row1.ID,    row2.ID);
          Aver.AreEqual(row1.Name, row2.Name);
          Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
          Aver.IsNotNull( row2.Father );
          Aver.AreEqual(row1.Father.ID, row2.Father.ID);
          Aver.AreEqual(row1.Father.Age, row2.Father.Age);
          Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

          Aver.AreEqual(row1.Father.Date, row2.Father.Date);
          Aver.IsNull(row2.Father.Str2);

          Aver.IsNotNull(row2.Mother);
          Aver.AreEqual(row1.Mother.ID, row2.Mother.ID);
          Aver.AreEqual(row1.Mother.Age, row2.Mother.Age);
          Aver.AreEqual(row1.Mother.Str1, row2.Mother.Str1);
          Aver.IsNotNull(row2.Mother.Str2);
          Aver.AreEqual(row1.Mother.Str2, row2.Mother.Str2);
          Aver.AreEqual(row1.Mother.Date, row2.Mother.Date);
      }
    }

    [Run]
    public void SerDeser_FamilyRow_4_EmptyArray()
    {
      var row1 = new FamilyRow
      {
        ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

        Father = new SimplePersonRow
        {
          Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
        },
        Mother = new SimplePersonRow
        {
          Age = 245, Bool1 =false, ID = new GDID(2,12), Name = "Katya Zhaba", Salary=180000, Str1="Snake", Str2="Borra", Date = new DateTime(1911, 01, 01, 14, 11, 07)
        },
        Brothers = new SimplePersonRow[0],
      };

      var writer = SlimFormat.Instance.GetWritingStreamer();
      var reader = SlimFormat.Instance.GetReadingStreamer();
      using(var ms = new MemoryStream())
      {
          writer.BindStream(ms);
          ArowSerializer.Serialize(row1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var row2 = new FamilyRow();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(row2, reader);
          reader.UnbindStream();

          Aver.AreNotSameRef(row1, row2);
          Aver.AreEqual(row1.ID,    row2.ID);
          Aver.AreEqual(row1.Name, row2.Name);
          Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
          Aver.IsNotNull( row2.Father );
          Aver.AreEqual(row1.Father.ID, row2.Father.ID);
          Aver.AreEqual(row1.Father.Age, row2.Father.Age);
          Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

          Aver.AreEqual(row1.Father.Date, row2.Father.Date);
          Aver.IsNull(row2.Father.Str2);

          Aver.IsNotNull(row2.Mother);
          Aver.AreEqual(row1.Mother.ID, row2.Mother.ID);
          Aver.AreEqual(row1.Mother.Age, row2.Mother.Age);
          Aver.AreEqual(row1.Mother.Str1, row2.Mother.Str1);
          Aver.IsNotNull(row2.Mother.Str2);
          Aver.AreEqual(row1.Mother.Str2, row2.Mother.Str2);
          Aver.AreEqual(row1.Mother.Date, row2.Mother.Date);

          Aver.IsNotNull(row2.Brothers);
          Aver.AreEqual(0, row2.Brothers.Length);
          Aver.IsNull(row2.Sisters);
          Aver.IsNull(row2.Advisers);
      }
    }

    [Run]
    public void SerDeser_FamilyRow_5_OneArrayFilled()
    {
      var row1 = new FamilyRow
      {
        ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

        Father = new SimplePersonRow
        {
          Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
        },
        Mother = new SimplePersonRow
        {
          Age = 245, Bool1 =false, ID = new GDID(2,12), Name = "Katya Zhaba", Salary=180000, Str1="Snake", Str2="Borra", Date = new DateTime(1911, 01, 01, 14, 11, 07)
        },
        Brothers = new []{ new SimplePersonRow{Age=111}, new SimplePersonRow{Age=222}, new SimplePersonRow{Age=333}}
      };

      var writer = SlimFormat.Instance.GetWritingStreamer();
      var reader = SlimFormat.Instance.GetReadingStreamer();
      using(var ms = new MemoryStream())
      {
          writer.BindStream(ms);
          ArowSerializer.Serialize(row1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var row2 = new FamilyRow();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(row2, reader);
          reader.UnbindStream();

          Aver.AreNotSameRef(row1, row2);
          Aver.AreEqual(row1.ID,    row2.ID);
          Aver.AreEqual(row1.Name, row2.Name);
          Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
          Aver.IsNotNull( row2.Father );
          Aver.AreEqual(row1.Father.ID, row2.Father.ID);
          Aver.AreEqual(row1.Father.Age, row2.Father.Age);
          Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

          Aver.AreEqual(row1.Father.Date, row2.Father.Date);
          Aver.IsNull(row2.Father.Str2);

          Aver.IsNotNull(row2.Mother);
          Aver.AreEqual(row1.Mother.ID, row2.Mother.ID);
          Aver.AreEqual(row1.Mother.Age, row2.Mother.Age);
          Aver.AreEqual(row1.Mother.Str1, row2.Mother.Str1);
          Aver.IsNotNull(row2.Mother.Str2);
          Aver.AreEqual(row1.Mother.Str2, row2.Mother.Str2);
          Aver.AreEqual(row1.Mother.Date, row2.Mother.Date);

          Aver.IsNotNull(row2.Brothers);
          Aver.AreEqual(3, row2.Brothers.Length);
          Aver.AreEqual(111, row2.Brothers[0].Age);
          Aver.AreEqual(222, row2.Brothers[1].Age);
          Aver.AreEqual(333, row2.Brothers[2].Age);
          Aver.IsNull( row2.Sisters);
          Aver.IsNull(row2.Advisers);
      }
    }

    [Run]
    public void SerDeser_FamilyRow_6_TwoArrayFilled()
    {
      var row1 = new FamilyRow
      {
        ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

        Father = new SimplePersonRow
        {
          Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
        },
        Mother = new SimplePersonRow
        {
          Age = 245, Bool1 =false, ID = new GDID(2,12), Name = "Katya Zhaba", Salary=180000, Str1="Snake", Str2="Borra", Date = new DateTime(1911, 01, 01, 14, 11, 07)
        },
        Brothers = new []{ new SimplePersonRow{Age=111}, new SimplePersonRow{Age=222}, new SimplePersonRow{Age=333}},
        Sisters = new []{ new SimplePersonRow{Age=12}, new SimplePersonRow{Age=13}}
      };

      var writer = SlimFormat.Instance.GetWritingStreamer();
      var reader = SlimFormat.Instance.GetReadingStreamer();
      using(var ms = new MemoryStream())
      {
          writer.BindStream(ms);
          ArowSerializer.Serialize(row1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var row2 = new FamilyRow();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(row2, reader);
          reader.UnbindStream();

          Aver.AreNotSameRef(row1, row2);
          Aver.AreEqual(row1.ID,    row2.ID);
          Aver.AreEqual(row1.Name, row2.Name);
          Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
          Aver.IsNotNull( row2.Father );
          Aver.AreEqual(row1.Father.ID, row2.Father.ID);
          Aver.AreEqual(row1.Father.Age, row2.Father.Age);
          Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

          Aver.AreEqual(row1.Father.Date, row2.Father.Date);
          Aver.IsNull(row2.Father.Str2);

          Aver.IsNotNull(row2.Mother);
          Aver.AreEqual(row1.Mother.ID, row2.Mother.ID);
          Aver.AreEqual(row1.Mother.Age, row2.Mother.Age);
          Aver.AreEqual(row1.Mother.Str1, row2.Mother.Str1);
          Aver.IsNotNull(row2.Mother.Str2);
          Aver.AreEqual(row1.Mother.Str2, row2.Mother.Str2);
          Aver.AreEqual(row1.Mother.Date, row2.Mother.Date);

          Aver.IsNotNull(row2.Brothers);
          Aver.AreEqual(3, row2.Brothers.Length);
          Aver.AreEqual(111, row2.Brothers[0].Age);
          Aver.AreEqual(222, row2.Brothers[1].Age);
          Aver.AreEqual(333, row2.Brothers[2].Age);
          Aver.IsNotNull(row2.Sisters);
          Aver.AreEqual(2, row2.Sisters.Length);
          Aver.AreEqual(12, row2.Sisters[0].Age);
          Aver.AreEqual(13, row2.Sisters[1].Age);
          Aver.IsNull(row2.Advisers);
      }
    }


    [Run]
    public void SerDeser_FamilyRow_7_ArraysAndList()
    {
      var row1 = new FamilyRow
      {
        ID = new GDID(1,345), Name = "Lalala", RegisteredToVote = true,

        Father = new SimplePersonRow
        {
          Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten", Date = new DateTime(1980, 08, 12, 13, 45, 11)
        },
        Mother = new SimplePersonRow
        {
          Age = 245, Bool1 =false, ID = new GDID(2,12), Name = "Katya Zhaba", Salary=180000, Str1="Snake", Str2="Borra", Date = new DateTime(1911, 01, 01, 14, 11, 07)
        },
        Brothers = new []{ new SimplePersonRow{Age=111}, new SimplePersonRow{Age=222}, new SimplePersonRow{Age=333}},
        Sisters = new []{ new SimplePersonRow{Age=12}, new SimplePersonRow{Age=13}},
        Advisers = new List<SimplePersonRow>{ new SimplePersonRow{Age=101, Name="Kaznatchei"}, new SimplePersonRow{Age=102}}
      };

      var writer = SlimFormat.Instance.GetWritingStreamer();
      var reader = SlimFormat.Instance.GetReadingStreamer();
      using(var ms = new MemoryStream())
      {
          writer.BindStream(ms);
          ArowSerializer.Serialize(row1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var row2 = new FamilyRow();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(row2, reader);
          reader.UnbindStream();

          Aver.AreNotSameRef(row1, row2);
          Aver.AreEqual(row1.ID,    row2.ID);
          Aver.AreEqual(row1.Name, row2.Name);
          Aver.AreEqual(row1.RegisteredToVote, row2.RegisteredToVote);
          Aver.IsNotNull( row2.Father );
          Aver.AreEqual(row1.Father.ID, row2.Father.ID);
          Aver.AreEqual(row1.Father.Age, row2.Father.Age);
          Aver.AreEqual(row1.Father.Str1, row2.Father.Str1);

          Aver.AreEqual(row1.Father.Date, row2.Father.Date);
          Aver.IsNull(row2.Father.Str2);

          Aver.IsNotNull(row2.Mother);
          Aver.AreEqual(row1.Mother.ID, row2.Mother.ID);
          Aver.AreEqual(row1.Mother.Age, row2.Mother.Age);
          Aver.AreEqual(row1.Mother.Str1, row2.Mother.Str1);
          Aver.IsNotNull(row2.Mother.Str2);
          Aver.AreEqual(row1.Mother.Str2, row2.Mother.Str2);
          Aver.AreEqual(row1.Mother.Date, row2.Mother.Date);

          Aver.IsNotNull(row2.Brothers);
          Aver.AreEqual(3, row2.Brothers.Length);
          Aver.AreEqual(111, row2.Brothers[0].Age);
          Aver.AreEqual(222, row2.Brothers[1].Age);
          Aver.AreEqual(333, row2.Brothers[2].Age);
          Aver.IsNotNull(row2.Sisters);
          Aver.AreEqual(2, row2.Sisters.Length);
          Aver.AreEqual(12, row2.Sisters[0].Age);
          Aver.AreEqual(13, row2.Sisters[1].Age);
          Aver.IsNotNull(row2.Advisers);

          Aver.AreEqual(2, row2.Advisers.Count);
          Aver.AreEqual(101, row2.Advisers[0].Age);
          Aver.AreEqual("Kaznatchei", row2.Advisers[0].Name);
          Aver.AreEqual(102, row2.Advisers[1].Age);
      }
    }


  }
}
