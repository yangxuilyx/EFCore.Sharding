﻿#if EFCORE3
using EFCore.Sharding.Config;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace EFCore.Sharding
{
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<挂起>")]
    internal class ShardingMigration : MigrationsModelDiffer
    {
        public ShardingMigration(
            IRelationalTypeMappingSource typeMappingSource,
            IMigrationsAnnotationProvider migrationsAnnotations,
            IChangeDetector changeDetector,
            IUpdateAdapterFactory updateAdapterFactory,
            CommandBatchPreparerDependencies commandBatchPreparerDependencies
            )
        : base(typeMappingSource, migrationsAnnotations, changeDetector, updateAdapterFactory, commandBatchPreparerDependencies)
        {
        }

        public override IReadOnlyList<MigrationOperation> GetDifferences(IModel source, IModel target)
        {
            List<MigrationOperation> resList = new List<MigrationOperation>();

            var shardingOption = Cache.ServiceProvider.GetService<IOptions<EFCoreShardingOptions>>().Value;
            var sourceOperations = base.GetDifferences(source, target).ToList();

            //忽略外键
            if (shardingOption.MigrationsWithoutForeignKey)
            {
                sourceOperations.RemoveAll(x => x is AddForeignKeyOperation || x is DropForeignKeyOperation);
                foreach (var operation in sourceOperations.OfType<CreateTableOperation>())
                {
                    operation.ForeignKeys?.Clear();
                }
            }
            resList.AddRange(sourceOperations);

            //分表
            

            return resList;
        }
    }
}

#endif