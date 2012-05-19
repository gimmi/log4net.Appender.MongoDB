MongoDB appender for log4net
----------------------------

The title says it all. Check [Log4Net site](http://logging.apache.org/log4net/) or [MongoDB site](http://www.mongodb.org/) if you need more info.

Installation
------------

[Get it on NuGet](http://nuget.org/packages/log4net.Appender.MongoDB), or download sources and run build.cmd to build

Appender configuration sample
-----------------------------

	<appender name="MongoDBAppender" type="log4net.Appender.MongoDB.MongoDBAppender, log4net.Appender.MongoDB">
		<!-- See http://www.mongodb.org/display/DOCS/Connections for connectionstring options -->
		<connectionString value="mongodb://localhost" />
		<field>
			<name value="timestamp" />
			<layout type="log4net.Layout.RawTimeStampLayout" />
		</field>
		<field>
			<name value="level" />
			<layout type="log4net.Layout.PatternLayout" value="%level" />
		</field>
		<field>
			<name value="thread" />
			<layout type="log4net.Layout.PatternLayout" value="%thread" />
		</field>
		<field>
			<name value="threadContextProperty" />
			<layout type="log4net.Layout.RawPropertyLayout">
				<key value="threadContextProperty" />
			</layout>
		</field>
		<field>
			<name value="globalContextProperty" />
			<layout type="log4net.Layout.RawPropertyLayout">
				<key value="globalContextProperty" />
			</layout>
		</field>
		<field>
			<name value="numberProperty" />
			<layout type="log4net.Layout.RawPropertyLayout">
				<key value="numberProperty" />
			</layout>
		</field>
		<field>
			<name value="dateProperty" />
			<layout type="log4net.Layout.RawPropertyLayout">
				<key value="dateProperty" />
			</layout>
		</field>
		<field>
			<name value="exception" />
			<layout type="log4net.Layout.ExceptionLayout" />
		</field>
	</appender>

License
-------

[APACHE 2](https://raw.github.com/gimmi/log4net.Appender.MongoDB/master/LICENSE)
