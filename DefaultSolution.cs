using Sharpmake;

using System;
using System.Linq;
using System.Reflection;

[module: Include("../*/*.cs")]
[module: Include("../*/*/*.cs")]
[module: Include("../*/*/*/*.cs")]
[module: Include("../*/*/*/*/*.cs")]
[module: Include("../*/*/*/*/*/*.cs")]
[module: Include("../*/*/*/*/*/*/*.cs")]
[module: Include("../*/*/*/*/*/*/*/*.cs")]

[Generate]
public class DefaultSolution : Solution
{
    public DefaultSolution()
    {
        BaseConfiguration.DefaultSolutionSharpmakeCsPath = SharpmakeCsPath;

        Name = BaseConfiguration.SolutionName;
        AddTargets(BaseConfiguration.Target);
    }

    [Configure]
    public virtual void Configure(Configuration conf, Target target)
    {
        conf.SolutionFileName = Name;
        conf.SolutionPath = BaseConfiguration.SharpmakeOutputDirectory;

        var projectTypes = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(BaseProject).IsAssignableFrom(type) && !type.IsAbstract);
        foreach (var projectType in projectTypes)
        {
            if (projectType.GetProperty("CanAdd", BindingFlags.Static | BindingFlags.Public) is PropertyInfo property)
            {
                if (property.GetValue(null) is true)
                {
                    conf.AddProject(projectType, target, solutionFolder: BaseProject.GetSolutionFolder(projectType));
                }
            }
            else
            {
                conf.AddProject(projectType, target, solutionFolder: BaseProject.GetSolutionFolder(projectType));
            }
        }

        conf.SetStartupProject<MainProject>();
    }
}