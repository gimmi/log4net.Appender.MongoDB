using System;
using System.IO;
using System.Security.Principal;
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

		[SetUp]
		public void SetUp()
		{
			GlobalContext.Properties.Clear();
			ThreadContext.Properties.Clear();

			MongoServer conn = MongoServer.Create("mongodb://localhost");
			MongoDatabase db = conn.GetDatabase("log4net");
			_collection = db.GetCollection("logs");
			_collection.RemoveAll();
		}

		private ILog GetConfiguredLog()
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
			return LogManager.GetLogger("Test");
		}

		[Test]
		public void Should_log_all_events()
		{
			var target = GetConfiguredLog();

			for(int i = 0; i < 1000; i++)
			{
				target.Info(i);
			}
			_collection.Count().Should().Be.EqualTo(1000);
		}

		[Test]
		public void Should_log_timestamp()
		{
			var target = GetConfiguredLog();

			target.Info("a log");

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("timestamp").Value.Should().Be.OfType<BsonDateTime>();
			doc.GetElement("timestamp").Value.AsDateTime.Should().Be.IncludedIn(DateTime.UtcNow.AddSeconds(-1), DateTime.Now);
		}

		[Test]
		public void Should_log_level()
		{
			var target = GetConfiguredLog();

			target.Info("a log");

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("level").Value.Should().Be.OfType<BsonString>();
			doc.GetElement("level").Value.AsString.Should().Be.EqualTo("INFO");
		}

		[Test]
		public void Should_log_thread()
		{
			var target = GetConfiguredLog();

			target.Info("a log");

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("thread").Value.Should().Be.OfType<BsonString>();
			doc.GetElement("thread").Value.AsString.Should().Be.EqualTo(Thread.CurrentThread.Name);
		}

		[Test]
		public void Should_log_exception()
		{
			var target = GetConfiguredLog();

			try
			{
				throw new ApplicationException("BOOM");
			}
			catch(Exception e)
			{
				target.Fatal("a log", e);
			}

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("exception").Value.Should().Be.OfType<BsonString>();
			doc.GetElement("exception").Value.AsString.Should().Contain("ApplicationException: BOOM");
		}

		[Test]
		public void Should_log_threadcontext_properties()
		{
			var target = GetConfiguredLog();

			ThreadContext.Properties["threadContextProperty"] = "value";

			target.Info("a log");

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("threadContextProperty").Value.AsString.Should().Be.EqualTo("value");
		}

		[Test]
		public void Should_log_globalcontext_properties()
		{
			var target = GetConfiguredLog();

			GlobalContext.Properties["globalContextProperty"] = "value";

			target.Info("a log");

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("globalContextProperty").Value.AsString.Should().Be.EqualTo("value");
		}

		[Test]
		public void Should_preserve_type_of_properties()
		{
			var target = GetConfiguredLog();

			GlobalContext.Properties["numberProperty"] = 123;
			ThreadContext.Properties["dateProperty"] = DateTime.Now;

			target.Info("a log");
	
			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("numberProperty").Value.Should().Be.OfType<BsonInt32>();
			doc.GetElement("dateProperty").Value.Should().Be.OfType<BsonDateTime>();
		}

		[Test]
		public void Should_log_standard_document_if_no_fields_defined()
		{
			XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
<log4net>
	<appender name='MongoDBAppender' type='log4net.Appender.MongoDB.MongoDBAppender, log4net.Appender.MongoDB'>
		<connectionString value='mongodb://localhost' />
	</appender>
	<root>
		<level value='ALL' />
		<appender-ref ref='MongoDBAppender' />
	</root>
</log4net>
")));
			var target = LogManager.GetLogger("Test");

			GlobalContext.Properties["GlobalContextProperty"] = "GlobalContextValue";
			ThreadContext.Properties["ThreadContextProperty"] = "ThreadContextValue";

			try
			{
				throw new ApplicationException("BOOM");
			}
			catch (Exception e)
			{
				target.Fatal("a log", e);
			}

			var doc = _collection.FindOneAs<BsonDocument>();
			doc.GetElement("timestamp").Value.AsDateTime.Should().Be.IncludedIn(DateTime.UtcNow.AddSeconds(-1), DateTime.Now);
			doc.GetElement("level").Value.AsString.Should().Be.EqualTo("FATAL");
			doc.GetElement("thread").Value.AsString.Should().Be.EqualTo(Thread.CurrentThread.Name);
			doc.GetElement("userName").Value.AsString.Should().Be.EqualTo(WindowsIdentity.GetCurrent().Name);
			doc.GetElement("message").Value.AsString.Should().Be.EqualTo("a log");
			doc.GetElement("loggerName").Value.AsString.Should().Be.EqualTo("Test");
			doc.GetElement("domain").Value.AsString.Should().Be.EqualTo(AppDomain.CurrentDomain.FriendlyName);
			doc.GetElement("machineName").Value.AsString.Should().Be.EqualTo(Environment.MachineName);

			var exception = doc.GetElement("exception").Value.AsBsonDocument;
			exception.GetElement("message").Value.AsString.Should().Be.EqualTo("BOOM");

			var properties = doc.GetElement("properties").Value.AsBsonDocument;
			properties.GetElement("GlobalContextProperty").Value.AsString.Should().Be.EqualTo("GlobalContextValue");
			properties.GetElement("ThreadContextProperty").Value.AsString.Should().Be.EqualTo("ThreadContextValue");
		}
	}
}