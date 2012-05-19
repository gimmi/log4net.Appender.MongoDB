using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using log4net.Appender;
using log4net.Core;

namespace log4net.Appender.MongoDB
{
	public class MongoDBAppender : IBulkAppender
	{
		/// <summary>
		/// MongoDB database connection in the format:
		/// mongodb://[username:password@]host1[:port1][,host2[:port2],...[,hostN[:portN]]][/[database][?options]]
		/// See http://www.mongodb.org/display/DOCS/Connections
		/// If no database specified, default to "log4net"
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// Name of the collection in database
		/// Defaults to "logs"
		/// </summary>
		public string CollectionName { get; set; }

		private readonly List<MongoAppenderFileld> _fields = new List<MongoAppenderFileld>();

		public void AddField(MongoAppenderFileld fileld)
		{
			_fields.Add(fileld);
		}

		public string Name { get; set; }

		public void Close() {}

		public void DoAppend(LoggingEvent loggingEvent)
		{
			DoAppend(new[] { loggingEvent });
		}

		public void DoAppend(LoggingEvent[] logs)
		{
			MongoUrl url = MongoUrl.Create(ConnectionString);
			MongoServer conn = MongoServer.Create(url);
			MongoDatabase db = conn.GetDatabase(url.DatabaseName ?? "log4net");
			MongoCollection collection = db.GetCollection(CollectionName ?? "logs");

			collection.InsertBatch(logs.Select(BuildBsonDocument));
		}

		private BsonDocument BuildBsonDocument(LoggingEvent log)
		{
			var doc = new BsonDocument();
			doc.Add("1", "2");
			foreach(var field in _fields)
			{
				var lookupProperty = log.LookupProperty("TimeStamp");
				var format = field.Layout.Format(log);
				var value = BsonValue.Create(format);
				doc.Add(field.Name, value);
			}
			return doc;
		}
	}
}