using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace Config.Server.Infrastructure.Persistence.Migrations;

[Migration(version: 1, description: "Initial migration")]
public class Initial : IMigration, IMigrationAssemblyMarker
{
    public void GetUpExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
            create table configurations
            (
                id    bigint primary key generated always as identity,
                key   text unique,
                value text not null
            );
            """,
        });
    }

    public void GetDownExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
            drop table configurations;
            """,
        });
    }

    public string ConnectionString => throw new NotSupportedException();
}
