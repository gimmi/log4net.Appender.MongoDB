using NUnit.Framework;
using log4net.Config;
using log4net.Util;

namespace log4net.Appender.MongoDB.Tests
{
	[SetUpFixture]
	public class SetUpFixture
	{
		[SetUp]
		public void SetUp()
		{
			//LogLog.InternalDebugging = true; // For capturing log4net internal logging. See http://logging.apache.org/log4net/release/faq.html
			XmlConfigurator.Configure();
		}
	}
}