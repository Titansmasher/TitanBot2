﻿using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using TitanBot.Commands.DefaultCommands.Abstract;
using TitanBot.Settings;
using TitanBot.TypeReaders;

namespace TitanBot.Commands.DefaultCommands.Admin
{
    [Description(TitanBotResource.SETTINGS_HELP_DESCRIPTION)]
    [DefaultPermission(8)]
    [Alias("Setting")]
    [RequireContext(ContextType.Guild)]
    public class SettingsCommand : SettingCommand
    {
        protected override IReadOnlyList<ISettingEditorCollection> Settings => SettingsManager.GetEditors(SettingScope.Guild);
        protected override IEntity<ulong> SettingContext => Guild;

        public SettingsCommand(ITypeReaderCollection readers)
            : base(readers) { }

        [Call]
        [Usage(TitanBotResource.SETTINGS_HELP_USAGE_DEFAULT)]
        new Task ListSettingsAsync([Dense]string settingGroup = null)
            => base.ListSettingsAsync(settingGroup);

        [Call("Toggle")]
        [Usage(TitanBotResource.SETTINGS_HELP_USAGE_TOGGLE)]
        new Task ToggleSettingAsync(string key)
            => base.ToggleSettingAsync(key);

        [Call("Set")]
        [Usage(TitanBotResource.SETTINGS_HELP_USAGE_SET)]
        new Task SetSettingAsync(string key, [Dense]string value = null)
            => base.SetSettingAsync(key, value);
    }
}
