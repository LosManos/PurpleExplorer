using System.Collections.ObjectModel;
using Configuration;
using FluentAssertions;
using Newtonsoft.Json;
using PurpleExplorer.Models;

namespace ConfigurationTest;

public class Base64JsonConverterTest
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class MyClass
    {
        public string Ordinary { get; set; } = "";
        [JsonConverter(typeof(Base64JsonConverter))]
        public string Encode { get; set; } = "";
        public MyInnerClass? Inner { get; set; }
    }

    public class MyInnerClass
    {
        public string InnerOrdinary { get; set; } = "";
        [JsonConverter(typeof(Base64JsonConverter))]
        public string InnerEncode { get; set; } = "";
    }
    // ReSharper restore UnusedAutoPropertyAccessor.Global

    [Fact]
    public void Deserializes_and_handles_encoded_properties()
    {
        var jsonString = @"{""Ordinary"":""MyOrdinary"",""Encode"":""TXlFbmNvZGU="",""Inner"":{""InnerOrdinary"":""MyInnerOrdinary"",""InnerEncode"":""TXlJbm5lckVuY29kZQ==""}}";

        var result = JsonConvert.DeserializeObject<MyClass>(jsonString);

        var expected = new MyClass
        {
            Ordinary = "MyOrdinary",
            Encode = "MyEncode",
            Inner = new MyInnerClass
            {
                InnerOrdinary = "MyInnerOrdinary",
                InnerEncode = "MyInnerEncode",
            }
        };

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Serialises_and_handles_encoded_properties()
    {
        var myData = new MyClass
        {
            Ordinary = "MyOrdinary",
            Encode = "MyEncode",
            Inner = new MyInnerClass
            {
                InnerOrdinary = "MyInnerOrdinary",
                InnerEncode = "MyInnerEncode",
            }
        };

        var json = JsonConvert.SerializeObject(myData);

        var expected = JsonConvert.SerializeObject(
            new
            {
                Ordinary = "MyOrdinary",
                Encode = "TXlFbmNvZGU=",
                Inner = new
                {
                    InnerOrdinary="MyInnerOrdinary",
                    InnerEncode="TXlJbm5lckVuY29kZQ=="
                }
            });

        json.Should().Be(expected);
    }

    [Fact]
    public void Encodes_AppState_ConnectionString_When_serialising()
    {
        var data = new AppState
        {
            SavedConnectionStrings = new ObservableCollection<ServiceBusConnectionString>
            {
                new ServiceBusConnectionString
                    { ConnectionString = "MustEncode", Name = "ShouldNotEncode", UseManagedIdentity = true },
            },
        };

        var result = JsonConvert.SerializeObject(data);

        result.Should().BeEquivalentTo(
            @"{""SavedConnectionStrings"":[{""UseManagedIdentity"":true,""ConnectionString"":""TXVzdEVuY29kZQ=="",""Name"":""ShouldNotEncode""}],""SavedMessages"":[],""AppSettings"":{""QueueListFetchCount"":100,""QueueMessageFetchCount"":100,""TopicListFetchCount"":100,""TopicMessageFetchCount"":100}}");
    }
}