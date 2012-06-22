using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using log4net.Core;

namespace log4net.Appender.MongoDB
{
	public class MongoDBAppender : AppenderSkeleton
	{
		private readonly List<MongoAppenderFileld> _fields = new List<MongoAppenderFileld>();

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

		public void AddField(MongoAppenderFileld fileld)
		{
			_fields.Add(fileld);
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			var collection = GetCollection();
			collection.Insert(BuildBsonDocument(loggingEvent));
		}

		protected override void Append(LoggingEvent[] loggingEvents)
		{
			var collection = GetCollection();
			collection.InsertBatch(loggingEvents.Select(BuildBsonDocument));
		}

		private MongoCollection GetCollection()
		{
			MongoUrl url = MongoUrl.Create(ConnectionString);
			MongoServer conn = MongoServer.Create(url);
			MongoDatabase db = conn.GetDatabase(url.DatabaseName ?? "log4net");
			MongoCollection collection = db.GetCollection(CollectionName ?? "logs");
			return collection;
		}

		private BsonDocument BuildBsonDocument(LoggingEvent log)
		{
			var doc = new BsonDocument();
			foreach(MongoAppenderFileld field in _fields)
			{
				object value = field.Layout.Format(log);
				BsonValue bsonValue = BsonValue.Create(value);
				doc.Add(field.Name, bsonValue);
			}
			return doc;
		}
	}
}