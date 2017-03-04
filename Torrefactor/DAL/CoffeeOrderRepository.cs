using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Torrefactor.Controllers;
using Torrefactor.Models;

namespace Torrefactor.DAL
{
    public class CoffeeOrderRepository : Repository<CoffeeOrder>
    {
        public CoffeeOrderRepository(IMongoDatabase db) : base(db, "coffeeOrders")
        {
            Collection.Indexes
                .CreateOneAsync(
                    Builders<CoffeeOrder>.IndexKeys.Ascending(_ => _.Username),
                    new CreateIndexOptions {Unique = true})
                .Wait();
        }

        public async Task<CoffeeOrder> GetUserOrders(string userName)
        {
            return await Collection.Find(_ => _.Username == userName).SingleOrDefaultAsync();
        }

        public async Task Update(CoffeeOrder userOrder)
        {
            if (userOrder.Version == null)
            {
                await Save(userOrder);
            }
            else
            {
                var query = Builders<CoffeeOrder>.Filter.Eq(_ => _.Username, userOrder.Username) &
                            Builders<CoffeeOrder>.Filter.Eq(_ => _.Version, userOrder.Version);
                userOrder.ChangeVersion();
                var result = await Collection.ReplaceOneAsync(query, userOrder);
                if (result.ModifiedCount == 0)
                {
                    throw new ConcurrencyException($"CoffeeOrder for user: {userOrder.Username}");
                }
            }
        }

        public async Task Save(CoffeeOrder userOrder)
        {
            userOrder.ChangeVersion();
            try
            {
                await Collection.InsertOneAsync(userOrder);
            }
            catch (MongoWriteException mwx)
            {
                if (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    throw new ConcurrencyException($"CoffeeOrder for user: {userOrder.Username}");
                }
            }
        }
    }
}