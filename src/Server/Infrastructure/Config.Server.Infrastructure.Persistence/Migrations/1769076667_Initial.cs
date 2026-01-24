#pragma warning disable SA1649

using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace Config.Server.Infrastructure.Persistence.Migrations;

[Migration(version: 1769076667, description: "Initial migration")]
public class Initial : IMigration, IMigrationAssemblyMarker
{
    public void GetUpExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
            create type config_environment as enum ('dev', 'stage', 'prod', 'global');

            create table configurations
            (
                id          bigint primary key generated always as identity,

                key         text,
                value       text not null,

                namespace   text not null,
                profile     text not null default 'default',
                environment config_environment[] not null,

                created_at  timestamp with time zone not null default now(),
                updated_at  timestamp with time zone not null default now(),
                created_by  text not null,

                is_deleted  bool default false,

                constraint unique_config_record
                    unique (namespace, profile, environment, key)
            );

            create type config_history_kind as enum ('created', 'updated', 'deleted');

            create table configuration_history
            (
                id         bigint primary key generated always as identity,
                config_id  bigint references configurations (id),

                operation  config_history_kind not null,
                old_value  text,
                new_value  text,
                changed_by text not null,
                changed_at timestamp with time zone not null
            );
            """,
        });
    }

    public void GetDownExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
            drop table configuration_history;
            drop table configurations;
            drop type config_history_kind;
            drop type config_environment;
            """,
        });
    }

    public string ConnectionString => throw new NotSupportedException();
}
