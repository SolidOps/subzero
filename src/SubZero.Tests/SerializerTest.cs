using SolidOps.SubZero;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace SubZero.Tests
{
    [TestClass]
    public class SerializerTest
    {
        public class Simple
        {
            public int IntProp { get; set; }
            public string StrProp { get; set; } = "";
            public decimal DecimalProp { get; set; }
            public Status StatusProp { get; set; }
        }

        public class Nested
        {
            public Simple? SimpleProp { get; set; } = null;
            public List<Simple> SimpleList { get; set; } = [];
            public Dictionary<string, Simple> SimpleDic { get; set; } = [];
        }
        public class NestedWithReadOnlyCollection
        {
            public Simple? SimpleProp { get; set; } = null;

            public IReadOnlyCollection<Simple> SimpleList { get; set; } = [];

            public Dictionary<string, Simple> SimpleDic { get; set; } = new Dictionary<string, Simple>();
        }

        public class CycleNested
        {
            public string Name { get; set; } = "";
            public CycleNested? Parent { get; set; }
            public List<CycleNested> Children { get; set; } = [];
        }


        public enum Status
        {
            Active,
            Inactive,
            Deleted
        }

        [TestMethod]
        public void TestSimpleSerialization()
        {
            var obj = new Simple()
            {
                DecimalProp = 1.5m,
                IntProp = 12,
                StatusProp = Status.Active,
                StrProp = "Test"
            };

            var json = Serializer.Serialize(obj, false);

            Assert.AreEqual("{\"IntProp\":12,\"StrProp\":\"Test\",\"DecimalProp\":1.5,\"StatusProp\":\"Active\"}", json);

            var res = Serializer.Deserialize<Simple>(json, false);

            Assert.AreEqual(obj.DecimalProp, res.DecimalProp);
            Assert.AreEqual(obj.IntProp, res.IntProp);
            Assert.AreEqual(obj.StatusProp, res.StatusProp);
            Assert.AreEqual(obj.StrProp, res.StrProp);

            var lower = json.ToLower();
            lower = lower.Replace("\"test\"", "\"Test\"");
            Assert.AreEqual("{\"intprop\":12,\"strprop\":\"Test\",\"decimalprop\":1.5,\"statusprop\":\"active\"}", lower);

            var altRes = Serializer.Deserialize<Simple>(lower, false);

            Assert.AreEqual(obj.DecimalProp, altRes.DecimalProp);
            Assert.AreEqual(obj.IntProp, altRes.IntProp);
            Assert.AreEqual(obj.StatusProp, altRes.StatusProp);
            Assert.AreEqual(obj.StrProp, altRes.StrProp);

            var json2 = Serializer.Serialize(res, false);
            Assert.AreEqual(json, json2);
        }

        [TestMethod]
        public void TestNestedSerialization()
        {
            var obj = new Nested()
            {
                SimpleDic = new Dictionary<string, Simple>(),
                SimpleList = new List<Simple>()
                {
                    new Simple()
                    {
                        DecimalProp = 1.5m,
                        IntProp = 12,
                        StatusProp = Status.Active,
                        StrProp = "Test"
                    },
                    new Simple()
                    {
                        DecimalProp = 1.7m,
                        IntProp = 56,
                        StatusProp = Status.Deleted,
                        StrProp = "Test2"
                    }
                },
                SimpleProp = new Simple()
                {
                    DecimalProp = 1.9m,
                    IntProp = 25,
                    StatusProp = Status.Inactive,
                    StrProp = "Test3"
                }
            };

            var json = Serializer.Serialize(obj, false);

            Assert.AreEqual("{\"SimpleProp\":{\"IntProp\":25,\"StrProp\":\"Test3\",\"DecimalProp\":1.9,\"StatusProp\":\"Inactive\"},\"SimpleList\":[{\"IntProp\":12,\"StrProp\":\"Test\",\"DecimalProp\":1.5,\"StatusProp\":\"Active\"},{\"IntProp\":56,\"StrProp\":\"Test2\",\"DecimalProp\":1.7,\"StatusProp\":\"Deleted\"}],\"SimpleDic\":{}}", json);

            var res = Serializer.Deserialize<Nested>(json, false);

            var json2 = Serializer.Serialize(res, false);

            Assert.AreEqual(json, json2);
        }



        [TestMethod]
        public void TestNestedWithReadOnlyCollectionSerialization()
        {
            var list = new List<Simple>()
                {
                    new Simple()
                    {
                        DecimalProp = 1.5m,
                        IntProp = 12,
                        StatusProp = Status.Active,
                        StrProp = "Test"
                    },
                    new Simple()
                    {
                        DecimalProp = 1.7m,
                        IntProp = 56,
                        StatusProp = Status.Deleted,
                        StrProp = "Test2"
                    }
                };

            var obj = new NestedWithReadOnlyCollection()
            {
                SimpleDic = new Dictionary<string, Simple>(),
                SimpleList = new ReadOnlyCollection<Simple>(list),
                SimpleProp = new Simple()
                {
                    DecimalProp = 1.9m,
                    IntProp = 25,
                    StatusProp = Status.Inactive,
                    StrProp = "Test3"
                }
            };

            var json = Serializer.Serialize(obj, false);

            Assert.AreEqual("{\"SimpleProp\":{\"IntProp\":25,\"StrProp\":\"Test3\",\"DecimalProp\":1.9,\"StatusProp\":\"Inactive\"},\"SimpleList\":[{\"IntProp\":12,\"StrProp\":\"Test\",\"DecimalProp\":1.5,\"StatusProp\":\"Active\"},{\"IntProp\":56,\"StrProp\":\"Test2\",\"DecimalProp\":1.7,\"StatusProp\":\"Deleted\"}],\"SimpleDic\":{}}", json);

            var res = Serializer.Deserialize<NestedWithReadOnlyCollection>(json, false);

            var json2 = Serializer.Serialize(res, false);

            Assert.AreEqual(json, json2);
        }

        [TestMethod]
        public void TestCycleNestedSerializationFail()
        {
            var child1 = new CycleNested() { Name = "child1", Children = new List<CycleNested>(), Parent = null };
            var child2 = new CycleNested() { Name = "child2", Children = new List<CycleNested>(), Parent = null };
            var parent = new CycleNested() { Name = "parent", Children = new List<CycleNested>() { child1, child2 }, Parent = null };
            var gdParent = new CycleNested() { Name = "gdParent", Children = new List<CycleNested>() { parent }, Parent = null };

            parent.Parent = gdParent;
            child1.Parent = parent;
            child2.Parent = parent;

            bool hasJsonException = false;
            try
            {
                Serializer.Serialize(gdParent, false, false);
            }
            catch (JsonException)
            {
                hasJsonException = true;
            }
            Assert.IsTrue(hasJsonException);
        }

        [TestMethod]
        public void TestCycleNestedDeserializationFail()
        {
            var child1 = new CycleNested() { Name = "child1", Children = new List<CycleNested>(), Parent = null };
            var child2 = new CycleNested() { Name = "child2", Children = new List<CycleNested>(), Parent = null };
            var parent = new CycleNested() { Name = "parent", Children = new List<CycleNested>() { child1, child2 }, Parent = null };
            var gdParent = new CycleNested() { Name = "gdParent", Children = new List<CycleNested>() { parent }, Parent = null };

            parent.Parent = gdParent;
            child1.Parent = parent;
            child2.Parent = parent;

            var json = Serializer.Serialize(gdParent, true);
            Assert.AreEqual("{\"$id\":\"1\",\"Name\":\"gdParent\",\"Parent\":null,\"Children\":{\"$id\":\"2\",\"$values\":[{\"$id\":\"3\",\"Name\":\"parent\",\"Parent\":{\"$ref\":\"1\"},\"Children\":{\"$id\":\"4\",\"$values\":[{\"$id\":\"5\",\"Name\":\"child1\",\"Parent\":{\"$ref\":\"3\"},\"Children\":{\"$id\":\"6\",\"$values\":[]}},{\"$id\":\"7\",\"Name\":\"child2\",\"Parent\":{\"$ref\":\"3\"},\"Children\":{\"$id\":\"8\",\"$values\":[]}}]}}]}}", json);

            bool hasException = false;
            try
            {
                var res = Serializer.Deserialize<CycleNested>(json, false, false);
            }
            catch (JsonException)
            {
                hasException = true;
            }
            Assert.IsTrue(hasException);


        }

        [TestMethod]
        public void TestCycleNestedDeserializationSucceed()
        {
            var child1 = new CycleNested() { Name = "child1", Children = new List<CycleNested>(), Parent = null };
            var child2 = new CycleNested() { Name = "child2", Children = new List<CycleNested>(), Parent = null };
            var parent = new CycleNested() { Name = "parent", Children = new List<CycleNested>() { child1, child2 }, Parent = null };
            var gdParent = new CycleNested() { Name = "gdParent", Children = new List<CycleNested>() { parent }, Parent = null };

            parent.Parent = gdParent;
            child1.Parent = parent;
            child2.Parent = parent;

            var json = Serializer.Serialize(gdParent, true);
            Assert.AreEqual("{\"$id\":\"1\",\"Name\":\"gdParent\",\"Parent\":null,\"Children\":{\"$id\":\"2\",\"$values\":[{\"$id\":\"3\",\"Name\":\"parent\",\"Parent\":{\"$ref\":\"1\"},\"Children\":{\"$id\":\"4\",\"$values\":[{\"$id\":\"5\",\"Name\":\"child1\",\"Parent\":{\"$ref\":\"3\"},\"Children\":{\"$id\":\"6\",\"$values\":[]}},{\"$id\":\"7\",\"Name\":\"child2\",\"Parent\":{\"$ref\":\"3\"},\"Children\":{\"$id\":\"8\",\"$values\":[]}}]}}]}}", json);

            var res = Serializer.Deserialize<CycleNested>(json, true);
            var json2 = Serializer.Serialize(res, true);
            Assert.AreEqual(json, json2);
        }

        [TestMethod]
        public void TestSimpleSerializationWithoutParameter()
        {
            var obj = new Simple()
            {
                DecimalProp = 1.5m,
                IntProp = 12,
                StatusProp = Status.Active,
                StrProp = "Test"
            };

            var json = Serializer.Serialize(obj);

            Assert.AreEqual("{\"IntProp\":12,\"StrProp\":\"Test\",\"DecimalProp\":1.5,\"StatusProp\":\"Active\"}", json);

            var res = Serializer.Deserialize<Simple>(json);

            Assert.AreEqual(obj.DecimalProp, res.DecimalProp);
            Assert.AreEqual(obj.IntProp, res.IntProp);
            Assert.AreEqual(obj.StatusProp, res.StatusProp);
            Assert.AreEqual(obj.StrProp, res.StrProp);

            var lower = json.ToLower();
            lower = lower.Replace("\"test\"", "\"Test\"");
            Assert.AreEqual("{\"intprop\":12,\"strprop\":\"Test\",\"decimalprop\":1.5,\"statusprop\":\"active\"}", lower);

            var altRes = Serializer.Deserialize<Simple>(lower);

            Assert.AreEqual(obj.DecimalProp, altRes.DecimalProp);
            Assert.AreEqual(obj.IntProp, altRes.IntProp);
            Assert.AreEqual(obj.StatusProp, altRes.StatusProp);
            Assert.AreEqual(obj.StrProp, altRes.StrProp);

            var json2 = Serializer.Serialize(res);
            Assert.AreEqual(json, json2);
        }



        [TestMethod]
        public void TestCycleNestedDeserializationSucceedWithoutParameter()
        {
            var child1 = new CycleNested() { Name = "child1", Children = new List<CycleNested>(), Parent = null };
            var child2 = new CycleNested() { Name = "child2", Children = new List<CycleNested>(), Parent = null };
            var parent = new CycleNested() { Name = "parent", Children = new List<CycleNested>() { child1, child2 }, Parent = null };
            var gdParent = new CycleNested() { Name = "gdParent", Children = new List<CycleNested>() { parent }, Parent = null };

            parent.Parent = gdParent;
            child1.Parent = parent;
            child2.Parent = parent;

            var json = Serializer.Serialize(gdParent);
            Assert.AreEqual("{\"$id\":\"1\",\"Name\":\"gdParent\",\"Parent\":null,\"Children\":{\"$id\":\"2\",\"$values\":[{\"$id\":\"3\",\"Name\":\"parent\",\"Parent\":{\"$ref\":\"1\"},\"Children\":{\"$id\":\"4\",\"$values\":[{\"$id\":\"5\",\"Name\":\"child1\",\"Parent\":{\"$ref\":\"3\"},\"Children\":{\"$id\":\"6\",\"$values\":[]}},{\"$id\":\"7\",\"Name\":\"child2\",\"Parent\":{\"$ref\":\"3\"},\"Children\":{\"$id\":\"8\",\"$values\":[]}}]}}]}}", json);

            var res = Serializer.Deserialize<CycleNested>(json);
            var json2 = Serializer.Serialize(res);
            Assert.AreEqual(json, json2);
        }

        private static CycleNested BuildCycle()
        {
            var parent = new CycleNested() { Name = "parent" };
            var child = new CycleNested() { Name = "child", Parent = parent };
            parent.Children.Add(child);
            return parent;
        }

        [TestMethod]
        public void TestCycleRetryKeepsCamelCase()
        {
            var json = Serializer.Serialize(BuildCycle(), camelCase: true);

            Assert.AreEqual("{\"$id\":\"1\",\"name\":\"parent\",\"parent\":null,\"children\":{\"$id\":\"2\",\"$values\":[{\"$id\":\"3\",\"name\":\"child\",\"parent\":{\"$ref\":\"1\"},\"children\":{\"$id\":\"4\",\"$values\":[]}}]}}", json);
        }

        [TestMethod]
        public void TestPreservedPayloadDetectedWhenIndented()
        {
            var options = Serializer.GetOptions(true);
            options.WriteIndented = true;
            var json = Environment.NewLine + "  " + JsonSerializer.Serialize(BuildCycle(), options);

            var res = Serializer.Deserialize<CycleNested>(json);

            Assert.AreSame(res, res.Children.Single().Parent);
        }

        [TestMethod]
        public void TestPreservedPayloadDetectedWithSpacedColon()
        {
            var json = "{ \"$id\" : \"1\", \"Name\" : \"parent\", \"Children\" : { \"$id\" : \"2\", \"$values\" : [ { \"$id\" : \"3\", \"Name\" : \"child\", \"Parent\" : { \"$ref\" : \"1\" } } ] } }";

            var res = Serializer.Deserialize<CycleNested>(json);

            Assert.AreSame(res, res.Children.Single().Parent);
        }

        public class Envelope
        {
            public CycleNested? Data { get; set; }
        }

        [TestMethod]
        public void TestPreservedPayloadDetectedWhenWrapped()
        {
            var json = "{\"Data\":" + Serializer.Serialize(BuildCycle(), true) + "}";

            var res = Serializer.Deserialize<Envelope>(json);

            Assert.AreSame(res.Data, res.Data!.Children.Single().Parent);
        }

        [TestMethod]
        public void TestStringValueEqualToIdDoesNotBreakPlainPayload()
        {
            var res = Serializer.Deserialize<Simple>("{\"IntProp\":3,\"StrProp\":\"$id\"}");

            Assert.AreEqual(3, res.IntProp);
            Assert.AreEqual("$id", res.StrProp);
        }
    }
}