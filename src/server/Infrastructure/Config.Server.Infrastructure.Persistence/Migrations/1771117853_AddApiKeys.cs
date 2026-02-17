#pragma warning disable SA1649

using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace Config.Server.Infrastructure.Persistence.Migrations;

[Migration(version: 1771117853, description: "Add Api Keys migration")]
public class AddApiKeys : IMigration, IMigrationAssemblyMarker
{
    public void GetUpExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
            create table api_keys
            (
                owner_id uuid not null primary key,
                key      text not null
            );
            """,
        });
    }

    public void GetDownExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
            drop table api_keys;
            """,
        });
    }

    public string ConnectionString => throw new NotSupportedException();
}