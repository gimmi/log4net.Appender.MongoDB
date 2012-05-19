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
			_collection.Count().Should().Be.EqualTo(1000);
		}

		[Test]
		public void Should_log_timestamp()
		{
			LogManager.GetLogger(typeof(MongoAppenderTest)).Info("a log");
			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("timestamp").Value.Should().Be.OfType<BsonDateTime>();
		}

		[Test]
		public void Should_log_threadcontext_properties()
		{
			ThreadContext.Properties["threadContextProperty"] = "value";

			LogManager.GetLogger("Test").Info("a log");

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("threadContextProperty").Value.AsString.Should().Be.EqualTo("value");
		}

		[Test]
		public void Should_preserve_type_of_properties()
		{
			GlobalContext.Properties["numberProperty"] = 123;
			ThreadContext.Properties["dateProperty"] = DateTime.Now;

			LogManager.GetLogger("Test").Info("a log");
	
			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("numberProperty").Value.Should().Be.OfType<BsonInt32>();
			doc.GetElement("dateProperty").Value.Should().Be.OfType<BsonDateTime>();
		}
	}
}