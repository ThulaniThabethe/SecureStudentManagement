using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using SecureStudentManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureStudentManagement.Services
{
    public class StudentCosmosDbService
    {
        private readonly Container _container;

        public StudentCosmosDbService(IConfiguration config)
        {
            var client = new CosmosClient(
                config["CosmosDatabaseService:EndpointUrl"],
                config["CosmosDatabaseService:PrimaryKey"]);

            var database = client.CreateDatabaseIfNotExistsAsync(
                config["CosmosDatabaseService:DatabaseIdentifier"]).Result;

            _container = database.Database.CreateContainerIfNotExistsAsync(
                config["CosmosDatabaseService:DataContainer"], "/id").Result.Container;
        }

        public async Task AddLearnerAsync(Learner learner)
        {
            await _container.CreateItemAsync(learner, new PartitionKey(learner.id));
        }

        public async Task<Learner> GetLearnerByIdAsync(string id)
        {
            try
            {
                var result = await _container.ReadItemAsync<Learner>(id, new PartitionKey(id));
                return result.Resource;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Learner>> GetPagedLearnersAsync(int page, int pageSize)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.IsDeleted = false");
            var iterator = _container.GetItemQueryIterator<Learner>(query);

            var all = new List<Learner>();

            while (iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync();
                all.AddRange(result);
            }

            return all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<int> GetTotalCountAsync()
        {
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.IsDeleted = false");
            var iterator = _container.GetItemQueryIterator<int>(query);

            var result = await iterator.ReadNextAsync();
            return result.FirstOrDefault();
        }

        public async Task UpdateLearnerAsync(Learner learner)
        {
            await _container.UpsertItemAsync(learner, new PartitionKey(learner.id));
        }

        public async Task SoftDeleteLearnerAsync(string id)
        {
            var learner = await GetLearnerByIdAsync(id);
            if (learner != null)
            {
                learner.IsDeleted = true;
                await UpdateLearnerAsync(learner);
            }
        }
    }
}