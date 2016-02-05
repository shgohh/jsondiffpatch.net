﻿using System;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestFixture]
	public class DiffUnitTests
	{
		[Test]
		public void Diff_EmptyObjects_EmptyPatch()
		{
			var jdp = new JsonDiffPatch();
			var empty = JObject.Parse(@"{}");

			JToken result = jdp.Diff(empty, empty);

			Assert.IsNull(result);
		}

		[Test]
		public void Diff_EqualBooleanProperty_NoDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{""p"": true }");
			var right = JObject.Parse(@"{""p"": true }");

			JToken result = jdp.Diff(left, right);

			Assert.IsNull(result);
		}

		[Test]
		public void Diff_DiffBooleanProperty_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{""p"": true }");
			var right = JObject.Parse(@"{""p"": false }");

			JToken result = jdp.Diff(left, right);
			
			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject) result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(2, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.IsTrue(((JArray)obj.Property("p").Value)[0].ToObject<bool>(), "Array Old Value");
			Assert.IsFalse(((JArray)obj.Property("p").Value)[1].ToObject<bool>(), "Array New Value");
		}

		[Test]
		public void Diff_BooleanPropertyDeleted_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""p"": true }");
			var right = JObject.Parse(@"{ }");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject)result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(3, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.IsTrue(((JArray)obj.Property("p").Value)[0].ToObject<bool>(), "Array Old Value");
			Assert.AreEqual(0, ((JArray)obj.Property("p").Value)[1].ToObject<int>(), "Array New Value");
			Assert.AreEqual(0, ((JArray)obj.Property("p").Value)[2].ToObject<int>(), "Array Deleted Indicator");
		}

		[Test]
		public void Diff_BooleanPropertyAdded_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ }");
			var right = JObject.Parse(@"{ ""p"": true }");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject)result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(1, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.IsTrue(((JArray)obj.Property("p").Value)[0].ToObject<bool>(), "Array Added Value");
		}

		[Test]
		public void Diff_EfficientStringDiff_ValidPatch()
		{
			var jdp = new JsonDiffPatch(new Options { TextDiff = TextDiffMode.Efficient });
			var left = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");
			var right = JObject.Parse(@"{ ""p"": ""blah1"" }");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject)result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(3, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.AreEqual("@@ -1,64 +1,5 @@\n-lp.Value.ToString().Length %3e _options.MinEfficientTextDiffLength\n+blah1\n", ((JArray)obj.Property("p").Value)[0].ToString(), "Array Added Value");
			Assert.AreEqual(0, ((JArray)obj.Property("p").Value)[1].ToObject<int>(), "Array Added Value");
			Assert.AreEqual(2, ((JArray)obj.Property("p").Value)[2].ToObject<int>(), "Array String Diff Indicator");
		}

		[Test]
		public void Diff_EfficientStringDiff_NoChanges()
		{
			var jdp = new JsonDiffPatch(new Options { TextDiff = TextDiffMode.Efficient });
			var left = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");
			var right = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");

			JToken result = jdp.Diff(left, right);

			Assert.IsNull(result, "No Changes");
		}

		[Test]
		public void Diff_LeftNull_Exception()
		{
			var jdp = new JsonDiffPatch();
			var obj = JObject.Parse(@"{ }");

			JToken result = jdp.Diff(null, obj);

			Assert.AreEqual(JTokenType.Array, result.Type);
		}

		[Test]
		public void Diff_RightNull_Exception()
		{
			var jdp = new JsonDiffPatch();
			var obj = JObject.Parse(@"{ }");

			JToken result = jdp.Diff(obj, null);

			Assert.AreEqual(JTokenType.Array, result.Type);
		}

		[Test]
		public void Diff_EfficientArrayDiff_Exception()
		{
			var jdp = new JsonDiffPatch(new Options {ArrayDiff = ArrayDiffMode.Efficient});
			var array = JObject.Parse(@"{ ""p"": [] }");

			Assert.Throws<NotImplementedException>(() => jdp.Diff(array, array));
		}

		[Test]
		public void Diff_IntStringDiff_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JToken.Parse(@"1");
			var right = JToken.Parse(@"""hello""");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Array, result.Type);
			JArray array = (JArray)result;
			Assert.AreEqual(2, array.Count);
			Assert.AreEqual(left, array[0]);
			Assert.AreEqual(right, array[1]);
		}
	}
}
