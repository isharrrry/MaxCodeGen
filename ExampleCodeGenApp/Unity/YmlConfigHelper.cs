using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Common
{
    public static class YmlConfigHelper
    {
        #region YmlConfigHelper 
        public static string ConfigSerializeWithTypeMappingSet<T>(T config, IList<Type> WithTypeMappingSet, Action<SerializerBuilder> SerializerBuilderWith = null)
        {
            var sb = new SerializerBuilder();
            sb.TypeTagMapping(WithTypeMappingSet);
            SerializerBuilderWith?.Invoke(sb);
            string txt = "";
            var serializer = sb
                //.WithTypeConverter(new PolymorphicAnimalTypeConverter())
                .Build();
            txt = serializer.Serialize(config);
            return txt;
        }

        public static T ConfigDeserializeWithTypeMappingSet<T>(string SerializeText, IList<Type> WithTypeMappingSet, Action<DeserializerBuilder> DeserializerBuilderWith = null) 
        {
            T obj = default;
            var sb = new DeserializerBuilder();
            sb.TypeTagMapping(WithTypeMappingSet);
            DeserializerBuilderWith?.Invoke(sb);
            var deserializer = sb
                //.WithTypeConverter(new PolymorphicAnimalTypeConverter())
                .Build();
            obj = deserializer.Deserialize<T>(SerializeText);
            return obj;
        }
        public static void TypeTagMapping<TBuilder>(this BuilderSkeleton<TBuilder> sb, IList<Type> tList) where TBuilder : BuilderSkeleton<TBuilder>
        {
            foreach (var t in tList)
            {
                sb.WithTagMapping($"!{t.FullName}", t);
            }
        }
        #endregion

    }
}
