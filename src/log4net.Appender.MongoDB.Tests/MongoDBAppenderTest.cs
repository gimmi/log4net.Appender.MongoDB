using System;
using System.IO;
using System.Text;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using SharpTestsEx;
using log4net.Config;

namespace log4net.Appender.MongoDB.Tests
{
	[TestFixture]
	public class MongoDBAppenderTest
	{
		private MongoCollection _collection;
		private ILog _target;

		[SetUp]
		public void SetUp()
		{
			XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
<log4net>
	<appender name='MongoDBAppender' type='log4net.Appender.MongoDB.MongoDBAppender, log4net.Appender.MongoDB'>
		<connectionString value='mongodb://localhost' />
		<field>
			<name value='timestamp' />
			<layout type='log4net.Layout.RawTimeStampLayout' />
		</field>
		<field>
			<name value='level' />
			<layout type='log4net.Layout.PatternLayout' value='%level' />
		</field>
		<field>
			<name value='thread' />
			<layout type='log4net.Layout.PatternLayout' value='%thread' />
		</field>
		<field>
			<name value='threadContextProperty' />
			<layout type='log4net.Layout.RawPropertyLayout'>
				<key value='threadContextProperty' />
			</layout>
		</field>
		<field>
			<name value='globalContextProperty' />
			<layout type='log4net.Layout.RawPropertyLayout'>
				<key value='globalContextProperty' />
			</layout>
		</field>
		<field>
			<name value='numberProperty' />
			<layout type='log4net.Layout.RawPropertyLayout'>
				<key value='numberProperty' />
			</layout>
		</field>
		<field>
			<name value='dateProperty' />
			<layout type='log4net.Layout.RawPropertyLayout'>
				<key value='dateProperty' />
			</layout>
		</field>
		<field>
			<name value='exception' />
			<layout type='log4net.Layout.ExceptionLayout' />
		</field>
	</appender>
	<root>
		<level value='ALL' />
		<appender-ref ref='MongoDBAppender' />
	</root>
</log4net>
")));
			MongoServer conn = MongoServer.Create("mongodb://localhost");
			MongoDatabase db = conn.GetDatabase("log4net");
			_collection = db.GetCollection("logs");
			_collection.RemoveAll();

			_target = LogManager.GetLogger("Test");
		}

		[Test]
		public void Should_log_all_events()
		{
			for(int i = 0; i < 1000; i++)
			{
				_target.Info(i);
			}
			_collection.Count().Should().Be.EqualTo(1000);
		}

		[Test]
		public void Should_log_timestamp()
		{
			_target.Info("a log");

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("timestamp").Value.Should().Be.OfType<BsonDateTime>();
			doc.GetElement("timestamp").Value.AsDateTime.Should().Be.IncludedIn(DateTime.UtcNow.AddSeconds(-1), DateTime.Now);
		}

		[Test]
		public void Should_log_level()
		{
			_target.Info("a log");

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("level").Value.Should().Be.OfType<BsonString>();
			doc.GetElement("level").Value.AsString.Should().Be.EqualTo("INFO");
		}

		[Test]
		public void Should_log_thread()
		{
			_target.Info("a log");

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("thread").Value.Should().Be.OfType<BsonString>();
			doc.GetElement("thread").Value.AsString.Should().Be.EqualTo(Thread.CurrentThread.Name);
		}

		[Test]
		public void Should_log_exception()
		{
			try
			{
				throw new ApplicationException("BOOM");
			}
			catch(Exception e)
			{
				_target.Fatal("a log", e);
			}

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("exception").Value.Should().Be.OfType<BsonString>();
			doc.GetElement("exception").Value.AsString.Should().Contain("ApplicationException: BOOM");
		}

		[Test]
		public void Should_log_threadcontext_properties()
		{
			ThreadContext.Properties["threadContextProperty"] = "value";

			_target.Info("a log");

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("threadContextProperty").Value.AsString.Should().Be.EqualTo("value");
		}

		[Test]
		public void Should_log_globalcontext_properties()
		{
			GlobalContext.Properties["globalContextProperty"] = "value";

			_target.Info("a log");

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("globalContextProperty").Value.AsString.Should().Be.EqualTo("value");
		}

		[Test]
		public void Should_preserve_type_of_properties()
		{
			GlobalContext.Properties["numberProperty"] = 123;
			ThreadContext.Properties["dateProperty"] = DateTime.Now;

			_target.Info("a log");
	
			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("numberProperty").Value.Should().Be.OfType<BsonInt32>();
			doc.GetElement("dateProperty").Value.Should().Be.OfType<BsonDateTime>();
		}
	}
}