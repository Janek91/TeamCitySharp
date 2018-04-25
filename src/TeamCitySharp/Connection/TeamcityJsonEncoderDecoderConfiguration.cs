using EasyHttp.Codecs;
using EasyHttp.Codecs.JsonFXExtensions;
using EasyHttp.Configuration;
using JsonFx.Json;
using JsonFx.Model;
using JsonFx.Serialization;
using System.Collections.Generic;
using System.IO;

namespace TeamCitySharp.Connection
{
    public class TeamcityJsonEncoderDecoderConfiguration : IEncoderDecoderConfiguration
    {
        public IEncoder GetEncoder()
        {
            CamelCaseJsonWriter jsonWriter =
              new CamelCaseJsonWriter(new DataWriterSettings(DefaultEncoderDecoderConfiguration.CombinedResolverStrategy()
                                                             , new TeamCityDateFilter()), "application/.*json", "text/.*json");

            List<IDataWriter> writers = new List<IDataWriter> { jsonWriter };
            RegExBasedDataWriterProvider dataWriterProvider = new RegExBasedDataWriterProvider(new List<IDataWriter> { jsonWriter });
            return new DefaultEncoder(dataWriterProvider);
        }

        public IDecoder GetDecoder()
        {
            JsonReader jsonReader =
              new JsonReader(new DataReaderSettings(DefaultEncoderDecoderConfiguration.CombinedResolverStrategy()
                                                    , new TeamCityDateFilter()), "application/.*json", "text/.*json");

            List<IDataReader> readers = new List<IDataReader> { jsonReader };
            RegExBasedDataReaderProvider dataReaderProvider = new RegExBasedDataReaderProvider(readers);
            return new DefaultDecoder(dataReaderProvider);
        }
    }

    public class CamelCaseJsonWriter : JsonWriter
    {
        public CamelCaseJsonWriter(DataWriterSettings settings, params string[] contentTypes)
          : base(settings, contentTypes)
        {
        }

        protected override ITextFormatter<ModelTokenType> GetFormatter()
        {
            return new CamelCaseJsonFormatter(Settings);
        }
    }

    public class CamelCaseJsonFormatter : JsonWriter.JsonFormatter
    {
        public CamelCaseJsonFormatter(DataWriterSettings settings)
          : base(settings)
        {
        }

        protected override void WritePropertyName(TextWriter writer, string propertyName)
        {
            base.WritePropertyName(writer, CamelCase(propertyName));
        }

        private static string CamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            char[] chars = input.ToCharArray();
            chars[0] = chars[0].ToString().ToLower().ToCharArray()[0];

            return new string(chars);
        }
    }
}
