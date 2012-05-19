using NUnit.Framework;
using log4net.Config;

namespace log4net.Appender.MongoDB.Tests
{
	[SetUpFixture]
	public class SetUpFixture
	{
		[SetUp]
		public void SetUp()
		{
			XmlConfigurator.Configure();
		}
	}
}