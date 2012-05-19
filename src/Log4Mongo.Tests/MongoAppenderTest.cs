using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using log4net;
using System.Linq;
using SharpTestsEx;

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
			var docs = _collection.FindAllAs<BsonDocument>();
			Assert.AreEqual(1000, docs.Count());
		}

		[Test]
		public void Should_log_standard_properties()
		{
			LogManager.GetLogger(typeof(MongoAppenderTest)).Info("a log");
			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("timestamp").Value.Should().Be.OfType<BsonDateTime>();
		}
	}
}