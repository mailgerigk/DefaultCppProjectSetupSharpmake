using Sharpmake;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public abstract class VendorProject : BaseProject
{
    protected abstract string VendorFolder { get; }

    protected VendorProject()
    {
        AddTargets(BaseConfiguration.Target);

        Name = TypeNameWithoutPostfix;
        SourceRootPath = Path.Combine(BaseConfiguration.VendorDirectory, VendorFolder);
        Debug.Assert(Directory.Exists(SourceRootPath));
        UsePrecompiledHeaders = false;
    }

    public override void Configure(Configuration conf, Target target)
    {
        base.Configure(conf, target);

        conf.Output = Configuration.OutputType.Utility;

        conf.SourceFilesCompileAsCRegex.Clear();
        conf.SourceFilesCompileAsCPPRegex.Clear();

        conf.IncludePaths.Clear();
    }

    public static void AddToConfiguration<T>(Configuration conf, Target target) where T : VendorProject, new()
    {
        var t = new T();
        var sourceRootPath = t.SourceRootPath;

        foreach (var directory in Directory.GetDirectories(sourceRootPath, "include", SearchOption.AllDirectories))
        {
            conf.IncludePaths.Add(directory);
        }

        bool foundLibrariesInPlatformFolder = false;
        foreach (var directory in Directory.GetDirectories(sourceRootPath, "*", SearchOption.AllDirectories))
        {
            if (directory.Contains(target.Platform == Platform.win32 ? "x86" : "x64"))
            {
                foreach (var file in Directory.GetFiles(directory, "*.lib", SearchOption.AllDirectories))
                {
                    conf.LibraryPaths.Add(Path.GetDirectoryName(file));
                    conf.LibraryFiles.Add(Path.GetFileNameWithoutExtension(file));
                    foundLibrariesInPlatformFolder = true;
                }
                foreach (var file in Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories))
                {
                    conf.TargetCopyFiles.Add(file);
                }
            }
        }
        // no libraryPaths found that contain either x86 or x64, try just a lib folder then
        if (!foundLibrariesInPlatformFolder)
        {
            foreach (var directory in Directory.GetDirectories(sourceRootPath, "lib", SearchOption.AllDirectories))
            {
                foreach (var file in Directory.GetFiles(directory, "*.lib", SearchOption.AllDirectories))
                {
                    conf.LibraryPaths.Add(Path.GetDirectoryName(file));
                    conf.LibraryFiles.Add(Path.GetFileNameWithoutExtension(file));
                }
                foreach (var file in Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories))
                {
                    conf.TargetCopyFiles.Add(file);
                }
            }
        }
    }
}