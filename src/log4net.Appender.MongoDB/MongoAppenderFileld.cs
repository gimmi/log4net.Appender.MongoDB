using log4net.Layout;

namespace log4net.Appender.MongoDB
{
	public class MongoAppenderFileld
	{
		public string Name { get; set; }
		public IRawLayout Layout { get; set; }
	}
}