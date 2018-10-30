using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserquakeSimulator.Reader
{
    class MongoDBReader : IReader, IVerifier
    {
        private MongoClient client;
        private IMongoCollection<BsonDocument> collection;

        public MongoDBReader(MongoClient client, IMongoCollection<BsonDocument> collection)
        {
            this.client = client;
            this.collection = collection;
        }

        public int AccuracySeconds { private get; set; } = 120;

        public IEnumerable<EPSPData> GetAll()
        {
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("code", 555) | builder.Eq("code", 561);

            foreach (BsonDocument document in collection.Find(filter).ToEnumerable())
            {
                var epspData = new EPSPData()
                {
                    DataType = (document["code"].AsInt32 == 555) ? DataType.Areapeer : DataType.Userquake,
                    AreaCode = (document["code"].AsInt32 == 561) ? document["area"].AsInt32.ToString("D3") : "",
                    ArrivalTime = DateTime.Parse(document["time"].AsString),
                    PeerMap = (document["code"].AsInt32 == 555) ? ExtractPeerMap(document) : null
                };
                yield return epspData;
            }
        }

        public bool HaveEarthquake(DateTime uqBegin, DateTime uqEnd, string[] prefecture)
        {
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("code", 551) &
                builder.Gte("earthquake.time", uqBegin.AddSeconds(-AccuracySeconds).ToString("yyyy/MM/dd HH:mm:ss")) &
                builder.Lte("earthquake.time", uqEnd.AddSeconds(AccuracySeconds).ToString("yyyy/MM/dd HH:mm:ss")) &
                builder.In("points.pref", prefecture);

            return collection.Find(filter).CountDocuments() > 0;
        }

        private IDictionary<string, int> ExtractPeerMap(BsonDocument document)
        {
            var dic = new Dictionary<string, int>();
            var areas = document["areas"].AsBsonArray;
            return areas.ToDictionary(
                area => area["id"].AsInt32.ToString("D3"),
                area => area["peer"].AsInt32
            );
        }
    }
}
