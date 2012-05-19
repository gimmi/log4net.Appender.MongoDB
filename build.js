load('jsmake.dotnet.DotNetUtils.js');

var fs = jsmake.Fs;
var utils = jsmake.Utils;
var sys = jsmake.Sys;
var dotnet = new jsmake.dotnet.DotNetUtils();

var version, assemblyVersion;

task('default', 'build');

task('version', function () {
	version = JSON.parse(fs.readFile('version.json'));
	assemblyVersion = [ version.major, version.minor, version.build, 0 ].join('.');
});

task('dependencies', function () {
	var pkgs = fs.createScanner('src').include('**/packages.config').scan();
	dotnet.downloadNuGetPackages(pkgs, 'lib');
});

task('assemblyinfo', 'version', function () {
	dotnet.writeAssemblyInfo('src/SharedAssemblyInfo.cs', {
		AssemblyTitle: 'log4net.Appender.MongoDB',
		AssemblyProduct: 'log4net.Appender.MongoDB',
		AssemblyDescription: 'MongoDB appender for log4net',
		AssemblyCopyright: 'Copyright © Gian Marco Gherardi ' + new Date().getFullYear(),
		AssemblyTrademark: '',
		AssemblyCompany: 'Gian Marco Gherardi',
		AssemblyConfiguration: '', // Probably a good place to put Git SHA1 and build date
		AssemblyVersion: assemblyVersion,
		AssemblyFileVersion: assemblyVersion,
		AssemblyInformationalVersion: assemblyVersion
	});
});

task('build', [ 'dependencies', 'assemblyinfo' ], function () {
	dotnet.runMSBuild('src/log4net.Appender.MongoDB.sln', [ 'Clean', 'Rebuild' ]);
});

task('test', 'build', function () {
	var testDlls = fs.createScanner('src').include('*.Tests/bin/Debug/*.Tests.dll').scan();
	dotnet.runNUnit(testDlls);
});

task('release', 'test', function () {
	fs.deletePath('build');
	fs.createDirectory('build');
	//dotnet.deployToNuGet('src/log4net.Appender.MongoDB/log4net.Appender.MongoDB.csproj', 'src/log4net.Appender.MongoDB/bin/Debug', true);
	
	sys.run('tools/nuget/nuget.exe', 'pack', 'src\\log4net.Appender.MongoDB\\log4net.Appender.MongoDB.csproj', '-Build', '-OutputDirectory', 'build', '-Symbols');
	sys.run('tools/nuget/nuget.exe', 'push', 'build\\log4net.Appender.MongoDB.' + assemblyVersion + '.nupkg');
	
	version.build += 1;
	fs.writeFile('version.json', JSON.stringify(version));
});

