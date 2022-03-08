//-----------------------------------------------------------------------
// <copyright file="EditorBuildConfigFileBuidler.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost.BuildConfig
{
    using System.Collections.Generic;
    using System.Text;

    public static class EditorBuildConfigFileBuilder
    {
        public static void GenerateBuildConfigsFile()
        {
            StringBuilder file = new StringBuilder();
            file.AppendLine("// <auto-generated/>              ".TrimEnd());
            file.AppendLine("#pragma warning disable           ".TrimEnd());
            file.AppendLine("                                  ".TrimEnd());
            file.AppendLine("using Lost.BuildConfig;           ".TrimEnd());
            file.AppendLine("using UnityEditor;                ".TrimEnd());
            file.AppendLine("                                  ".TrimEnd());
            file.AppendLine("public static class BuildConfigs  ".TrimEnd());
            file.AppendLine("{                                 ".TrimEnd());
            file.Append(GetConstants());
            file.Append(GetMethods());
            file.AppendLine("}");

            // Writing out to disk and refershing
            FileUtil.CreateOrUpdateFile(file.ToString(), EditorBuildConfigs.BuildConfigsScriptPath);
        }

        private static string GetConstants()
        {
            StringBuilder constants = new StringBuilder();

            foreach (var buildConfig in LostCore.BuildConfigs.BuildConfigs)
            {
                // If we have more than one config, then skip the root config
                if (LostCore.BuildConfigs.BuildConfigs.Count > 1 && buildConfig == LostCore.BuildConfigs.RootBuildConfig)
                {
                    continue;
                }

                StringBuilder constantsBuilder = new StringBuilder();
                constantsBuilder.AppendLine("    private const string Config{config_name}Path = \"{menu_item_name}\";");
                constantsBuilder.AppendLine("    private const string Config{config_name}Guid = \"{config_guid}\";");

                string constantsString = constantsBuilder.ToString()
                    .Replace("{config_name}", buildConfig.SafeName)
                    .Replace("{config_guid}", buildConfig.Id)
                    .Replace("{menu_item_name}", GetMenuItemName(buildConfig));

                constants.AppendLine(constantsString);
            }

            return constants.ToString();
        }

        private static string GetMethods()
        {
            List<string> methods = new List<string>();

            foreach (var buildConfig in LostCore.BuildConfigs.BuildConfigs)
            {
                // If we have more than one config, then skip the root config
                if (LostCore.BuildConfigs.BuildConfigs.Count > 1 && buildConfig == LostCore.BuildConfigs.RootBuildConfig)
                {
                    continue;
                }

                StringBuilder methodBuilder = new StringBuilder();
                methodBuilder.AppendLine("    [MenuItem(Config{config_name}Path, false, 0)]                                                             ".TrimEnd());
                methodBuilder.AppendLine("    public static void Set{config_name}Config()                                                               ".TrimEnd());
                methodBuilder.AppendLine("    {                                                                                                         ".TrimEnd());
                methodBuilder.AppendLine("        EditorBuildConfigs.SetActiveConfig(Config{config_name}Guid);                                          ".TrimEnd());
                methodBuilder.AppendLine("    }                                                                                                         ".TrimEnd());
                methodBuilder.AppendLine("                                                                                                              ".TrimEnd());
                methodBuilder.AppendLine("    [MenuItem(Config{config_name}Path, true, 0)]                                                              ".TrimEnd());
                methodBuilder.AppendLine("    private static bool Set{config_name}ConfigValidate()                                                      ".TrimEnd());
                methodBuilder.AppendLine("    {                                                                                                         ".TrimEnd());
                methodBuilder.AppendLine("        Menu.SetChecked(Config{config_name}Path, EditorBuildConfigs.IsActiveConfig(Config{config_name}Guid)); ".TrimEnd());
                methodBuilder.AppendLine("        return true;                                                                                          ".TrimEnd());
                methodBuilder.AppendLine("    }                                                                                                         ".TrimEnd());

                methods.Add(methodBuilder.ToString().Replace("{config_name}", buildConfig.SafeName));
            }

            // Constructing the final methods string
            StringBuilder methodsBuilder = new StringBuilder();

            for (int i = 0; i < methods.Count; i++)
            {
                if (i != methods.Count - 1)
                {
                    methodsBuilder.AppendLine(methods[i]);
                }
                else
                {
                    methodsBuilder.Append(methods[i]);
                }
            }

            return methodsBuilder.ToString();
        }

        private static string GetSafeBuildConfigName(BuildConfig buildConfig)
        {
            return buildConfig.Parent != null ?
                GetSafeBuildConfigName(buildConfig.Parent) + "/" + buildConfig.SafeName :
                buildConfig.SafeName;
        }

        private static string GetMenuItemName(BuildConfig buildConfig)
        {
            string menuItemName = "Tools/Lost/Configs/" + GetSafeBuildConfigName(buildConfig);

            // Removing the root config from the menu item path if we have more than one configs
            if (LostCore.BuildConfigs.BuildConfigs.Count > 1)
            {
                menuItemName = menuItemName.Replace(LostCore.BuildConfigs.RootBuildConfig.SafeName + "/", string.Empty);
            }

            return menuItemName;
        }
    }
}
