using NUnit.Framework;
using log4net.Config;

namespace Log4Mongo.Tests
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