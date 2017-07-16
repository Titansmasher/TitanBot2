﻿using System;
using System.Linq;
using System.Linq.Expressions;

namespace TitanBot.Settings
{
    public abstract class EditableSetting
    {
        public string Name { get; protected set; }
        public abstract Type Type { get; }
        public abstract bool TrySave(ulong guildId, object value, out string errors);
        public abstract string Display(ulong guildId);

        public static EditableSetting Create<TGroup, TStore, TAccept>(Func<ulong, TGroup> retriever,
                                                                Action<ulong, TGroup> saver,
                                                                string name,
                                                                Expression<Func<TGroup, TStore>> property,
                                                                Func<TAccept, TStore> converter,
                                                                Func<TStore, string> viewer,
                                                                Func<TAccept, string> validator)
            => new TypedSetting<TGroup, TStore, TAccept>(retriever, saver, name, property, converter, viewer, validator);

        private class TypedSetting<TGroup, TStore, TAccept> : EditableSetting
        {
            Func<ulong, TGroup> Retriever { get; }
            Action<ulong, TGroup> Saver { get; }
            Func<TAccept, TStore> Converter { get; }
            Func<TStore, string> Viewer { get; }
            Func<TAccept, string> Validator { get; }
            Action<TGroup, TStore> Setter { get; }
            Func<TGroup, TStore> Getter { get; }

            public override Type Type => typeof(TAccept);

            internal TypedSetting(Func<ulong, TGroup> retriever,
                                  Action<ulong, TGroup> saver,
                                  string name,
                                  Expression<Func<TGroup, TStore>> property,
                                  Func<TAccept, TStore> converter,
                                  Func<TStore, string> viewer,
                                  Func<TAccept, string> validator)
            {
                Name = name;
                Retriever = retriever;
                Setter = CreateSetter(property);
                Getter = property.Compile();
                Converter = converter;
                Viewer = viewer ?? (s => s?.ToString());
                Validator = validator ?? (s => null);
                Saver = saver;
            }

            public override string Display(ulong guildId)
                => Viewer(Getter(Retriever(guildId)));

            public override bool TrySave(ulong guildId, object value, out string errors)
            {
                if (!(value is TAccept))
                    errors = $"`{value.GetType()}` is not a valid {typeof(TAccept)}";
                else
                    errors = Validator((TAccept)value);

                if (errors != null)
                    return false;

                var group = Retriever(guildId);
                Setter(group, Converter((TAccept)value));
                Saver(guildId, group);
                return true;
            }

            private Action<TGroup, TStore> CreateSetter(Expression<Func<TGroup, TStore>> selector)
            {
                var valueParam = Expression.Parameter(typeof(TStore));
                var body = Expression.Assign(selector.Body, valueParam);
                return Expression.Lambda<Action<TGroup, TStore>>(body,
                    selector.Parameters.Single(),
                    valueParam).Compile();
            }
        }
    }
}
