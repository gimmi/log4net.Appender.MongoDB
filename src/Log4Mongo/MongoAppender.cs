using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using log4net.Appender;
using log4net.Core;

namespace Log4Mongo
{
	public class MongoAppender : IBulkAppender
	{
		/// <summary>
		/// MongoDB database connection in the format:
		/// mongodb://[username:password@]host1[:port1][,host2[:port2],...[,hostN[:portN]]][/[database][?options]]
		/// See http://www.mongodb.org/display/DOCS/Connections
		/// If no database specified, default to "logs"
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// Name of the collection in database
		/// Defaults to "logs"
		/// </summary>
		public string CollectionName { get; set; }

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
			MongoDatabase db = conn.GetDatabase(url.DatabaseName ?? "logs");
			MongoCollection collection = db.GetCollection(CollectionName ?? "logs");

			collection.InsertBatch(logs.Select(BuildBsonDocument));
		}

		private BsonDocument BuildBsonDocument(LoggingEvent log)
		{
			return new BsonDocument {
				{ "timestamp", new BsonDateTime(log.TimeStamp) },
				{ "level", new BsonString(log.Level.ToString()) },
			};
		}
	}
}