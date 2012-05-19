using System;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using SharpTestsEx;
using log4net;

namespace Log4Mongo.Tests
{
	[TestFixture]
	public class MongoAppenderTest
	{
		private MongoCollection _collection;

		[SetUp]
		public void SetUp()
		{
			MongoServer conn = MongoServer.Create("mongodb://localhost");
			MongoDatabase db = conn.GetDatabase("logs");
			_collection = db.GetCollection("logs");
			_collection.RemoveAll();
		}

		[Test]
		public void Should_log_all_events()
		{
			for(int i = 0; i < 1000; i++)
			{
				LogManager.GetLogger(typeof(MongoAppenderTest)).Info(i);
			}
			MongoCursor<BsonDocument> docs = _collection.FindAllAs<BsonDocument>();
			Assert.AreEqual(1000, docs.Count());
		}

		[Test]
		public void Should_log_timestamp()
		{
			LogManager.GetLogger(typeof(MongoAppenderTest)).Info("a log");
			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("timestamp").Value.Should().Be.OfType<BsonDateTime>();
		}

		[Test]
		public void Should_log_custom_properties()
		{
			ThreadContext.Properties["customnumber"] = 123;
			ThreadContext.Properties["customdate"] = new DateTime(2012, 5, 19, 0, 0, 0, DateTimeKind.Utc);
			LogManager.GetLogger(typeof(MongoAppenderTest)).Info("a log");
			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("customnumber").Value.Should().Be.OfType<BsonInt32>();
			doc.GetElement("customnumber").Value.ToInt32().Should().Be.EqualTo(123);
			doc.GetElement("customdate").Value.Should().Be.OfType<BsonDateTime>();
			doc.GetElement("customdate").Value.AsDateTime.Date.Should().Be.EqualTo(new DateTime(2012, 5, 19));
		}
	}
}