using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserquakeSimulator.Reader
{
    class MongoDBReader : IReader
    {
        private MongoClient client;
        private IMongoCollection<BsonDocument> collection;

        public MongoDBReader(MongoClient client, IMongoCollection<BsonDocument> collection)
        {
            this.client = client;
            this.collection = collection;
        }

        public IEnumerable<EPSPData> GetAll()
        {
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("code", 555) | builder.Eq("code", 561);

            foreach (BsonDocument document in collection.Find(filter).ToEnumerable())
            {

            }
            throw new NotImplementedException();
        }
    }
}
