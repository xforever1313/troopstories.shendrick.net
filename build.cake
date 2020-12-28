string target = Argument( "target", "taste" );

const string pretzelExe = "./_pretzel/src/Pretzel/bin/Debug/netcoreapp3.1/Pretzel.dll";
const string pluginDir = "./_plugins";
const string categoryPlugin = "./_plugins/Pretzel.Categories.dll";
const string extensionPlugin = "./_plugins/Pretzel.SethExtensions.dll";

// ---------------- Tasks ----------------

Task( "taste" )
.Does(
    () =>
    {
        RunPretzel( "taste", false );
    }
).Description( "Calls pretzel taste to try the site locally" );


Task( "generate" )
.Does(
    () =>
    {
        EnsureDirectoryExists( "_site" );
        CleanDirectory( "_site" );
        RunPretzel( "bake", true );
    }
).Description( "Builds the site for publishing." );

Task( "build_pretzel" )
.Does(
    () =>
    {
        BuildPretzel();
    }
).Description( "Compiles Pretzel" );

Task( "build_all" )
.IsDependentOn( "build_pretzel" )
.IsDependentOn( "taste" );

// ---------------- Functions  ----------------

void BuildPretzel()
{
    Information( "Building Pretzel..." );

    DotNetCoreBuildSettings settings = new DotNetCoreBuildSettings
    {
        Configuration = "Debug"
    };

    DotNetCoreBuild( "./_pretzel/src/Pretzel.sln", settings );

    EnsureDirectoryExists( pluginDir );

    // Move Pretzel.Categories.
    {
        FilePathCollection files = GetFiles( "./_pretzel/src/Pretzel.Categories/bin/Debug/netstandard2.1/Pretzel.Categories.*" );
        CopyFiles( files, Directory( pluginDir ) );
    }

    // Move Pretzel.SethExtensions
    {
        FilePathCollection files = GetFiles( "./_pretzel/src/Pretzel.SethExtensions/bin/Debug/netstandard2.1/Pretzel.SethExtensions.*" );
        CopyFiles( files, Directory( pluginDir ) );
    }

    Information( "Building Pretzel... Done!" );
}

void RunPretzel( string argument, bool abortOnFail )
{
    CheckPretzelDependency();

    bool fail = false;
    string onStdOut( string line )
    {
        if( string.IsNullOrWhiteSpace( line ) )
        {
            return line;
        }
        else if( line.StartsWith( "Failed to render template" ) )
        {
            fail = true;
        }

        Console.WriteLine( line );

        return line;
    }

    ProcessSettings settings = new ProcessSettings
    {
        Arguments = ProcessArgumentBuilder.FromString( $"\"{pretzelExe}\" {argument} --debug" ),
        Silent = false,
        RedirectStandardOutput = abortOnFail,
        RedirectedStandardOutputHandler = onStdOut
    };

    int exitCode = StartProcess( "dotnet", settings );
    if( exitCode != 0 )
    {
        throw new Exception( $"Pretzel exited with exit code: {exitCode}" );
    }

    if( abortOnFail && fail )
    {
        throw new Exception( "Failed to render template" );   
    }
}

void CheckPretzelDependency()
{
    if(
        ( FileExists( pretzelExe ) == false ) ||
        ( FileExists( categoryPlugin ) == false ) ||
        ( FileExists( extensionPlugin ) == false )
    )
    {
        BuildPretzel();
    }
}

RunTarget( target );