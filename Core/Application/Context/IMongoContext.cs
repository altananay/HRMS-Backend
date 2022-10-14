using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Context
{
    public interface IMongoContext
    {
        MongoClient connection { get; }
        IMongoDatabase database { get; }
    }
}
